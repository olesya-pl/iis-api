using Iis.Interfaces.Ontology.Data;
using Iis.Services.Contracts.Csv;
using Iis.Utility.Csv;
using System;
using System.Collections.Generic;
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

            foreach (var attributeNode in entity.GetAllAttributeNodes())
            {
                var dotName = attributeNode.GetDotName();
                if (string.IsNullOrEmpty(dotName))
                {
                    var i = 0;
                }
                csvData.Add(new CsvDataItem(
                    entity.Id.ToString(),
                    attributeNode.GetDotName(),
                    attributeNode.Value
                ));
            
            }

            foreach (var relationNode in entity.GetAllRelationNodes(n => n.NodeType.RelationType.TargetType.IsObjectSign))
            {
                var dotName = relationNode.GetDotName();
                if (string.IsNullOrEmpty(dotName))
                {
                    var i = 0;
                }
                csvData.Add(new CsvDataItem(
                    entity.Id.ToString(),
                    relationNode.GetDotName(),
                    relationNode.Relation.TargetNode.GetSingleDirectProperty("value").Value
                ));
            }

            foreach (var relationNode in entity.GetAllRelationNodes(n => n.NodeType.RelationType.TargetType.IsEnum))
            {
                var dotName = relationNode.GetDotName();
                if (string.IsNullOrEmpty(dotName))
                {
                    var i = 0;
                }
                csvData.Add(new CsvDataItem(
                    entity.Id.ToString(),
                    relationNode.GetDotName(),
                    relationNode.Relation.TargetNode.GetSingleDirectProperty("name").Value
                ));
            }

            foreach (var relationNode in entity.GetAllRelationNodes(n => n.NodeType.RelationType.TargetType.IsObject))
            {
                var dotName = relationNode.GetDotName();
                if (string.IsNullOrEmpty(dotName))
                {
                    var i = 0;
                }
                csvData.Add(new CsvDataItem(
                    entity.Id.ToString(),
                    relationNode.GetDotName(),
                    relationNode.Relation.TargetNode.GetComputedValue("__title")
                ));
            }
            return csvData;
        }
    }
}
