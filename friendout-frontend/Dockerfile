# ─── Étape 1 : Build React ─────────────────────────────────────────────────
FROM node:22-alpine AS build
WORKDIR /app

# Installer pnpm (ton projet utilise pnpm-lock.yaml)
RUN npm install -g pnpm

# Copier les manifestes et installer les dépendances
COPY package.json pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

# Copier le reste du code source et builder
COPY . .
RUN pnpm run build

# ─── Étape 2 : Servir avec Nginx ───────────────────────────────────────────
FROM nginx:1.27-alpine AS final

# Copier les fichiers buildés
COPY --from=build /app/dist /usr/share/nginx/html

# Copier la config Nginx custom
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
