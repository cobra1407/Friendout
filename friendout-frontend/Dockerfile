# ─── Step 1: Build React ────────────────────────────────────────────────────────────────────────────────────────────────
FROM node:22-alpine AS build
WORKDIR /app

# Install pnpm (project uses pnpm-lock.yaml)
RUN npm install -g pnpm@10.12.4

# Copy manifests and install dependencies
# --frozen-lockfile: ensures build reproducibility
# approve-builds   : authorizes native scripts for esbuild and @parcel/watcher
#                    (necessary for Vite to work correctly)
COPY package.json pnpm-lock.yaml ./
# onlyBuiltDependencies in package.json allows esbuild and @parcel/watcher
# to run their native scripts without interaction.
RUN pnpm install --frozen-lockfile

# Copy the rest of the source code and build
COPY . .
RUN pnpm run build

# ─── Step 2: Serve with Nginx ──────────────────────────────────────────────────────────────────────────────────────────
FROM nginx:1.27-alpine AS final

# Copy the built files
COPY --from=build /app/dist /usr/share/nginx/html

# Copy the custom Nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
