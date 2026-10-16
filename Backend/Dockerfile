# ──────────────────────────────────────────────────────────────────
# NMU Shuttle API — multi-stage build
# Build context must be the Backend/ folder (where this Dockerfile lives).
# ──────────────────────────────────────────────────────────────────

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore first (layer-cached when only source changes)
COPY NmuShuttle.Api/NmuShuttle.Api.csproj NmuShuttle.Api/
RUN dotnet restore NmuShuttle.Api/NmuShuttle.Api.csproj

# Copy the rest of the API source and publish
COPY NmuShuttle.Api/ NmuShuttle.Api/
RUN dotnet publish NmuShuttle.Api/NmuShuttle.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Listen on the port Fly.io (and most container hosts) expect.
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "NmuShuttle.Api.dll"]
