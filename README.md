# aws-topic-creator
Create AWS topics and subscribed queues


dotnet pack ./src --configuration Release --no-restore --no-build --output ./nupkgs -p:PackageVersion=0.0.1


dotnet nuget push "./nupkgs/*.nupkg" --api-key ghp_xxx --source "https://nuget.pkg.github.com/rehmanab/index.json" --skip-duplicate