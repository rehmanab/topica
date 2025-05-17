FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Pulsar.Producer.Host/Pulsar.Producer.Host.csproj", "Pulsar.Producer.Host/"]
COPY ["Topica.Pulsar/Topica.Pulsar.csproj", "Topica.Pulsar/"]
COPY ["Topica/Topica.csproj", "Topica/"]
RUN dotnet restore "Pulsar.Producer.Host/Pulsar.Producer.Host.csproj"
COPY . .
WORKDIR "/src/Pulsar.Producer.Host"
RUN dotnet build "./Pulsar.Producer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Pulsar.Producer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pulsar.Producer.Host.dll"]
