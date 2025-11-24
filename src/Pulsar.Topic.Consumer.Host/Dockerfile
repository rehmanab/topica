# buid and run from the src folder using cmd
# docker build -f Pulsar.Topic.Consumer.Host\Dockerfile -t pulsar.topic.consumer.host:latest . && docker run -d --name pulsar.topic.consumer.host pulsar.topic.consumer.host:latest

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Pulsar.Topic.Consumer.Host/Pulsar.Topic.Consumer.Host.csproj", "Pulsar.Topic.Consumer.Host/"]
COPY ["Topica.SharedMessageHandlers/Topica.SharedMessageHandlers.csproj", "Topica.SharedMessageHandlers/"]
COPY ["Topica/Topica.csproj", "Topica/"]
COPY ["Topica.Pulsar/Topica.Pulsar.csproj", "Topica.Pulsar/"]
RUN dotnet restore "Pulsar.Topic.Consumer.Host/Pulsar.Topic.Consumer.Host.csproj"
COPY . .
WORKDIR "/src/Pulsar.Topic.Consumer.Host"
RUN dotnet build "./Pulsar.Topic.Consumer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Pulsar.Topic.Consumer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pulsar.Topic.Consumer.Host.dll"]
