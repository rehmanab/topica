FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RabbitMq.Consumer.Host/RabbitMq.Consumer.Host.csproj", "RabbitMq.Consumer.Host/"]
COPY ["Topica.RabbitMq/Topica.RabbitMq.csproj", "Topica.RabbitMq/"]
COPY ["Topica/Topica.csproj", "Topica/"]
RUN dotnet restore "RabbitMq.Consumer.Host/RabbitMq.Consumer.Host.csproj"
COPY . .
WORKDIR "/src/RabbitMq.Consumer.Host"
RUN dotnet build "./RabbitMq.Consumer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RabbitMq.Consumer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RabbitMq.Consumer.Host.dll"]
