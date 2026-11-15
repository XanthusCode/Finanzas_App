using Finanzas.API;
using Finanzas.Application.Interfaces;
using Finanzas.Application.Mappings;
using QuestPDF.Infrastructure;
using Finanzas.Application.Services;
using Finanzas.Domain.Entities;
using Finanzas.Infrastructure.Data;
using Finanzas.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

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
builder.Services.AddScoped<AutomatizacionService>();

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

// CORS - solo orígenes exactos: el frontend de producción, FRONTEND_URL y localhost en dev.
// Antes se aceptaba cualquier *.vercel.app, y cualquiera puede desplegar ahí.
// Para un preview concreto de Vercel: agregarlo en CORS_ORIGINS (separados por coma).
var origenesPermitidos = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "https://finanzas-app-ashy-one.vercel.app",
};
if (builder.Environment.IsDevelopment())
    origenesPermitidos.Add("http://localhost:5173");
foreach (var o in $"{builder.Configuration["FRONTEND_URL"]},{builder.Configuration["CORS_ORIGINS"]}"
             .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    origenesPermitidos.Add(o.TrimEnd('/'));

builder.Services.AddCors(options =>
{
    options.AddPolicy("VueApp", policy =>
        policy.WithOrigins([.. origenesPermitidos])
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// IP real del cliente detrás del proxy de Render: se toma solo la última IP de
// X-Forwarded-For (la que agrega Render), así el cliente no puede falsificarla.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Rate limiting: frena fuerza bruta en login/registro y el abuso de los endpoints de IA
// (que consumen el cupo de Groq de toda la app).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (ctx, ct) =>
    {
        ctx.HttpContext.Response.ContentType = "application/json";
        await ctx.HttpContext.Response.WriteAsync(
            """{"message":"Demasiados intentos. Espera un momento e intenta de nuevo."}""", ct);
    };

    // Autenticación: 5 intentos por minuto por IP
    options.AddPolicy("auth", http => RateLimitPartition.GetFixedWindowLimiter(
        http.Connection.RemoteIpAddress?.ToString() ?? "sin-ip",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));

    // IA: 20 llamadas por hora por usuario (o por IP si no hay sesión)
    options.AddPolicy("ia", http => RateLimitPartition.GetFixedWindowLimiter(
        http.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? http.Connection.RemoteIpAddress?.ToString() ?? "sin-ip",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromHours(1), QueueLimit = 0 }));
});

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Datos inválidos → { message } (mismo formato que el resto de la API), con el
        // primer error en español. El frontend muestra response.data.message.
        options.InvalidModelStateResponseFactory = ctx =>
        {
            var primero = ctx.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));
            return new BadRequestObjectResult(new { message = primero ?? "Revisa los datos enviados." });
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

// Headers de seguridad en todas las respuestas de la API
app.Use(async (ctx, next) =>
{
    var h = ctx.Response.Headers;
    h["X-Content-Type-Options"] = "nosniff";
    h["X-Frame-Options"] = "DENY";
    h["Referrer-Policy"] = "no-referrer";
    await next();
});

app.UseCors("VueApp");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter(); // después de la autenticación: la política "ia" particiona por usuario
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
