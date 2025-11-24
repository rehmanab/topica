# buid and run from the src folder using cmd
# docker build -f Azure.ServiceBus.Topic.Producer.Host\Dockerfile -t azure.servicebus.topic.producer.host:latest . && docker run -d --name azure.servicebus.topic.producer.host azure.servicebus.topic.producer.host:latest

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Azure.ServiceBus.Topic.Producer.Host/Azure.ServiceBus.Topic.Producer.Host.csproj", "Azure.ServiceBus.Topic.Producer.Host/"]
COPY ["Topica.Azure.ServiceBus/Topica.Azure.ServiceBus.csproj", "Topica.Azure.ServiceBus/"]
COPY ["Topica/Topica.csproj", "Topica/"]
COPY ["Topica.SharedMessageHandlers/Topica.SharedMessageHandlers.csproj", "Topica.SharedMessageHandlers/"]
RUN dotnet restore "Azure.ServiceBus.Topic.Producer.Host/Azure.ServiceBus.Topic.Producer.Host.csproj"
COPY . .
WORKDIR "/src/Azure.ServiceBus.Topic.Producer.Host"
RUN dotnet build "./Azure.ServiceBus.Topic.Producer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Azure.ServiceBus.Topic.Producer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Azure.ServiceBus.Topic.Producer.Host.dll"]
