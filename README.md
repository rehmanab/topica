## Topical message topic and queuing library for .NET
Create AWS topics and subscribed queues

## Installation
```
dotnet add package Topical.Aws.Sns.Sqs --version 0.0.1
```
## Nuget local Usage
```
dotnet pack ./src --configuration Release --no-restore --no-build --output ./nupkgs -p:PackageVersion=0.0.1
dotnet nuget push "./nupkgs/*.nupkg" --api-key ghp_xxx --source "https://nuget.pkg.github.com/rehmanab/index.json" --skip-duplicate

Add for project NOT to be packagble into a .nupkg in the .csproj
<IsPackable>false</IsPackable>
```

## Docker local Usage
```
docker build -f Aws.Consumer.Host\Dockerfile -t aws.consumer.host:latest . && docker run -d --name aws.consumer.host aws.consumer.host:latest
or run after image created
docker run -d --name aws.consumer.host --pull missing aws.consumer.host:latest
```