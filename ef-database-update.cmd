@echo OFF
CD .\src
SET DbPassword=

echo ON
dotnet ef database update --startup-project Iis.Api --project Iis.DataModel --context Iis.DataModel.OntologyContext
PAUSE