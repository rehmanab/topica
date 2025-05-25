# buid and run from the src folder using cmd
# docker build -f Azure.ServiceBus.Producer.Host\Dockerfile -t azure.servicebus.producer.host:latest . && docker run -d --name azure.servicebus.producer.host azure.servicebus.producer.host:latest

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Azure.ServiceBus.Producer.Host/Azure.ServiceBus.Producer.Host.csproj", "Azure.ServiceBus.Producer.Host/"]
RUN dotnet restore "Azure.ServiceBus.Producer.Host/Azure.ServiceBus.Producer.Host.csproj"
COPY . .
WORKDIR "/src/Azure.ServiceBus.Producer.Host"
RUN dotnet build "./Azure.ServiceBus.Producer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Azure.ServiceBus.Producer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Azure.ServiceBus.Producer.Host.dll"]
