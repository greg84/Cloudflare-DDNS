FROM mcr.microsoft.com/dotnet/runtime:8.0.3-cbl-mariner2.0-distroless-amd64 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0.204-cbl-mariner2.0 AS build
WORKDIR /src
COPY ["CloudflareDDNSUpdater.csproj", ""]
RUN dotnet restore "./CloudflareDDNSUpdater.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "CloudflareDDNSUpdater.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "CloudflareDDNSUpdater.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CloudflareDDNSUpdater.dll"]