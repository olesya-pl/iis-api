using Iis.Interfaces.Ontology.Data;
using Iis.Services.Contracts.Csv;
using Iis.Utility.Csv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.Services
{
    public class CsvService : ICsvService
    {
        readonly IOntologyNodesData _data;
        public CsvService(IOntologyNodesData data)
        {
            _data = data;
        }
        public string GetDorCsvByTypeName(string typeName)
        {
            var entities = _data.GetEntitiesByTypeName(typeName);
            var csv = GetDorCsv(entities);
            return csv;
        }

        public string GetDorCsv(IEnumerable<INode> entities)
        {
            var csvData = new List<CsvDataItem>();

            foreach (var entity in entities)
            {
                csvData.AddRange(GetCsvDataItems(entity));
            }

            var csvManager = new CsvManager(csvData);
            return csvManager.GetCsv();
        }
        private List<CsvDataItem> GetCsvDataItems(INode entity)
        {
            var csvData = new List<CsvDataItem>();
            foreach (var dotNameValue in entity.GetDotNameValues(true).Items.Where(item => item != null))
            {
                csvData.Add(new CsvDataItem(entity.Id.ToString("N"), dotNameValue.DotName, dotNameValue.Value));
            }
            return csvData;
        }
    }
}
