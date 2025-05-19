# buid and run from the src folder using cmd
# docker build -f Aws.Consumer.Host\Dockerfile -t aws.consumer.host:latest . && docker run -d --name aws.consumer.host aws.consumer.host:latest
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Aws.Consumer.Host/Aws.Consumer.Host.csproj", "Aws.Consumer.Host/"]
COPY ["Topica.Aws/Topica.Aws.csproj", "Topica.Aws/"]
COPY ["Topica/Topica.csproj", "Topica/"]
RUN dotnet restore "Aws.Consumer.Host/Aws.Consumer.Host.csproj"
COPY . .
WORKDIR "/src/Aws.Consumer.Host"
RUN dotnet build "./Aws.Consumer.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./Aws.Consumer.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Aws.Consumer.Host.dll"]
