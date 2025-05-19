# buid and run from the src folder using cmd
# docker build -f Kafka.Consumer.Host\Dockerfile -t kafka.consumer.host:latest . && docker run -d --name kafka.consumer.host kafka.consumer.host:latest
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
RUN echo what
COPY ["Kafka.Consumer.Host/Kafka.Consumer.Host.csproj", "Kafka.Consumer.Host/"]
COPY ["Topica/Topica.csproj", "Topica/"]
COPY ["Topica.Kafka/Topica.Kafka.csproj", "Topica.Kafka/"]
RUN dotnet restore "Kafka.Consumer.Host/Kafka.Consumer.Host.csproj"
COPY . .
WORKDIR "/src/Kafka.Consumer.Host"
RUN dotnet build "Kafka.Consumer.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Kafka.Consumer.Host.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kafka.Consumer.Host.dll"]
