# buid and run from the src folder using cmd
# docker build -f Azure.ServiceBus.Queue.Consumer\Dockerfile -t azure.servicebus.queue.consumer:latest . && docker run -d --name azure.servicebus.queue.consumer azure.servicebus.queue.consumer:latest

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Topica/Topica.csproj", "Topica/"]
COPY ["Topica.Azure.ServiceBus/Topica.Azure.ServiceBus.csproj", "Topica.Azure.ServiceBus/"]
COPY ["Azure.ServiceBus.Queue.Consumer.Host/Azure.ServiceBus.Queue.Consumer.Host.csproj", "Azure.ServiceBus.Queue.Consumer.Host/"]
COPY ["Topica.SharedMessageHandlers/Topica.SharedMessageHandlers.csproj", "Topica.SharedMessageHandlers/"]
RUN dotnet restore "Azure.ServiceBus.Queue.Consumer.Host/Azure.ServiceBus.Queue.Consumer.Host.csproj"
COPY . .
WORKDIR "/src/Azure.ServiceBus.Queue.Consumer.Host"
RUN dotnet build "./Azure.ServiceBus.Queue.Consumer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Azure.ServiceBus.Queue.Consumer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Azure.ServiceBus.Queue.Consumer.Host.dll"]
