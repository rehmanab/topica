# buid and run from the src folder using cmd
# docker build -f Kafka.Topic.Consumer.Host\Dockerfile -t kafka.topic.consumer.host:latest . && docker run -d --name kafka.topic.consumer.host kafka.topic.consumer.host:latest

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Kafka.Topic.Consumer.Host/Kafka.Topic.Consumer.Host.csproj", "Kafka.Topic.Consumer.Host/"]
COPY ["Topica.SharedMessageHandlers/Topica.SharedMessageHandlers.csproj", "Topica.SharedMessageHandlers/"]
COPY ["Topica/Topica.csproj", "Topica/"]
COPY ["Topica.Kafka/Topica.Kafka.csproj", "Topica.Kafka/"]
RUN dotnet restore "Kafka.Topic.Consumer.Host/Kafka.Topic.Consumer.Host.csproj"
COPY . .
WORKDIR "/src/Kafka.Topic.Consumer.Host"
RUN dotnet build "./Kafka.Topic.Consumer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Kafka.Topic.Consumer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kafka.Topic.Consumer.Host.dll"]
