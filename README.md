0. Install dotnet-sdk-2.2.5 on machine.
1. Run IIS/startup bash script. Services will be published to IIS/publish folder.
2. Go to IIS/publish/web folder and run `dotnet IIS.Web.dll`
3. Go to IIS/publish/replication folder and run `dotnet IIS.Replication.dll`

web is on port 5000
replication is on port 5500