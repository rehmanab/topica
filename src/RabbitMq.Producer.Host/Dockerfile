# buid and run from the src folder using cmd
# docker build -f RabbitMq.Producer.Host\Dockerfile -t rabbitmq.producer.host:latest . && docker run --name rabbitmq.producer.host rabbitmq.producer.host:latest
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RabbitMq.Producer.Host/RabbitMq.Producer.Host.csproj", "RabbitMq.Producer.Host/"]
COPY ["Topica.RabbitMq/Topica.RabbitMq.csproj", "Topica.RabbitMq/"]
COPY ["Topica/Topica.csproj", "Topica/"]
RUN dotnet restore "RabbitMq.Producer.Host/RabbitMq.Producer.Host.csproj"
COPY . .
WORKDIR "/src/RabbitMq.Producer.Host"
RUN dotnet build "./RabbitMq.Producer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RabbitMq.Producer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RabbitMq.Producer.Host.dll"]
