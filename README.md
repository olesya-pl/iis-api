

0. Install dotnet-sdk-2.2.105 on machine.
1. Run `cd IIS/ && sh ./startup` bash script. Services will be published to IIS/publish folder.
2. Setup configuration env `export ASPNETCORE_ENVIRONMENT=Staging`
3. Go to `cd IIS/publish/web` folder and run `dotnet IIS.Web.dll`
4. Go to `cd IIS/publish/replication` folder and run `dotnet IIS.Replication.dll`

web is on port 5000
replication is on port 5500