# =============================================================================
# AI API — Multi-stage Production Dockerfile
# Build from repo root: docker build -f services/ai-api/Dockerfile .
# =============================================================================

# Stage 1: Restore dependencies (layer cached on .csproj changes only)
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS restore
WORKDIR /src

COPY services/ai-api/src/SaasTemplate.AiApi.Domain/SaasTemplate.AiApi.Domain.csproj services/ai-api/src/SaasTemplate.AiApi.Domain/
COPY services/ai-api/src/SaasTemplate.AiApi.Application/SaasTemplate.AiApi.Application.csproj services/ai-api/src/SaasTemplate.AiApi.Application/
COPY services/ai-api/src/SaasTemplate.AiApi.Infrastructure/SaasTemplate.AiApi.Infrastructure.csproj services/ai-api/src/SaasTemplate.AiApi.Infrastructure/
COPY services/ai-api/src/SaasTemplate.AiApi.Api/SaasTemplate.AiApi.Api.csproj services/ai-api/src/SaasTemplate.AiApi.Api/

RUN dotnet restore services/ai-api/src/SaasTemplate.AiApi.Api/SaasTemplate.AiApi.Api.csproj

# Stage 2: Build and publish
FROM restore AS build
COPY services/ai-api/src/ services/ai-api/src/
RUN dotnet publish services/ai-api/src/SaasTemplate.AiApi.Api/SaasTemplate.AiApi.Api.csproj \
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

ENTRYPOINT ["dotnet", "SaasTemplate.AiApi.Api.dll"]
