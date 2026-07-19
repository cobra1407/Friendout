FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Friendout.API/Friendout.API.csproj                       Friendout.API/
COPY Friendout.Domain/Friendout.Domain.csproj                 Friendout.Domain/
COPY Friendout.Infrastructure/Friendout.Infrastructure.csproj Friendout.Infrastructure/

RUN dotnet restore Friendout.API/Friendout.API.csproj

# Copy all source code
COPY . .

# Publish in Release mode
RUN dotnet publish Friendout.API/Friendout.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

    FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# curl is used by the Docker healthcheck (docker-compose.yml) to poll /api/health.
# Not included in the base aspnet image, so we install it explicitly.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Create the uploads folder (mounted volume in production)
RUN mkdir -p /app/uploads

COPY --from=build /app/publish .

# Port exposed by Kestrel
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
# Default value for Dockerfile — overridden by docker-compose.yml
# (Development locally, Production if HTTPS is available).
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Friendout.API.dll"]
