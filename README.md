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

##### Command-line usage:
`dotnet IIS.Core.dll --iis-actions action1,action2 [--iis-run-server true]`

You may specify multiple actions separated with comma, actions will run in the same order they were specified.

If any action was found, web server would not start. To override this behaviour, you may add `--iis-run-server true`.

###### List of actions:
* `clear-types` Deletes all types from database.
* `migrate-legacy-types` Migrates types from NodeJS database. Drops all types and entities.
* `migrate-legacy-entities` Migrates entities from NodeJS database.
* `migrate-legacy-files` Migrates all Files from NodeJS database with blob content.
* `fill-odysseus-types` Fills Odysseus ontology. Drops all types and entities.
* `seed-contour-data` Seeds Contour enums from files.
* `seed-odysseus-data` Seeds Odysseus enums from files.
* `apply-ef-migrations` Applies Entity Framework migrations to db, creates if it does not exist.
* `help` Displays this list in console.

## Migrate entities from Node.js db

1. Inside `appsettings.Staging.json` specify `db-legacy` with parameters to Node.js database
2. `db` setting should point to destination database (for .NET)
3. inside `publish/core` (or `publish/web`) run
   ```sh
   ASPNETCORE_ENVIRONMENT=Staging dotnet IIS.Core.dll --iis-actions apply-ef-migrations,clear-types,migrate-legacy-types,seed-contour-data,migrate-legacy-entities
   ```