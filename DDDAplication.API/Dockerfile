# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY DDDAplication.sln .
COPY DDDAplication.API/DDDAplication.API.csproj DDDAplication.API/
COPY DDDAplication.Application/DDDAplication.Application.csproj DDDAplication.Application/
COPY DDDAplication.Domain/DDDAplication.Domain.csproj DDDAplication.Domain/
COPY DDDAplication.Infrastructure/DDDAplication.Infrastructure.csproj DDDAplication.Infrastructure/
COPY DDDAplication.NUnitTests/DDDAplication.NUnitTests.csproj DDDAplication.NUnitTests/
COPY DDDAplication.Api.IntegrationTests/DDDAplication.Api.IntegrationTests.csproj DDDAplication.Api.IntegrationTests/

RUN dotnet restore

COPY . .
WORKDIR /src/DDDAplication.API
RUN dotnet publish -c Release -o /app/publish

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "DDDAplication.API.dll"]
