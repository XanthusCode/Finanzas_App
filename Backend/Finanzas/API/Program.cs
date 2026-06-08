using System.Text;
using Finanzas.Application.Interfaces;
using Finanzas.Application.Mappings;
using Finanzas.Application.Services;
using Finanzas.Infrastructure.Data;
using Finanzas.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// DbContext - PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repositorios
builder.Services.AddScoped<IGastosRepository, GastosRepository>();
builder.Services.AddScoped<IIngresosRepository, IngresosRepository>();
builder.Services.AddScoped<ICategoriasRepository, CategoriasRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Servicios
builder.Services.AddScoped<GastosService>();
builder.Services.AddScoped<IngresosService>();
builder.Services.AddScoped<CategoriasService>();
builder.Services.AddScoped<ResumenService>();
builder.Services.AddScoped<AuthService>();

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

// CORS - permite llamadas desde Vue
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueApp", policy =>
        policy.WithOrigins("http://localhost:5173")
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

// Crear/migrar esquema al arrancar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Si las tablas aún tienen PK entero, las eliminamos y EnsureCreated las recrea con UUID.
    // Este bloque se ejecuta una sola vez: cuando el Id sea de tipo integer.
    db.Database.ExecuteSqlRaw(@"
        DO $$
        BEGIN
            IF EXISTS (
                SELECT 1 FROM information_schema.columns
                WHERE table_name = 'Gastos' AND column_name = 'Id' AND data_type = 'integer'
            ) THEN
                DROP TABLE IF EXISTS ""Gastos"";
                DROP TABLE IF EXISTS ""Ingresos"";
                DROP TABLE IF EXISTS ""Categorias"";
                DROP TABLE IF EXISTS ""Users"";
            END IF;
        END $$;
    ");

    db.Database.EnsureCreated();
}

app.Run();
