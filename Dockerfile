# ── Build ────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar solo los .csproj primero (para cachear el restore)
COPY Backend/Domain/Finanzas.Domain.csproj           Backend/Domain/
COPY Backend/Application/Finanzas.Application.csproj Backend/Application/
COPY Backend/Infrastructure/Finanzas.Infrastructure.csproj Backend/Infrastructure/
COPY Backend/API/Finanzas.API.csproj                 Backend/API/

RUN dotnet restore Backend/API/Finanzas.API.csproj

# Copiar el resto del código y publicar
COPY Backend/ Backend/
RUN dotnet publish Backend/API/Finanzas.API.csproj -c Release -o /app/publish

# ── Runtime ───────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Fuentes necesarias para QuestPDF (generación de PDF)
RUN apt-get update && apt-get install -y --no-install-recommends \
    fontconfig fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Render asigna el puerto via $PORT (por defecto 10000)
ENV ASPNETCORE_URLS=http://+:${PORT:-10000}
EXPOSE 10000

ENTRYPOINT ["dotnet", "Finanzas.API.dll"]
