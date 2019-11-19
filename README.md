# Integration Information System API

## Install

1. Install dotnet-sdk-2.2.105 on machine.
2. Run `cd IIS/IIS.Core` and `dotnet publish IIS.Core.csproj -o ../publish/core`. Services will be published to IIS/publish folder.
3. Configure application by changing `appsettings.${ASPNETCORE_ENVIRONMENT}.json` file, where `ASPNETCORE_ENVIRONMENT` is an env variable (e.g., `export ASPNETCORE_ENVIRONMENT=Staging`)
4. Go to `cd IIS/publish/core` folder and run `dotnet IIS.Core.dll`
or `dotnet IIS.Replication.dll server.urls=http://localhost:${PORT}/` where any `PORT` is a variable which contains server port number. By default app starts at `localhost:5000` port

To run in detached mode use [supervisor](https://til.secretgeek.net/linux/supervisor.html).

## Helthcheck endpoint

Healthcheck is available at `/api/server-health` and returns information about version and service availability. Currently it shows information about database, elasticsearch and RabbitMQ connection status. Is used to monitor server health by monitoring tools.

## CLI utils

There are several CLI actions which help to setup database and ontology:

```sh
dotnet IIS.Core.dll --iis-actions action1,action2,...,actionN
```

You may specify multiple actions separated with comma, actions will run in the same order they were specified.

If `--iis-actions` option is specified, application server doesn't start. To override this behaviour, you may add `--iis-run-server true`.

### List of actions:
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
   ASPNETCORE_ENVIRONMENT=Staging dotnet IIS.Core.dll --iis-actions apply-ef-migrations,clear-types,migrate-legacy-types,seed-contour-data,migrate-legacy-entities,migrate-legacy-files
   ```

## Setup for project

Ontology is cached in server's memory, so don't forget to restart server when you change it!

### Odysseus

For this project the ontology structure is hand-coded in [IIS/IIS.Core/Ontology.Seeding/Odysseus](./IIS/IIS.Core/Ontology.Seeding/Odysseus) folder. So, you can seed and run the project:

```sh
ASPNETCORE_ENVIRONMENT=Staging dotnet IIS.Core.dll --iis-actions apply-ef-migrations,clear-types,fill-odysseus-types,seed-odysseus-data
```

### Contour

For this project there is no hand-coded ontology version in .NET, only in [Node.js](https://git.warfare-tec.com/IIS/iis-api/tree/contour-master/src/ontology/contour). So, to run and seed the project you need to have either database dump for .NET or Node.js database with all required data (then follow [Migrate entities from Node.js db instructions](#migrate-entities-from-nodejs-db))