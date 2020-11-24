FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["CloudflareDDNSUpdater.csproj", ""]
RUN dotnet restore "./CloudflareDDNSUpdater.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "CloudflareDDNSUpdater.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CloudflareDDNSUpdater.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CloudflareDDNSUpdater.dll"]