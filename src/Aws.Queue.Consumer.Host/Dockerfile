# buid and run from the src folder using cmd
# docker build -f Aws.Queue.Consumer.Host\Dockerfile -t aws.queue.consumer.host:latest . && docker run -d --name aws.queue.consumer.host aws.queue.consumer.host:latest

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Aws.Queue.Consumer.Host/Aws.Queue.Consumer.Host.csproj", "Aws.Queue.Consumer.Host/"]
COPY ["Topica.Aws/Topica.Aws.csproj", "Topica.Aws/"]
COPY ["Topica/Topica.csproj", "Topica/"]
COPY ["Topica.Host.Shared/Topica.Host.Shared.csproj", "Topica.Host.Shared/"]
RUN dotnet restore "Aws.Queue.Consumer.Host/Aws.Queue.Consumer.Host.csproj"
COPY . .
WORKDIR "/src/Aws.Queue.Consumer.Host"
RUN dotnet build "./Aws.Queue.Consumer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Aws.Queue.Consumer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Aws.Queue.Consumer.Host.dll"]
