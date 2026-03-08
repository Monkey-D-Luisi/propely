# =============================================================================
# AI API — Multi-stage Production Dockerfile
# Build from repo root: docker build -f services/ai-api/Dockerfile .
# =============================================================================

# Stage 1: Restore dependencies (layer cached on .csproj changes only)
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS restore
WORKDIR /src

COPY services/ai-api/src/Propely.AiApi.Domain/Propely.AiApi.Domain.csproj services/ai-api/src/Propely.AiApi.Domain/
COPY services/ai-api/src/Propely.AiApi.Application/Propely.AiApi.Application.csproj services/ai-api/src/Propely.AiApi.Application/
COPY services/ai-api/src/Propely.AiApi.Infrastructure/Propely.AiApi.Infrastructure.csproj services/ai-api/src/Propely.AiApi.Infrastructure/
COPY services/ai-api/src/Propely.AiApi.Api/Propely.AiApi.Api.csproj services/ai-api/src/Propely.AiApi.Api/

# SDK client project files referenced by ai-api (required for dotnet restore)
COPY services/properties-api/src/Propely.PropertiesApi.Client/Propely.PropertiesApi.Client.csproj services/properties-api/src/Propely.PropertiesApi.Client/
COPY services/contacts-api/src/Propely.ContactsApi.Client/Propely.ContactsApi.Client.csproj services/contacts-api/src/Propely.ContactsApi.Client/
COPY services/appointments-api/src/Propely.AppointmentsApi.Client/Propely.AppointmentsApi.Client.csproj services/appointments-api/src/Propely.AppointmentsApi.Client/

# Shared project referenced by all SDK clients
COPY services/shared/Propely.Shared.Http/Propely.Shared.Http.csproj services/shared/Propely.Shared.Http/

RUN dotnet restore services/ai-api/src/Propely.AiApi.Api/Propely.AiApi.Api.csproj

# Stage 2: Build and publish
FROM restore AS build
COPY services/ai-api/src/ services/ai-api/src/

# SDK client source files (referenced as ProjectReferences from ai-api)
COPY services/properties-api/src/Propely.PropertiesApi.Client/ services/properties-api/src/Propely.PropertiesApi.Client/
COPY services/contacts-api/src/Propely.ContactsApi.Client/ services/contacts-api/src/Propely.ContactsApi.Client/
COPY services/appointments-api/src/Propely.AppointmentsApi.Client/ services/appointments-api/src/Propely.AppointmentsApi.Client/
COPY services/shared/Propely.Shared.Http/ services/shared/Propely.Shared.Http/

RUN dotnet publish services/ai-api/src/Propely.AiApi.Api/Propely.AiApi.Api.csproj \
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

ENTRYPOINT ["dotnet", "Propely.AiApi.Api.dll"]
