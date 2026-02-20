# =============================================================================
# Properties API — Multi-stage Production Dockerfile
# Build from repo root: docker build -f services/contacts-api/Dockerfile .
# =============================================================================

# Stage 1: Restore dependencies (layer cached on .csproj changes only)
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS restore
WORKDIR /src

COPY services/contacts-api/src/Propely.ContactsApi.Domain/Propely.ContactsApi.Domain.csproj services/contacts-api/src/Propely.ContactsApi.Domain/
COPY services/contacts-api/src/Propely.ContactsApi.Application/Propely.ContactsApi.Application.csproj services/contacts-api/src/Propely.ContactsApi.Application/
COPY services/contacts-api/src/Propely.ContactsApi.Infrastructure/Propely.ContactsApi.Infrastructure.csproj services/contacts-api/src/Propely.ContactsApi.Infrastructure/
COPY services/contacts-api/src/Propely.ContactsApi.Api/Propely.ContactsApi.Api.csproj services/contacts-api/src/Propely.ContactsApi.Api/

RUN dotnet restore services/contacts-api/src/Propely.ContactsApi.Api/Propely.ContactsApi.Api.csproj

# Stage 2: Build and publish
FROM restore AS build
COPY services/contacts-api/src/ services/contacts-api/src/
RUN dotnet publish services/contacts-api/src/Propely.ContactsApi.Api/Propely.ContactsApi.Api.csproj \
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

ENTRYPOINT ["dotnet", "Propely.ContactsApi.Api.dll"]
