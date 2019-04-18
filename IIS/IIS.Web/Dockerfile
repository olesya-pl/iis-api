FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY IIS.Web/IIS.Web.csproj IIS.Web/
RUN dotnet restore IIS.Web/IIS.Web.csproj
COPY . .
WORKDIR /src/IIS.Web
RUN dotnet build IIS.Web.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish IIS.Web.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "IIS.Web.dll"]
