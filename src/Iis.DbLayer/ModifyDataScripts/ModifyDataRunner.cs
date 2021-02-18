using Iis.DataModel;
using Iis.Interfaces.Ontology.Data;
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
        ModifyDataItems _items = new ModifyDataItems();

        public ModifyDataRunner(OntologyContext context, IOntologyNodesData ontologyData)
        {
            _context = context;
            _ontologyData = ontologyData;
            AddItems();
        }

        private void AddItems()
        {
            var actions = new ModifyDataActions();
            _items.Add("RemoveEventWikiLinks", actions.RemoveEventWikiLinks);
        }
        public void Run()
        {
            foreach (var item in _items.Items)
            {
                if (!ActionDeployed(item.Name))
                {
                    Run(item);
                }
            }
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
