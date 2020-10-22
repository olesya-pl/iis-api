FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build
ARG BUILD_VERSION=local


ENV IIS_SOURCE_REVISION_ID=${BUILD_VERSION}
WORKDIR /opt/core
COPY . /opt/core/

RUN dotnet publish src/Iis.Api/Iis.Api.csproj -c Release -o publish/core

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /opt/core
COPY --from=build /opt/core/publish/core/ /opt/core/

EXPOSE 5000
CMD ["dotnet", "Iis.Api.dll"]
