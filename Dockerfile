FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build
ARG BUILD_VERSION=local


ENV IIS_SOURCE_REVISION_ID=${BUILD_VERSION}
WORKDIR /opt/core
COPY . /opt/core/

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /opt/core
COPY --from=build /opt/core/publish/core/ /opt/core/
RUN rm -f /opt/core/appsettings.Production.json && ln -s /local/appsettings.json /opt/core/appsettings.Production.json

EXPOSE 5000
CMD ["dotnet", "Iis.Api.dll"]
