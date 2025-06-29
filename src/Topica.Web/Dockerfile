# buid and run from the src folder using cmd
# docker build -f Topica.Web\Dockerfile -t topica.web:latest . && docker run -d -p 7055:7055 -p 7022:7022 --env ASPNETCORE_ENVIRONMENT=Development -e ASPNETCORE_URLS=https://*:7055;http://*:7022 -e ASPNETCORE_Kestrel__Certificates__Default__Password="password" -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx -v %USERPROFILE%\.aspnet\https:/https/ --name topica.web topica.web:latest

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Topica.Web/Topica.Web.csproj", "Topica.Web/"]
COPY ["Topica.Aws/Topica.Aws.csproj", "Topica.Aws/"]
COPY ["Topica/Topica.csproj", "Topica/"]
COPY ["Topica.Azure.ServiceBus/Topica.Azure.ServiceBus.csproj", "Topica.Azure.ServiceBus/"]
COPY ["Topica.Kafka/Topica.Kafka.csproj", "Topica.Kafka/"]
COPY ["Topica.Pulsar/Topica.Pulsar.csproj", "Topica.Pulsar/"]
COPY ["Topica.RabbitMq/Topica.RabbitMq.csproj", "Topica.RabbitMq/"]
RUN dotnet restore "Topica.Web/Topica.Web.csproj"
COPY . .
WORKDIR "/src/Topica.Web"
RUN dotnet build "./Topica.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Topica.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Topica.Web.dll"]
