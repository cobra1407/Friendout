# ─── Step 1: Build React ────────────────────────────────────────────────────────────────────────────────────────────────
FROM node:22-alpine AS build
WORKDIR /app

# Copy manifests and install dependencies
# --frozen-lockfile: ensures build reproducibility
# approve-builds   : authorizes native scripts for esbuild and @parcel/watcher
#                    (necessary for Vite to work correctly)
COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./

# Install the exact pnpm version declared in the "packageManager" field
# (strips an optional "+sha512..." suffix)
RUN npm install -g "pnpm@$(node -p "require('./package.json').packageManager.split('@')[1].split('+')[0]")"

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
