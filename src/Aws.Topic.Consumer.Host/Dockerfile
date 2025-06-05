# buid and run from the src folder using cmd
# docker build -f Aws.Topic.Consumer.Host\Dockerfile -t aws.topic.consumer.host:latest . && docker run -d --name aws.topic.consumer.host aws.topic.consumer.host:latest

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Aws.Topic.Consumer.Host/Aws.Topic.Consumer.Host.csproj", "Aws.Topic.Consumer.Host/"]
COPY ["Topica.Aws/Topica.Aws.csproj", "Topica.Aws/"]
COPY ["Topica/Topica.csproj", "Topica/"]
COPY ["Topica.Host.Shared/Topica.Host.Shared.csproj", "Topica.Host.Shared/"]
RUN dotnet restore "Aws.Topic.Consumer.Host/Aws.Topic.Consumer.Host.csproj"
COPY . .
WORKDIR "/src/Aws.Topic.Consumer.Host"
RUN dotnet build "./Aws.Topic.Consumer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Aws.Topic.Consumer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Aws.Topic.Consumer.Host.dll"]
