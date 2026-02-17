# =============================================================================
# Web App (Next.js) — Multi-stage Production Dockerfile
# Build from repo root: docker build -f apps/web/Dockerfile .
# =============================================================================

# Stage 1: Install dependencies
FROM node:22-alpine AS deps
WORKDIR /app

COPY apps/web/package.json apps/web/package-lock.json ./
RUN npm ci --ignore-scripts

# Stage 2: Build the application
FROM node:22-alpine AS builder
WORKDIR /app

# NEXT_PUBLIC_* vars are inlined by Next.js at build time, so they must be
# available as env vars during `npm run build`. Pass them via --build-arg.
ARG NEXT_PUBLIC_WEB_URL=http://localhost:3000
ARG NEXT_PUBLIC_AI_API_URL=http://localhost:5010
ARG NEXT_PUBLIC_ORGS_API_URL=http://localhost:5020

ENV NEXT_PUBLIC_WEB_URL=$NEXT_PUBLIC_WEB_URL
ENV NEXT_PUBLIC_AI_API_URL=$NEXT_PUBLIC_AI_API_URL
ENV NEXT_PUBLIC_ORGS_API_URL=$NEXT_PUBLIC_ORGS_API_URL
ENV NEXT_TELEMETRY_DISABLED=1

COPY --from=deps /app/node_modules ./node_modules
COPY apps/web/ .

RUN npm run build

# Stage 3: Production runner
FROM node:22-alpine AS runner
WORKDIR /app

ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1
ENV HOSTNAME=0.0.0.0
ENV PORT=3000

RUN addgroup -S appgroup && adduser -S appuser -G appgroup

# Remove npm/npx (not needed at runtime) to reduce image size and eliminate
# npm-bundled vulnerabilities (e.g., node-tar CVEs) flagged by Trivy.
RUN npm cache clean --force && rm -rf /usr/local/lib/node_modules /usr/local/bin/npm /usr/local/bin/npx

COPY --from=builder /app/public ./public
COPY --from=builder --chown=appuser:appgroup /app/.next/standalone ./
COPY --from=builder --chown=appuser:appgroup /app/.next/static ./.next/static

USER appuser

EXPOSE 3000

CMD ["node", "server.js"]
