# buid and run from the src folder using cmd
# docker build -f Pulsar.Consumer.Host\Dockerfile -t pulsar.consumer.host:latest . && docker run -d --name pulsar.consumer.host pulsar.consumer.host:latest
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Pulsar.Consumer.Host/Pulsar.Consumer.Host.csproj", "Pulsar.Consumer.Host/"]
COPY ["Topica.Pulsar/Topica.Pulsar.csproj", "Topica.Pulsar/"]
COPY ["Topica/Topica.csproj", "Topica/"]
RUN dotnet restore "Pulsar.Consumer.Host/Pulsar.Consumer.Host.csproj"
COPY . .
WORKDIR "/src/Pulsar.Consumer.Host"
RUN dotnet build "./Pulsar.Consumer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Pulsar.Consumer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pulsar.Consumer.Host.dll"]
