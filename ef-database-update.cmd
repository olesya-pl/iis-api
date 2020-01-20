@echo OFF
CD .\IIS
SET DbPassword=

echo ON
dotnet ef database update --startup-project IIS.Core --project Iis.DataModel --context Iis.DataModel.OntologyContext
PAUSE