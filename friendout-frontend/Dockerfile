# ─── Step 1: Build React ────────────────────────────────────────────────────────────────────────────────────────────────
FROM node:22-alpine AS build
WORKDIR /app

# Copy manifests and install dependencies
# --frozen-lockfile: ensures build reproducibility
# approve-builds   : authorizes native scripts for esbuild and @parcel/watcher
#                    (necessary for Vite to work correctly)
COPY package.json pnpm-lock.yaml ./

# Enable corepack and prepare pnpm using the version from package.json
RUN corepack enable && corepack prepare --activate

# onlyBuiltDependencies in package.json allows esbuild and @parcel/watcher
# to run their native scripts without interaction.
RUN pnpm install --frozen-lockfile

# Copy the rest of the source code and build
COPY . .
RUN pnpm run build

# ─── Step 2: Serve with Nginx ──────────────────────────────────────────────────────────────────────────────────────────
FROM nginxinc/nginx-unprivileged:1.30-alpine AS final

# Copy the built files
COPY --from=build /app/dist /usr/share/nginx/html

# Copy the custom Nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Unprivileged nginx image: runs as the "nginx" user (UID 101) and listens on 8080
# instead of 80, since a non-root process cannot bind ports below 1024.
EXPOSE 8080

USER nginx

CMD ["nginx", "-g", "daemon off;"]
