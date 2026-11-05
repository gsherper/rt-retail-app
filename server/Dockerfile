# Builds and runs Dashboard.Api. In compose.yml the ui container proxies /hub
# to this one on port 5001; the data is in memory, so it resets on restart.

# --- Build ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, so this layer is cached until the project's packages change
COPY src/Dashboard.Api/Dashboard.Api.csproj src/Dashboard.Api/
RUN dotnet restore src/Dashboard.Api/Dashboard.Api.csproj

COPY src/ src/
RUN dotnet publish src/Dashboard.Api/Dashboard.Api.csproj \
    --configuration Release --no-restore --output /app

# --- Run ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

# Listen on all interfaces on 5001 (launchSettings.json isn't used here)
ENV ASPNETCORE_HTTP_PORTS=5001
EXPOSE 5001

# Run as the image's built-in non-root user
USER $APP_UID

ENTRYPOINT ["dotnet", "Dashboard.Api.dll"]
