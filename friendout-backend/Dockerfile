# ─── Étape 1 : Build ───────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copier les .csproj de chaque couche et restaurer les dépendances
COPY friendout-backend.sln ./
COPY Friendout.API/Friendout.API.csproj                       Friendout.API/
COPY Friendout.Domain/Friendout.Domain.csproj                 Friendout.Domain/
COPY Friendout.Infrastructure/Friendout.Infrastructure.csproj Friendout.Infrastructure/

RUN dotnet restore friendout-backend.sln

# Copier tout le code source
COPY . .

# Publier en mode Release
RUN dotnet publish Friendout.API/Friendout.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ─── Étape 2 : Runtime (image légère, sans SDK) ────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Créer le dossier uploads (volume monté en prod)
RUN mkdir -p /app/uploads

COPY --from=build /app/publish .

# Port exposé par Kestrel
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Friendout.API.dll"]
