using Iis.DataModel;
using Iis.Interfaces.Ontology.Data;
using Iis.Services.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.DbLayer.ModifyDataScripts
{
    public class ModifyDataRunner
    {
        OntologyContext _context;
        IOntologyNodesData _ontologyData;
        IOntologySchemaService _ontologySchemaService;
        IConnectionStringService _connectionStringService;
        ModifyDataItems _items = new ModifyDataItems();

        public ModifyDataRunner(
            OntologyContext context, 
            IOntologyNodesData ontologyData, 
            IOntologySchemaService ontologySchemaService,
            IConnectionStringService connectionStringService)
        {
            _context = context;
            _ontologyData = ontologyData;
            _ontologySchemaService = ontologySchemaService;
            _connectionStringService = connectionStringService;
            AddItems();
        }

        private void AddItems()
        {
            var actions = new ModifyDataActions(_ontologySchemaService, _connectionStringService);
            _items.Add("RemoveEventWikiLinks", actions.RemoveEventWikiLinks);
            _items.Add("AddAccessLevelAccessObject", actions.AddAccessLevelAccessObject);
            _items.Add("FixFlightRadarLocationHistory", actions.FixFlightRadarLocationHistory);
            _items.Add("AddPhotoType", actions.AddPhotoType);
            _items.Add("AddObjectType", actions.AddObjectType);
            _items.Add("AddAccessLevels", actions.AddAccessLevels);
            _items.Add("AddAccessLevelToObject", actions.AddAccessLevelToObject);
            _items.Add("InitNewColumnsForAccessObjects", actions.InitNewColumnsForAccessObjects);
            _items.Add("RemoveCreareFromMaterialEntityAccess", actions.RemoveCreareFromMaterialEntityAccess);
            _items.Add("DefaultAccessLevelsForDors", actions.DefaultAccessLevelsForDors);
            _items.Add("AddPhotosToObject", actions.AddPhotosToObject);
            _items.Add("AddWikiEntityAccessObject", actions.AddWikiEntityAccessObject);
            _items.Add("SetupNewTypesForPhoneSign", actions.SetupNewTypesForPhoneSign);
            _items.Add("AddTitlePhotosToObject", actions.AddTitlePhotosToObject);
            _items.Add("RemoveMaterialLinkAccessObjects", actions.RemoveMaterialLinkAccessObjects);
            _items.Add("AddEventTitle", actions.AddEventTitle);
            _items.Add("RemoveDuplicateRelations", actions.RemoveDuplicateRelations);
            _items.Add("ClosePersinMultiple", actions.ClosePersinMultiple);
            _items.Add("MaterialChannel", actions.MaterialChannel);
            _items.Add("SetupObjectImportanceSortOrder", actions.SetupObjectImportanceSortOrder);
            //_items.Add("AddSecurityLevels", actions.AddSecurityLevels);
            //_items.Add("AddSecurityLevelsData", actions.AddSecurityLevelsData);
        }
        public bool Run()
        {
            var restartNeeded = false;
            foreach (var item in _items.Items)
            {
                if (!ActionDeployed(item.Name))
                {
                    Run(item);
                    if (item.HostRestartNeeded)
                        restartNeeded = true;
                }
            }
            return restartNeeded;
        }
        private void Run(ModifyDataItem item)
        {
            try
            {
                item.Action.Invoke(_context, _ontologyData);
                AddLog(item.Name, true, null);
            }
            catch (Exception ex)
            {
                AddLog(item.Name, false, $"{ex.Message}; {ex.InnerException?.Message}");
            }
        }
        private bool ActionDeployed(string name)
        {
            return _context.ModifyDataLogs.Any(x => x.Name == name && x.Success);
        }
        private void AddLog(string name, bool success, string error)
        {
            var logEntity = new ModifyDataLogEntity
            {
                Id = Guid.NewGuid(),
                Name = name,
                RunDate = DateTime.Now,
                Success = success,
                Error = error
            };

            _context.ModifyDataLogs.Add(logEntity);
            _context.SaveChanges();
        }
    }
}
