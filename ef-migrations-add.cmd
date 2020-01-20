@echo OFF
CD .\src

echo ON
dotnet ef migrations add Moved --startup-project Iis.Api --project Iis.DataModel --context Iis.DataModel.OntologyContext
PAUSE