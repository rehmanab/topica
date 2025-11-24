# buid and run from the src folder using cmd
# docker build -f RabbitMq.Topic.Producer.Host\Dockerfile -t rabbitmq.topic.producer.host:latest . && docker run -d --name rabbitmq.topic.producer.host rabbitmq.topic.producer.host:latest

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RabbitMq.Topic.Producer.Host/RabbitMq.Topic.Producer.Host.csproj", "RabbitMq.Topic.Producer.Host/"]
COPY ["Topica.SharedMessageHandlers/Topica.SharedMessageHandlers.csproj", "Topica.SharedMessageHandlers/"]
COPY ["Topica/Topica.csproj", "Topica/"]
COPY ["Topica.RabbitMq/Topica.RabbitMq.csproj", "Topica.RabbitMq/"]
RUN dotnet restore "RabbitMq.Topic.Producer.Host/RabbitMq.Topic.Producer.Host.csproj"
COPY . .
WORKDIR "/src/RabbitMq.Topic.Producer.Host"
RUN dotnet build "./RabbitMq.Topic.Producer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RabbitMq.Topic.Producer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RabbitMq.Topic.Producer.Host.dll"]
