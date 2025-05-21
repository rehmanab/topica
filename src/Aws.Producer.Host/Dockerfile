# buid and run from the src folder using cmd
# docker build -f Aws.Producer.Host\Dockerfile -t aws.producer.host:latest . && docker run -d --name aws.producer.host aws.producer.host:latest

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Aws.Producer.Host/Aws.Producer.Host.csproj", "Aws.Producer.Host/"]
RUN dotnet restore "Aws.Producer.Host/Aws.Producer.Host.csproj"
COPY . .
WORKDIR "/src/Aws.Producer.Host"
RUN dotnet build "./Aws.Producer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Aws.Producer.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Aws.Producer.Host.dll"]
