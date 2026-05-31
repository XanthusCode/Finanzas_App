using Finanzas.Application.Mappings;
using Finanzas.Application.Interfaces;
using Finanzas.Application.Services;
using Finanzas.Infrastructure.Data;
using Finanzas.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext - MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repositorios
builder.Services.AddScoped<IGastosRepository, GastosRepository>();
builder.Services.AddScoped<IIngresosRepository, IngresosRepository>();
builder.Services.AddScoped<ICategoriasRepository, CategoriasRepository>();

// Servicios
builder.Services.AddScoped<GastosService>();
builder.Services.AddScoped<IngresosService>();
builder.Services.AddScoped<CategoriasService>();
builder.Services.AddScoped<ResumenService>();

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// CORS - permite llamadas desde Vue
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueApp", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("VueApp");
app.UseAuthorization();
app.MapControllers();

// Crear tablas automaticamente si no existen
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();