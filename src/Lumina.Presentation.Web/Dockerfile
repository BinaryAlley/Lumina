# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
# curl is used by the container healthcheck to probe the /health endpoints
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir /logs && chown -R app:app /logs
USER app
WORKDIR /app
EXPOSE 5012


# ==== Stage 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# restore NuGet packages (keeping this section separate from the rest of the project files allows Docker to cache the restored packages)
COPY ["AssemblyInfo_Shared.cs", "."]
COPY ["Lumina.Presentation.Web/Lumina.Presentation.Web.csproj", "Lumina.Presentation.Web/"]

RUN dotnet restore "./Lumina.Presentation.Web/Lumina.Presentation.Web.csproj" /p:BaseOutputPath=/app/publish/

# build the project
COPY ["Lumina.Presentation.Web/", "Lumina.Presentation.Web/"]
COPY . .
WORKDIR /src/Lumina.Presentation.Web

RUN dotnet build "./Lumina.Presentation.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build /p:BaseOutputPath=/app/publish/

# ==== Stage 2: Publish stage
FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "./Lumina.Presentation.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:BaseOutputPath=/app/publish/

# ==== Stage 3: Run stage
#FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .
# The HTTP port is controlled per run mode: launchSettings.json for local development, ASPNETCORE_HTTP_PORTS here for
# Docker, and for a published DLL run directly on a host, ASPNETCORE_URLS (or ASPNETCORE_HTTP_PORTS) must be set
ENV ASPNETCORE_HTTP_PORTS=5012
EXPOSE 5012
ENTRYPOINT ["dotnet", "Lumina.Presentation.Web.dll"]

# to build this Docker image, run the following command from the root of the project (location of the .sln file):
# docker build --no-cache -t lumina-web -f Lumina.Presentation.Web/Dockerfile .
