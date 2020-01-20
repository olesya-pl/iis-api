FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY IIS.Search/IIS.Search.csproj IIS.Search/
RUN dotnet restore IIS.Search/IIS.Search.csproj
COPY . .
WORKDIR /src/IIS.Search
RUN dotnet build IIS.Search.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish IIS.Search.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "IIS.Search.dll"]
