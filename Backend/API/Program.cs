using Finanzas.API;
using Finanzas.Application.Interfaces;
using Finanzas.Application.Mappings;
using QuestPDF.Infrastructure;
using Finanzas.Application.Services;
using Finanzas.Domain.Entities;
using Finanzas.Infrastructure.Data;
using Finanzas.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Npgsql v6+ exige DateTime con Kind=Utc para timestamp with time zone.
// Este switch acepta cualquier Kind, igual que v5.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// DbContext - PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repositorios
builder.Services.AddScoped<IGastosRepository, GastosRepository>();
builder.Services.AddScoped<IDeudaRepository, DeudaRepository>();
builder.Services.AddScoped<ICuentaRepository, CuentaRepository>();
builder.Services.AddScoped<IIngresosRepository, IngresosRepository>();
builder.Services.AddScoped<ICategoriasRepository, CategoriasRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPresupuestosRepository, PresupuestosRepository>();
builder.Services.AddScoped<IMetasRepository, MetasRepository>();

// Servicios
builder.Services.AddScoped<GastosService>();
builder.Services.AddScoped<IngresosService>();
builder.Services.AddScoped<CategoriasService>();
builder.Services.AddScoped<ResumenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PresupuestosService>();
builder.Services.AddScoped<MetasService>();
builder.Services.AddScoped<DeudaService>();
builder.Services.AddScoped<CuentaService>();
builder.Services.AddScoped<ReporteService>();
builder.Services.AddScoped<RecordatoriosService>();
builder.Services.AddScoped<WhatsAppService>();
builder.Services.AddHttpClient<EmailService>();
builder.Services.AddScoped<InsightsService>();
builder.Services.AddHttpClient<GroqService>();

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// CORS - permite localhost en dev y cualquier subdominio de vercel.app en producción
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueApp", policy =>
        policy.SetIsOriginAllowed(origin =>
            origin == "http://localhost:5173" ||
            new Uri(origin).Host.EndsWith(".vercel.app") ||
            origin == (builder.Configuration["FRONTEND_URL"] ?? ""))
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("VueApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Endpoint de keep-alive: evita el cold start de Render Y el auto-suspend de Neon
// (Neon en el plan free suspende el cómputo tras ~5 min sin actividad en la BD,
// independiente de si el proceso de Render está despierto). Por eso SÍ debe tocar
// la base de datos con una consulta mínima, no solo responder desde el proceso.
app.MapGet("/health", async (AppDbContext db) =>
{
    await db.Database.ExecuteSqlRawAsync("SELECT 1");
    return Results.Ok("OK");
});

// Crear/migrar esquema al arrancar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Si las tablas aún tienen PK entero, las eliminamos y EnsureCreated las recrea con UUID.
    // Este bloque se ejecuta una sola vez: cuando el Id sea de tipo integer.

    db.Database.EnsureCreated();

    // EnsureCreated() no agrega tablas nuevas si la base de datos ya existía de antes
    // (solo crea el esquema completo en bases de datos nuevas). Por eso las tablas
    // agregadas después del primer arranque se crean manualmente aquí.
    // Todo en un solo ExecuteSqlRaw: cada llamada es un round-trip a Postgres y esto
    // corre en cada cold start de Render, así que menos round-trips = arranque más rápido.
    db.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS "Abonos" (
            "Id" uuid NOT NULL PRIMARY KEY,
            "DeudaId" uuid NOT NULL,
            "Monto" numeric(12,2) NOT NULL,
            "Fecha" timestamp with time zone NOT NULL,
            CONSTRAINT "FK_Abonos_Deudas_DeudaId" FOREIGN KEY ("DeudaId") REFERENCES "Deudas" ("Id") ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS "Cuentas" (
            "Id" uuid NOT NULL PRIMARY KEY,
            "UserId" uuid NOT NULL,
            "Nombre" character varying(100) NOT NULL,
            "Tipo" text NOT NULL,
            "Activa" boolean NOT NULL,
            "CreadoEn" timestamp with time zone NOT NULL,
            CONSTRAINT "FK_Cuentas_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
        );
        CREATE TABLE IF NOT EXISTS "MovimientosCuenta" (
            "Id" uuid NOT NULL PRIMARY KEY,
            "CuentaId" uuid NOT NULL,
            "Monto" numeric(12,2) NOT NULL,
            "Descripcion" character varying(150),
            "Fecha" timestamp with time zone NOT NULL,
            CONSTRAINT "FK_MovimientosCuenta_Cuentas_CuentaId" FOREIGN KEY ("CuentaId") REFERENCES "Cuentas" ("Id") ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS "MovimientosMeta" (
            "Id" uuid NOT NULL PRIMARY KEY,
            "MetaId" uuid NOT NULL,
            "Monto" numeric(12,2) NOT NULL,
            "Fecha" timestamp with time zone NOT NULL,
            CONSTRAINT "FK_MovimientosMeta_Metas_MetaId" FOREIGN KEY ("MetaId") REFERENCES "Metas" ("Id") ON DELETE CASCADE
        );

        ALTER TABLE "Gastos" ADD COLUMN IF NOT EXISTS "CuentaId" uuid NULL;
        ALTER TABLE "Ingresos" ADD COLUMN IF NOT EXISTS "CuentaId" uuid NULL;
        ALTER TABLE "Ingresos" ADD COLUMN IF NOT EXISTS "Frecuencia" text NULL;
        ALTER TABLE "Ingresos" ADD COLUMN IF NOT EXISTS "DiaPago" integer NULL;
        ALTER TABLE "Ingresos" ADD COLUMN IF NOT EXISTS "DeudaId" uuid NULL;
        ALTER TABLE "Gastos" ADD COLUMN IF NOT EXISTS "Fecha" timestamp with time zone NULL;
        ALTER TABLE "Gastos" ADD COLUMN IF NOT EXISTS "DeudaId" uuid NULL;
        ALTER TABLE "Categorias" DROP COLUMN IF EXISTS "Tipo";
        ALTER TABLE "Metas" ADD COLUMN IF NOT EXISTS "CuentaId" uuid NULL;
        ALTER TABLE "MovimientosMeta" ADD COLUMN IF NOT EXISTS "Descripcion" character varying(200) NULL;
        ALTER TABLE "Gastos"   ADD COLUMN IF NOT EXISTS "ActualizadoEn" timestamp with time zone NULL;
        ALTER TABLE "Ingresos" ADD COLUMN IF NOT EXISTS "ActualizadoEn" timestamp with time zone NULL;
        ALTER TABLE "Cuentas"  ADD COLUMN IF NOT EXISTS "ActualizadoEn" timestamp with time zone NULL;
        ALTER TABLE "Metas"    ADD COLUMN IF NOT EXISTS "ActualizadoEn" timestamp with time zone NULL;
        ALTER TABLE "Deudas"   ADD COLUMN IF NOT EXISTS "ActualizadoEn" timestamp with time zone NULL;
        ALTER TABLE "Deudas"   ADD COLUMN IF NOT EXISTS "CuentaId" uuid NULL;
        ALTER TABLE "MovimientosCuenta" ADD COLUMN IF NOT EXISTS "DeudaId" uuid NULL;
        ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "RecordatoriosActivos" boolean NOT NULL DEFAULT TRUE;
        ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "UltimoRecordatorioActividadEn" timestamp with time zone NULL;
        ALTER TABLE "Users" DROP COLUMN IF EXISTS "Telefono";
        ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "WhatsAppId" character varying(64) NULL;
        ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "WhatsAppCodigo" character varying(6) NULL;
        ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "WhatsAppCodigoExpira" timestamp with time zone NULL;
        CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_WhatsAppId" ON "Users" ("WhatsAppId") WHERE "WhatsAppId" IS NOT NULL;

        DO $mig$
        BEGIN
            IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Gastos_Cuentas_CuentaId') THEN
                ALTER TABLE "Gastos" ADD CONSTRAINT "FK_Gastos_Cuentas_CuentaId" FOREIGN KEY ("CuentaId") REFERENCES "Cuentas" ("Id") ON DELETE SET NULL;
            END IF;
            IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Ingresos_Cuentas_CuentaId') THEN
                ALTER TABLE "Ingresos" ADD CONSTRAINT "FK_Ingresos_Cuentas_CuentaId" FOREIGN KEY ("CuentaId") REFERENCES "Cuentas" ("Id") ON DELETE SET NULL;
            END IF;
            IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Metas_Cuentas_CuentaId') THEN
                ALTER TABLE "Metas" ADD CONSTRAINT "FK_Metas_Cuentas_CuentaId" FOREIGN KEY ("CuentaId") REFERENCES "Cuentas" ("Id") ON DELETE SET NULL;
            END IF;
            IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Deudas_Cuentas_CuentaId') THEN
                ALTER TABLE "Deudas" ADD CONSTRAINT "FK_Deudas_Cuentas_CuentaId" FOREIGN KEY ("CuentaId") REFERENCES "Cuentas" ("Id") ON DELETE SET NULL;
            END IF;
        END
        $mig$;
        """);
}

app.Run();
