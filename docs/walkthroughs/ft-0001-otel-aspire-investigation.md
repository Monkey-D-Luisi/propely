# ft-0001: Fix OpenTelemetry Aspire Dashboard Connectivity

## Goal

Resolve the issue where `orgs-api` telemetry traces were not appearing in the Aspire Dashboard.

## Changes

### 1. Fix Docker Compose Port Mapping
The root cause was a mismatch between the Docker port mapping and the ports the Aspire Dashboard container listens on for OTLP.

- **Before:** Maps `4317:4317` (Aspire listens on 18889 for gRPC OTLP)
- **After:** Maps `4317:18889` (gRPC) and `4318:18890` (HTTP)

```yaml
# docker-compose.yml
    ports:
      - "127.0.0.1:18888:18888"   # Dashboard UI
      - "127.0.0.1:4317:18889"    # OTLP gRPC (external 4317 -> internal 18889)
      - "127.0.0.1:4318:18890"    # OTLP HTTP (external 4318 -> internal 18890)
```

## Verification

### 1. Verify Port Mapping
Run `docker ps` to confirm the ports are correctly mapped:
```bash
docker ps --filter "name=aspire"
# Should show: 127.0.0.1:4317->18889/tcp
```

### 2. Verify Telemetry Connectivity
1. Ensure `ORGSAPI_OTEL_ENDPOINT=http://localhost:4317` is set.
2. Run the API: `.\scripts\run-orgs-api.ps1`
3. Hit endpoints (e.g., `http://localhost:5020/health/live`) to generate traffic.
4. Check Aspire Dashboard at `http://localhost:18888` (Traces tab).
5. Verify no connection errors in API logs.

## Conclusion
The application code and `TelemetryConfiguration.cs` were correct. The issue was solely infrastructure configuration in `docker-compose.yml`.
