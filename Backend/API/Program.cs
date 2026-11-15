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
builder.Services.AddScoped<BusquedaService>();
builder.Services.AddHttpClient<EmailService>();
builder.Services.AddScoped<InsightsService>();
builder.Services.AddHttpClient<GroqService>();
builder.Services.AddScoped<CopilotoService>();

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// JWT Authentication
builder.Services.AddScoped<AutomatizacionService>();
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
        ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "RecurrentesCopiadosHasta" integer NULL;
        ALTER TABLE "Gastos" ADD COLUMN IF NOT EXISTS "GeneradoAuto" boolean NOT NULL DEFAULT FALSE;
        
        """);
}

app.Run();
