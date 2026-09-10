# Build multi-stage: la imagen final solo contiene el runtime de ASP.NET Core
# y los binarios publicados, no el SDK completo (imagen más chica y segura).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Se copian primero los .csproj para aprovechar el cache de capas de Docker:
# si no cambiaron las dependencias, no hace falta repetir el restore.
COPY Clinica.Dominio/*.csproj Clinica.Dominio/
COPY Clinica.Aplicacion/*.csproj Clinica.Aplicacion/
COPY Clinica.Infraestructura/*.csproj Clinica.Infraestructura/
COPY Clinica.API/*.csproj Clinica.API/
RUN dotnet restore Clinica.API/Clinica.API.csproj

COPY . .
RUN dotnet publish Clinica.API/Clinica.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Muchos servicios de hosting (Render, Railway, etc.) asignan el puerto
# mediante la variable de entorno PORT; se usa con un valor por defecto para
# poder correr el contenedor igual en entornos que no la definen.
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["/bin/sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet Clinica.API.dll"]
