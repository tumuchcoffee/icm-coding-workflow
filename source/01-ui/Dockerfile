# source/01-ui/Dockerfile
# Angular 19 SPA — multi-stage Docker build
# ADR-011: Multi-stage builds for all custom images
# ADR-012: nginx for production serving

# ── Stage 1: Build ──────────────────────────────────────────────
FROM node:22-alpine AS build
WORKDIR /app

# Copy package manifests first for layer caching
COPY source/01-ui/package.json source/01-ui/package-lock.json* ./
RUN npm ci --ignore-scripts

# Copy Angular source and build
COPY source/01-ui/ .
RUN npm run build -- --configuration production

# ── Stage 2: Runtime (nginx) — default target ───────────────────
FROM nginx:alpine AS runtime
# ADR-012: nginx for Angular SPA serving

# Remove default nginx config
RUN rm /etc/nginx/conf.d/default.conf

# Copy custom nginx configuration
COPY source/01-ui/nginx.conf /etc/nginx/conf.d/default.conf

# Copy built Angular app from build stage
# Angular 19 with outputPath=dist produces browser output at dist/browser/
COPY --from=build /app/dist/browser /usr/share/nginx/html

EXPOSE 80
HEALTHCHECK --interval=10s --timeout=5s --retries=3 \
  CMD wget -qO- http://localhost:80/ || exit 1

# ── Stage 3: Development (Angular dev server) — optional target ──
FROM node:22-alpine AS development
# ADR-015: Dev profile for hot reload
WORKDIR /app

# Install Angular CLI globally
RUN npm install -g @angular/cli@19

# Copy package manifests from UI source directory and install
COPY source/01-ui/package.json source/01-ui/package-lock.json* ./
RUN npm ci

# Source is mounted as a volume at runtime — not copied
EXPOSE 4200
CMD ["ng", "serve", "--host", "0.0.0.0", "--port", "4200", "--poll", "2000"]