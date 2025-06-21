# buid and run from the src folder using cmd
# docker build -f Aws.Queue.Producer.Host\Dockerfile -t aws.queue.producer.host:latest . && docker run -d --name aws.queue.producer.host aws.queue.producer.host:latest

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Aws.Queue.Producer.Host/Aws.Queue.Producer.Host.csproj", "Aws.Queue.Producer.Host/"]
COPY ["Topica.Aws/Topica.Aws.csproj", "Topica.Aws/"]
COPY ["Topica/Topica.csproj", "Topica/"]
COPY ["Topica.SharedMessageHandlers/Topica.SharedMessageHandlers.csproj", "Topica.SharedMessageHandlers/"]
RUN dotnet restore "Aws.Queue.Producer.Host/Aws.Queue.Producer.Host.csproj"
COPY . .
WORKDIR "/src/Aws.Queue.Producer.Host"
RUN dotnet build "./Aws.Queue.Producer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Aws.Queue.Producer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Aws.Queue.Producer.Host.dll"]
