0. Install dotnet-sdk-2.2.105 on machine.
1. Run `cd IIS/ && sh ./startup` bash script. Services will be published to IIS/publish folder.
2. Setup configuration env `export ASPNETCORE_ENVIRONMENT=Staging`
3. Go to `cd IIS/publish/web` folder and run `dotnet IIS.Web.dll`
4. Go to `cd IIS/publish/replication` folder and run `dotnet IIS.Replication.dll`
or `dotnet IIS.Replication.dll server.urls=http://localhost:PORT/` where any PORT can be assigned.
You can run it in detached mode or use tools like supervisor to start/stop services.
web is on port 5000 by default.
replication is on port 5500. Look at console output to find out which port is used.

To run in detached mode use [supervisor](https://til.secretgeek.net/linux/supervisor.html) or `pm2`. 
To change configuration find `IIS/publish/replication/appsettings.${ENV}.json` file and `IIS/publish/web/appsettings.${ENV}.json`. 
`${ENV}` is env name and equals to the value of env variable `ASPNETCORE_ENVIRONMENT` (in our case `Staging`)