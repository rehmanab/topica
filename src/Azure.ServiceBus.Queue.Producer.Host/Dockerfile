# buid and run from the src folder using cmd
# docker build -f Azure.ServiceBus.Queue.Producer.Host\Dockerfile -t azure.servicebus.queue.producer.host:latest . && docker run -d --name azure.servicebus.queue.producer.host azure.servicebus.queue.producer.host:latest

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Azure.ServiceBus.Queue.Producer.Host/Azure.ServiceBus.Queue.Producer.Host.csproj", "Azure.ServiceBus.Queue.Producer.Host/"]
COPY ["Topica.Azure.ServiceBus/Topica.Azure.ServiceBus.csproj", "Topica.Azure.ServiceBus/"]
COPY ["Topica/Topica.csproj", "Topica/"]
COPY ["Topica.SharedMessageHandlers/Topica.SharedMessageHandlers.csproj", "Topica.SharedMessageHandlers/"]
RUN dotnet restore "Azure.ServiceBus.Queue.Producer.Host/Azure.ServiceBus.Queue.Producer.Host.csproj"
COPY . .
WORKDIR "/src/Azure.ServiceBus.Queue.Producer.Host"
RUN dotnet build "./Azure.ServiceBus.Queue.Producer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Azure.ServiceBus.Queue.Producer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Azure.ServiceBus.Queue.Producer.Host.dll"]
