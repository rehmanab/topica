# buid and run from the src folder using cmd
# docker build -f Kafka.Topic.Producer.Host\Dockerfile -t kafka.topic.producer.host:latest . && docker run -d --name kafka.topic.producer.host kafka.topic.producer.host:latest

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Kafka.Topic.Producer.Host/Kafka.Topic.Producer.Host.csproj", "Kafka.Topic.Producer.Host/"]
COPY ["Topica.Host.Shared/Topica.Host.Shared.csproj", "Topica.Host.Shared/"]
COPY ["Topica/Topica.csproj", "Topica/"]
COPY ["Topica.Kafka/Topica.Kafka.csproj", "Topica.Kafka/"]
RUN dotnet restore "Kafka.Topic.Producer.Host/Kafka.Topic.Producer.Host.csproj"
COPY . .
WORKDIR "/src/Kafka.Topic.Producer.Host"
RUN dotnet build "./Kafka.Topic.Producer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Kafka.Topic.Producer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kafka.Topic.Producer.Host.dll"]
