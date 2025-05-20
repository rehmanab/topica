# buid and run from the src folder using cmd
# docker build -f Kafka.Producer.Host\Dockerfile -t kafka.producer.host:latest . && docker run -d --name kafka.producer.host kafka.producer.host:latest
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Kafka.Producer.Host/Kafka.Producer.Host.csproj", "Kafka.Producer.Host/"]
RUN dotnet restore "Kafka.Producer.Host/Kafka.Producer.Host.csproj"
COPY . .
WORKDIR "/src/Kafka.Producer.Host"
RUN dotnet build "./Kafka.Producer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Kafka.Producer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kafka.Producer.Host.dll"]
