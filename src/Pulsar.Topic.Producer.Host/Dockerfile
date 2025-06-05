# buid and run from the src folder using cmd
# docker build -f Pulsar.Topic.Producer.Host\Dockerfile -t pulsar.topic.producer.host:latest . && docker run -d --name pulsar.topic.producer.host pulsar.topic.producer.host:latest

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Pulsar.Topic.Producer.Host/Pulsar.Topic.Producer.Host.csproj", "Pulsar.Topic.Producer.Host/"]
COPY ["Topica.Host.Shared/Topica.Host.Shared.csproj", "Topica.Host.Shared/"]
COPY ["Topica/Topica.csproj", "Topica/"]
COPY ["Topica.Pulsar/Topica.Pulsar.csproj", "Topica.Pulsar/"]
RUN dotnet restore "Pulsar.Topic.Producer.Host/Pulsar.Topic.Producer.Host.csproj"
COPY . .
WORKDIR "/src/Pulsar.Topic.Producer.Host"
RUN dotnet build "./Pulsar.Topic.Producer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Pulsar.Topic.Producer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pulsar.Topic.Producer.Host.dll"]
