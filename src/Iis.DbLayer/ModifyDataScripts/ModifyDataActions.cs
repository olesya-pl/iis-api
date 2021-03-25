using System;
using System.Linq;
using System.Collections.Generic;
using Iis.DataModel;
using Iis.DataModel.Roles;
using Iis.Interfaces.Roles;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts.Interfaces;

namespace Iis.DbLayer.ModifyDataScripts
{
    public class ModifyDataActions
    {
        private IOntologySchemaService _ontologySchemaService;
        private IConnectionStringService _connectionStringService;
        public ModifyDataActions(IOntologySchemaService ontologySchemaService, IConnectionStringService connectionStringService)
        {
            _ontologySchemaService = ontologySchemaService;
            _connectionStringService = connectionStringService;
        }
        public void RemoveEventWikiLinks(OntologyContext context, IOntologyNodesData data)
        {
            var list = new List<IRelation>();
            data.WriteLock(() =>
            {
                var events = data.GetEntitiesByTypeName(EntityTypeNames.Event.ToString());
                foreach (var ev in events)
                {
                    var relations = ev.OutgoingRelations
                        .Where(r => r.Node.NodeType.Name == "associatedWithEvent"
                            && !r.TargetNode.NodeType.IsObjectOfStudy)
                        .ToList();

                    foreach (var relation in relations)
                    {
                        list.Add(relation);
                        data.RemoveNode(relation.Id);
                    }
                }
            });
        }

        public void AddAccessLevelAccessObject(OntologyContext context, IOntologyNodesData data)
        {
            var entityId = new Guid("a60af6c5d930476c96218ea5c0147fb7");

            var existingEntity = context.AccessObjects.Find(entityId);

            if( existingEntity != null) return;

            context.AccessObjects.Add(
                new AccessObjectEntity
                {
                    Id = entityId,
                    Title = "Зміна грифу обмеження доступу",
                    Kind = AccessKind.AccessLevelChange,
                    Category = AccessCategory.Entity,
                    UpdateAllowed = true
                }
            );
            context.SaveChanges();
        }
        
        public void AddAccessLevels(OntologyContext context, IOntologyNodesData data)
        {
            if (data.Schema.GetEntityTypeByName(EntityTypeNames.AccessLevel.ToString()) != null) return;

            const string NAME = "name";
            const string NUMERIC_INDEX = "numericIndex";

            var enumEntityType = data.Schema.GetEntityTypeByName(EntityTypeNames.Enum.ToString());
            var accessLevelType = data.Schema.CreateEntityType(EntityTypeNames.AccessLevel.ToString(), "Гріфи (рівні доступу)", false, enumEntityType.Id);
            data.Schema.CreateAttributeType(accessLevelType.Id, NUMERIC_INDEX, "Числовий індекс", ScalarType.Int, EmbeddingOptions.Required);
            _ontologySchemaService.SaveToDatabase(data.Schema, _connectionStringService.GetIisApiConnectionString());

            data.WriteLock(() =>
            {
                var node = data.CreateNode(accessLevelType.Id);
                data.AddValueByDotName(node.Id, "НВ - Не визначено", NAME);
                data.AddValueByDotName(node.Id, "0", NUMERIC_INDEX);

                node = data.CreateNode(accessLevelType.Id);
                data.AddValueByDotName(node.Id, "НТ - Не таємно", NAME);
                data.AddValueByDotName(node.Id, "1", NUMERIC_INDEX);

                node = data.CreateNode(accessLevelType.Id);
                data.AddValueByDotName(node.Id, "ДСВ - Для службового використання", NAME);
                data.AddValueByDotName(node.Id, "2", NUMERIC_INDEX);

                node = data.CreateNode(accessLevelType.Id);
                data.AddValueByDotName(node.Id, "Т - Таємно", NAME);
                data.AddValueByDotName(node.Id, "3", NUMERIC_INDEX);

                node = data.CreateNode(accessLevelType.Id);
                data.AddValueByDotName(node.Id, "ЦТ - Цілком таємно", NAME);
                data.AddValueByDotName(node.Id, "4", NUMERIC_INDEX);

                node = data.CreateNode(accessLevelType.Id);
                data.AddValueByDotName(node.Id, "ОС - Особливої важливості", NAME);
                data.AddValueByDotName(node.Id, "5", NUMERIC_INDEX);
            });
        }

        public void AddAccessLevelToObject(OntologyContext context, IOntologyNodesData data)
        {
            const string ACCESS_LEVEL = "accessLevel";
            
            var objectType = data.Schema.GetEntityTypeByName(EntityTypeNames.Object.ToString());
            var accessLevelType = data.Schema.GetEntityTypeByName(EntityTypeNames.AccessLevel.ToString());

            var accessLevel = objectType.GetProperty(ACCESS_LEVEL);
            if (accessLevel != null) return;
            var jsonMeta = "{ \"FormField\": {\"Type\": \"dropdown\" }}";

            data.Schema.CreateRelationTypeJson(
                objectType.Id, 
                accessLevelType.Id, 
                ACCESS_LEVEL, 
                "Гріф (рівень доступу)", 
                EmbeddingOptions.Optional,
                jsonMeta);

            _ontologySchemaService.SaveToDatabase(data.Schema, _connectionStringService.GetIisApiConnectionString());
        }
    }
}
