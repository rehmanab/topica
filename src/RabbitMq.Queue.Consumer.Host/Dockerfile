# buid and run from the src folder using cmd
# docker build -f RabbitMq.Queue.Consumer.Host\Dockerfile -t rabbitmq.queue.consumer.host:latest . && docker run -d --name rabbitmq.queue.consumer.host rabbitmq.queue.consumer.host:latest

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RabbitMq.Queue.Consumer.Host/RabbitMq.Queue.Consumer.Host.csproj", "RabbitMq.Queue.Consumer.Host/"]
COPY ["Topica.SharedMessageHandlers/Topica.SharedMessageHandlers.csproj", "Topica.SharedMessageHandlers/"]
COPY ["Topica/Topica.csproj", "Topica/"]
COPY ["Topica.RabbitMq/Topica.RabbitMq.csproj", "Topica.RabbitMq/"]
RUN dotnet restore "RabbitMq.Queue.Consumer.Host/RabbitMq.Queue.Consumer.Host.csproj"
COPY . .
WORKDIR "/src/RabbitMq.Queue.Consumer.Host"
RUN dotnet build "./RabbitMq.Queue.Consumer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RabbitMq.Queue.Consumer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RabbitMq.Queue.Consumer.Host.dll"]
