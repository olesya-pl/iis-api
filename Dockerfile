FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /opt/core

COPY publish/core/ /opt/core/
RUN mkdir /etc/core/ &&  rm -f /opt/core/appsettings.Production.json && ln -s /etc/core/appsettings.Production.json /opt/core/appsettings.Production.json

EXPOSE 5000
CMD ["dotnet", "Iis.Api.dll"]


