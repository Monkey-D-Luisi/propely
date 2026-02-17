# =============================================================================
# Orgs API — Multi-stage Production Dockerfile
# Build from repo root: docker build -f services/orgs-api/Dockerfile .
# =============================================================================

# Stage 1: Restore dependencies (layer cached on .csproj changes only)
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS restore
WORKDIR /src

COPY services/orgs-api/src/SaasTemplate.OrgsApi.Domain/SaasTemplate.OrgsApi.Domain.csproj services/orgs-api/src/SaasTemplate.OrgsApi.Domain/
COPY services/orgs-api/src/SaasTemplate.OrgsApi.Application/SaasTemplate.OrgsApi.Application.csproj services/orgs-api/src/SaasTemplate.OrgsApi.Application/
COPY services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/SaasTemplate.OrgsApi.Infrastructure.csproj services/orgs-api/src/SaasTemplate.OrgsApi.Infrastructure/
COPY services/orgs-api/src/SaasTemplate.OrgsApi.Api/SaasTemplate.OrgsApi.Api.csproj services/orgs-api/src/SaasTemplate.OrgsApi.Api/

RUN dotnet restore services/orgs-api/src/SaasTemplate.OrgsApi.Api/SaasTemplate.OrgsApi.Api.csproj

# Stage 2: Build and publish
FROM restore AS build
COPY services/orgs-api/src/ services/orgs-api/src/
RUN dotnet publish services/orgs-api/src/SaasTemplate.OrgsApi.Api/SaasTemplate.OrgsApi.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime

RUN addgroup -S appgroup && adduser -S appuser -G appgroup

WORKDIR /app
COPY --from=build /app/publish .

USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "SaasTemplate.OrgsApi.Api.dll"]
