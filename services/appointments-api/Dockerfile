# =============================================================================
# Appointments API — Multi-stage Production Dockerfile
# Build from repo root: docker build -f services/appointments-api/Dockerfile .
# =============================================================================

# Stage 1: Restore dependencies (layer cached on .csproj changes only)
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS restore
WORKDIR /src

COPY services/appointments-api/src/Propely.AppointmentsApi.Domain/Propely.AppointmentsApi.Domain.csproj services/appointments-api/src/Propely.AppointmentsApi.Domain/
COPY services/appointments-api/src/Propely.AppointmentsApi.Application/Propely.AppointmentsApi.Application.csproj services/appointments-api/src/Propely.AppointmentsApi.Application/
COPY services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/Propely.AppointmentsApi.Infrastructure.csproj services/appointments-api/src/Propely.AppointmentsApi.Infrastructure/
COPY services/appointments-api/src/Propely.AppointmentsApi.Api/Propely.AppointmentsApi.Api.csproj services/appointments-api/src/Propely.AppointmentsApi.Api/

RUN dotnet restore services/appointments-api/src/Propely.AppointmentsApi.Api/Propely.AppointmentsApi.Api.csproj

# Stage 2: Build and publish
FROM restore AS build
COPY services/appointments-api/src/ services/appointments-api/src/
RUN dotnet publish services/appointments-api/src/Propely.AppointmentsApi.Api/Propely.AppointmentsApi.Api.csproj \
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

ENTRYPOINT ["dotnet", "Propely.AppointmentsApi.Api.dll"]
