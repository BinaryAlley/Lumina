# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 5214

# ==== Stage 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# restore NuGet packages (keeping this section separate from the rest of the project files allows Docker to cache the restored packages)
COPY ["AssemblyInfo_Shared.cs", "."]
COPY ["Lumina.Presentation.Api/Lumina.Presentation.Api.csproj", "Lumina.Presentation.Api/"]
COPY ["Lumina.Application/Lumina.Application.csproj", "Lumina.Application/"]
COPY ["Lumina.Domain/Lumina.Domain.csproj", "Lumina.Domain/"]
COPY ["Lumina.Infrastructure/Lumina.Infrastructure.csproj", "Lumina.Infrastructure/"]
COPY ["Lumina.DataAccess/Lumina.DataAccess.csproj", "Lumina.DataAccess/"]

RUN dotnet restore "./Lumina.Presentation.Api/Lumina.Presentation.Api.csproj" /p:BaseOutputPath=/app/publish/

# build the project
COPY ["Lumina.Presentation.Api/", "Lumina.Presentation.Api/"]
COPY ["Lumina.Application/", "Lumina.Application/"]
COPY ["Lumina.Domain/", "Lumina.Domain/"]
COPY ["Lumina.Infrastructure/", "Lumina.Infrastructure/"]
COPY ["Lumina.DataAccess/", "Lumina.DataAccess/"]
COPY . .
WORKDIR /src/Lumina.Presentation.Api

RUN dotnet build "./Lumina.Presentation.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build /p:BaseOutputPath=/app/publish/

# ==== Stage 2: Publish stage
FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "./Lumina.Presentation.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:BaseOutputPath=/app/publish/

# ==== Stage 3: Run stage
#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .
ENV ASPNETCORE_HTTP_PORTS=5214
EXPOSE 5214
ENTRYPOINT ["dotnet", "Lumina.Presentation.Api.dll"]

# to build this Docker image, run the following command from the root of the project (location of the .sln file):
# docker build --no-cache -t lumina-server -f Lumina.Presentation.Api/Dockerfile .