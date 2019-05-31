FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY IIS.Replication/IIS.Replication.csproj IIS.Replication/
COPY IIS.Core/IIS.Core.csproj IIS.Core/
COPY IIS/IIS.csproj IIS/
RUN dotnet restore IIS.Replication/IIS.Replication.csproj
COPY . .
WORKDIR /src/IIS.Replication
RUN dotnet build IIS.Replication.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish IIS.Replication.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "IIS.Replication.dll"]
