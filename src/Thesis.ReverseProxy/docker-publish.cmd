@echo off
dotnet publish -c Release
cd bin/Release/net8.0/publish
docker buildx build --platform linux/amd64 -t seljmov/thesis-reverse-proxy:amd64 .
docker push seljmov/thesis-reverse-proxy:amd64