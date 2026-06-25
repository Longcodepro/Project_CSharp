# src/TuneVault.API/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY TuneVault.sln .
COPY TuneVault.Domain/TuneVault.Domain.csproj TuneVault.Domain/
COPY TuneVault.Application/TuneVault.Application.csproj TuneVault.Application/
COPY TuneVault.Infrastructure/TuneVault.Infrastructure.csproj TuneVault.Infrastructure/
COPY TuneVault.API/TuneVault.API.csproj TuneVault.API/

RUN dotnet restore TuneVault.API/TuneVault.API.csproj

COPY . .

RUN dotnet publish TuneVault.API/TuneVault.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TuneVault.API.dll"]
