# =============================================================================
# Properties API — Multi-stage Production Dockerfile
# Build from repo root: docker build -f services/properties-api/Dockerfile .
# =============================================================================

# Stage 1: Restore dependencies (layer cached on .csproj changes only)
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS restore
WORKDIR /src

COPY services/properties-api/src/Propely.PropertiesApi.Domain/Propely.PropertiesApi.Domain.csproj services/properties-api/src/Propely.PropertiesApi.Domain/
COPY services/properties-api/src/Propely.PropertiesApi.Application/Propely.PropertiesApi.Application.csproj services/properties-api/src/Propely.PropertiesApi.Application/
COPY services/properties-api/src/Propely.PropertiesApi.Infrastructure/Propely.PropertiesApi.Infrastructure.csproj services/properties-api/src/Propely.PropertiesApi.Infrastructure/
COPY services/properties-api/src/Propely.PropertiesApi.Api/Propely.PropertiesApi.Api.csproj services/properties-api/src/Propely.PropertiesApi.Api/

RUN dotnet restore services/properties-api/src/Propely.PropertiesApi.Api/Propely.PropertiesApi.Api.csproj

# Stage 2: Build and publish
FROM restore AS build
COPY services/properties-api/src/ services/properties-api/src/
RUN dotnet publish services/properties-api/src/Propely.PropertiesApi.Api/Propely.PropertiesApi.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
RUN apk add --no-cache krb5-libs
RUN addgroup -S appgroup && adduser -S appuser -G appgroup

WORKDIR /app
COPY --from=build /app/publish .

USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Propely.PropertiesApi.Api.dll"]
