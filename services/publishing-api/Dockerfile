# =============================================================================
# Properties API — Multi-stage Production Dockerfile
# Build from repo root: docker build -f services/publishing-api/Dockerfile .
# =============================================================================

# Stage 1: Restore dependencies (layer cached on .csproj changes only)
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS restore
WORKDIR /src

COPY services/publishing-api/src/Propely.PublishingApi.Domain/Propely.PublishingApi.Domain.csproj services/publishing-api/src/Propely.PublishingApi.Domain/
COPY services/publishing-api/src/Propely.PublishingApi.Application/Propely.PublishingApi.Application.csproj services/publishing-api/src/Propely.PublishingApi.Application/
COPY services/publishing-api/src/Propely.PublishingApi.Infrastructure/Propely.PublishingApi.Infrastructure.csproj services/publishing-api/src/Propely.PublishingApi.Infrastructure/
COPY services/publishing-api/src/Propely.PublishingApi.Api/Propely.PublishingApi.Api.csproj services/publishing-api/src/Propely.PublishingApi.Api/

RUN dotnet restore services/publishing-api/src/Propely.PublishingApi.Api/Propely.PublishingApi.Api.csproj

# Stage 2: Build and publish
FROM restore AS build
COPY services/publishing-api/src/ services/publishing-api/src/
RUN dotnet publish services/publishing-api/src/Propely.PublishingApi.Api/Propely.PublishingApi.Api.csproj \
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

ENTRYPOINT ["dotnet", "Propely.PublishingApi.Api.dll"]
