@echo OFF
CD .\IIS

echo ON
dotnet ef migrations add Moved --startup-project IIS.Core --project Iis.DataModel --context Iis.DataModel.OntologyContext
PAUSE