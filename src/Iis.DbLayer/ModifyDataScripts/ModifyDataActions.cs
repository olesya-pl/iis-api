using System;
using System.Linq;
using System.Collections.Generic;
using Iis.DataModel;
using Iis.DataModel.Roles;
using Iis.Interfaces.Roles;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts.Interfaces;
using Iis.OntologySchema.ChangeParameters;

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
        private void SaveOntologySchema(IOntologySchema schema) =>
            _ontologySchemaService.SaveToDatabase(schema, _connectionStringService.GetIisApiConnectionString());
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

        public void FixFlightRadarLocationHistory(OntologyContext context, IOntologyNodesData data)
        {
            var signType = data.Schema.GetEntityTypeByName("ICAOSign");

            var signList = data.GetNodesByTypeId(signType.Id)
                .Select(sign => sign.Id)
                .ToList();

            if(signList.Count == 0) return;

            var entityList = context.LocationHistory
                .Where(e => e.NodeId != null && e.NodeId != e.EntityId && signList.Contains(e.NodeId.Value))
                .ToList();

            if(entityList.Count == 0) return;

            foreach (var entity in entityList)
            {
                entity.EntityId = entity.NodeId;
            }

            context.LocationHistory.UpdateRange(entityList);

            context.SaveChanges();
        }
        public void AddAccessLevels(OntologyContext context, IOntologyNodesData data)
        {
            const string NAME = "name";
            const string NUMERIC_INDEX = "numericIndex";
            var accessLevelType = data.Schema.GetEntityTypeByName(EntityTypeNames.AccessLevel.ToString());

            if (accessLevelType == null)
            {
                var enumEntityType = data.Schema.GetEntityTypeByName(EntityTypeNames.Enum.ToString());
                accessLevelType = data.Schema.CreateEntityType(EntityTypeNames.AccessLevel.ToString(), "Грифи (рівні доступу)", false, enumEntityType.Id);
                data.Schema.CreateAttributeType(accessLevelType.Id, NUMERIC_INDEX, "Числовий індекс", ScalarType.Int, EmbeddingOptions.Required);
                SaveOntologySchema(data.Schema);
            }

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
                "Гриф (рівень доступу)", 
                EmbeddingOptions.Optional,
                jsonMeta);

            SaveOntologySchema(data.Schema);
        }

        public void InitNewColumnsForAccessObjects(OntologyContext context, IOntologyNodesData data)
        {
            var accesEntityId = new Guid("a60af6c5d930476c96218ea5c0147fb7");
            var accessEntity = context.AccessObjects.Find(accesEntityId);
            if (accessEntity != null)
            {
                context.AccessObjects.Remove(accessEntity);
            }
            context.SaveChanges();

            var reportsEntityId = new Guid("bb2fe99de99645528e89acc5bd7232e7");
            var reportsEntity = context.AccessObjects.Find(reportsEntityId);
            if (reportsEntity == null)
            {
                context.AccessObjects.Add(new AccessObjectEntity
                {
                    Id = new Guid("bb2fe99de99645528e89acc5bd7232e7"),
                    Title = "Звіти",
                    Kind = AccessKind.Report,
                    Category = AccessCategory.Entity,
                    CreateAllowed = true,
                    ReadAllowed = true,
                    UpdateAllowed = true,
                    AccessLevelUpdateAllowed = true,
                    CommentingAllowed = true,
                    SearchAllowed = true
                });
            }

            var reportsTabId = new Guid("56c3dd7aeb8a424882ce82862c3c4388");
            var reportsTab = context.AccessObjects.Find(reportsTabId);
            if (reportsTab == null)
            {
                context.AccessObjects.Add(new AccessObjectEntity
                {
                    Id = new Guid("56c3dd7aeb8a424882ce82862c3c4388"),
                    Title = "Звіти",
                    Kind = AccessKind.Report,
                    Category = AccessCategory.Tab,
                    ReadAllowed = true,
                });
            }

            context.SaveChanges();

            var themesTabId = new Guid("1d20fd240de84531a19c4986cb2d277b");
            var themesTab = context.AccessObjects.Find(themesTabId);
            if (themesTab == null)
            {
                context.AccessObjects.Add(new AccessObjectEntity
                {
                    Id = new Guid("1d20fd240de84531a19c4986cb2d277b"),
                    Title = "Теми та оновлення",
                    Kind = AccessKind.ThemesTab,
                    Category = AccessCategory.Tab,
                    ReadAllowed = true,
                });
            }

            context.SaveChanges();

            var materialUploadTabId = new Guid("b51766b93422450ca165d9f9d98a1fb0");
            var materialUploadTab = context.AccessObjects.Find(materialUploadTabId);
            if (themesTab == null)
            {
                context.AccessObjects.Add(new AccessObjectEntity
                {
                    Id = new Guid("b51766b93422450ca165d9f9d98a1fb0"),
                    Title = "Завантаження матеріалів",
                    Kind = AccessKind.MaterialUpoadTab,
                    Category = AccessCategory.Tab,
                    ReadAllowed = true,
                });
            }

            context.SaveChanges();

            var wikiTabId = new Guid("cda32d549dd4403a94c391f8ff6d5bca");
            var wikiTab = context.AccessObjects.Find(wikiTabId);
            if (themesTab == null)
            {
                context.AccessObjects.Add(new AccessObjectEntity
                {
                    Id = new Guid("cda32d549dd4403a94c391f8ff6d5bca"),
                    Title = "Довідник ОІВТ",
                    Kind = AccessKind.Wiki,
                    Category = AccessCategory.Tab,
                    ReadAllowed = true,
                });
            }

            context.SaveChanges();

            var enityIds = new[] { new Guid("01380557fb27480c96ed6c56b8ae45a8"), new Guid("036137a67db34a0e9566f4ce9691a878") };
            var entities = context.AccessObjects.Where(p => enityIds.Contains(p.Id));
            foreach (var entity in entities)
            {
                entity.SearchAllowed = entity.CommentingAllowed = entity.AccessLevelUpdateAllowed = true;
                
            }
            context.AccessObjects.UpdateRange(entities);
            context.SaveChanges();

            var materialsEntityId = new Guid("02c1895f7d444512a0a97ebbf6c6690c");
            var materialsEntity = context.AccessObjects.Find(materialsEntityId);
            if (materialsEntity == null)
            {
                context.AccessObjects.Add(new AccessObjectEntity
                {
                    Id = new Guid("02c1895f7d444512a0a97ebbf6c6690c"),
                    Title = "Матеріали",
                    Kind = AccessKind.Material,
                    Category = AccessCategory.Entity,
                    CreateAllowed = true,
                    ReadAllowed = true,
                    UpdateAllowed = true,
                    AccessLevelUpdateAllowed = true,
                    CommentingAllowed = true,
                    SearchAllowed = true
                });
            }
            else
            {
                materialsEntity.AccessLevelUpdateAllowed = materialsEntity.CommentingAllowed = materialsEntity.SearchAllowed = true;
                materialsEntity.CreateAllowed = true;
                context.AccessObjects.Update(materialsEntity);
            }
            context.SaveChanges();

            var materialsTabId = new Guid("08e273695e9a49ee8eb4daa305cf9029");
            var materialsTab = context.AccessObjects.Find(materialsTabId);
            if (materialsTab == null)
            {
                context.AccessObjects.Add(new AccessObjectEntity
                {
                    Id = new Guid("08e273695e9a49ee8eb4daa305cf9029"),
                    Title = "Матеріали",
                    Kind = AccessKind.Material,
                    Category = AccessCategory.Tab,
                    ReadAllowed = true,
                });
            }
            else
            {
                materialsTab.Title = "Матеріали";
                materialsTab.Kind = AccessKind.Material;
                context.AccessObjects.Update(materialsTab);
            }
            context.SaveChanges();

            var eventsTabId = new Guid("06be568c17aa4c38983aae5e80dac279");
            var eventsTab = context.AccessObjects.Find(eventsTabId);
            if (eventsTab == null)
            {
                context.AccessObjects.Add(new AccessObjectEntity
                {
                    Id = new Guid("06be568c17aa4c38983aae5e80dac279"),
                    Title = "Події",
                    Kind = AccessKind.Event,
                    Category = AccessCategory.Tab,
                    ReadAllowed = true,
                });
            }
            else
            {
                eventsTab.Kind = AccessKind.Event;
                context.AccessObjects.Update(eventsTab);
            }
            context.SaveChanges();

            var entityTabId = new Guid("076b6fd6204b46d7afc923b3328687a4");
            var entityTab = context.AccessObjects.Find(entityTabId);
            if (entityTab == null)
            {
                context.AccessObjects.Add(new AccessObjectEntity
                {
                    Id = new Guid("076b6fd6204b46d7afc923b3328687a4"),
                    Title = "Об'єкти розвідки",
                    Kind = AccessKind.Entity,
                    Category = AccessCategory.Tab,
                    ReadAllowed = true,
                });
            }
            else
            {
                entityTab.Kind = AccessKind.Entity;
                context.AccessObjects.Update(entityTab);
            }
            context.SaveChanges();

            var materialsEntityRelationTabId = new Guid("0971390a21fa4ab4ae277bb4c7c5bd45");
            var materialsEntityRelationTab = context.AccessObjects.Find(materialsEntityRelationTabId);
            if (materialsEntityRelationTab == null)
            {
                context.AccessObjects.Add(new AccessObjectEntity
                {
                    Id = new Guid("0971390a21fa4ab4ae277bb4c7c5bd45"),
                    Title = "Прив'язка матеріалів до об'єктів розвідки",
                    Kind = AccessKind.MaterialDorLink,
                    Category = AccessCategory.Entity,
                    CreateAllowed = true,
                    ReadAllowed = true,
                    UpdateAllowed = true,
                });
            }
            else
            {
                materialsEntityRelationTab.Title = "Прив'язка матеріалів до об'єктів розвідки";
                context.AccessObjects.Update(materialsTab);
            }
            context.SaveChanges();

            var eventRelationTabId = new Guid("102617ecd2514b5f97e8be1a9bf99bc3");
            var eventRelationTab = context.AccessObjects.Find(eventRelationTabId);
            if (materialsEntityRelationTab == null)
            {
                context.AccessObjects.Add(new AccessObjectEntity
                {
                    Id = new Guid("102617ecd2514b5f97e8be1a9bf99bc3"),
                    Title = "Прив'язка подій до об'єктів розвідки та матеріалів",
                    Kind = AccessKind.EventLink,
                    Category = AccessCategory.Entity,
                    CreateAllowed = true,
                    ReadAllowed = true,
                    UpdateAllowed = true,
                });
            }
            else
            {
                eventRelationTab.Title = "Прив'язка подій до об'єктів розвідки та матеріалів";
                context.AccessObjects.Update(materialsTab);
            }

            context.SaveChanges();
        }

        public void RemoveCreareFromMaterialEntityAccess(OntologyContext context, IOntologyNodesData data)
        {
            var materialsEntityId = new Guid("02c1895f7d444512a0a97ebbf6c6690c");
            var materialsEntity = context.AccessObjects.Find(materialsEntityId);
            if (materialsEntity == null)
            {
                context.AccessObjects.Add(new AccessObjectEntity
                {
                    Id = new Guid("02c1895f7d444512a0a97ebbf6c6690c"),
                    Title = "Матеріали",
                    Kind = AccessKind.Material,
                    Category = AccessCategory.Entity,
                    CreateAllowed = false,
                    ReadAllowed = true,
                    UpdateAllowed = true,
                    AccessLevelUpdateAllowed = true,
                    CommentingAllowed = true,
                    SearchAllowed = true
                });
            }
            else
            {
                materialsEntity.AccessLevelUpdateAllowed = materialsEntity.CommentingAllowed = materialsEntity.SearchAllowed = true;
                materialsEntity.CreateAllowed = false;
                context.AccessObjects.Update(materialsEntity);
            }
            context.SaveChanges();
        }
        public void DefaultAccessLevelsForDors(OntologyContext context, IOntologyNodesData data)
        {
            const string ACCESS_LEVEL = "accessLevel";
            var noAccessLevelNodes = data.GetAllNodes()
                .Where(n =>
                    n.NodeType.IsObject && n.GetSingleProperty(ACCESS_LEVEL) == null)
                .ToList();

            var accessLevelType = data.Schema.GetEntityTypeByName(EntityTypeNames.AccessLevel.ToString());
            if (accessLevelType == null)
                throw new Exception("Сутність AccessType не знайдена в Онтології");

            var defaultAccessLevelId = data.GetNodesByTypeId(accessLevelType.Id)
                .Where(n => n.GetSingleProperty("numericIndex").Value == "0")
                .Select(n => n.Id)
                .Single();

            var objectEntity = data.Schema.GetEntityTypeByName(EntityTypeNames.Object.ToString());
            var accessLevelRelationType = objectEntity.GetProperty("accessLevel");

            data.WriteLock(() =>
            {
                foreach (var node in noAccessLevelNodes)
                {
                    data.CreateRelation(node.Id, defaultAccessLevelId, accessLevelRelationType.Id);
                }
            });

            data.Schema.UpdateNodeType(new NodeTypeUpdateParameter 
            { 
                Id = accessLevelRelationType.Id,
                EmbeddingOptions = EmbeddingOptions.Required
            });
            SaveOntologySchema(data.Schema);
        }

        public void AddPhotosToObject(OntologyContext context, IOntologyNodesData data)
        {
            const string PHOTO = "Photo";
            if (data.Schema.GetEntityTypeByName(PHOTO) != null) return;

            var photoType = data.Schema.CreateEntityType(PHOTO);
            data.Schema.CreateAttributeType(photoType.Id, "image", "Фото", ScalarType.File, EmbeddingOptions.Required);
            data.Schema.CreateAttributeType(photoType.Id, "title", "Заголовок", ScalarType.String, EmbeddingOptions.Optional);

            var objectType = data.Schema.GetEntityTypeByName(EntityTypeNames.Object.ToString());
            data.Schema.CreateRelationType(objectType.Id, photoType.Id, "photos", "Фото", EmbeddingOptions.Multiple);

            SaveOntologySchema(data.Schema);
        }

        public void AddWikiEntityAccessObject(OntologyContext context, IOntologyNodesData data)
        {
            var wikiEntityId = new Guid("c28c097bd74e49d1a8cbbeb0ecf43b08");
            var wikiEntity = context.AccessObjects.Find(wikiEntityId);
            if (wikiEntity == null)
            {
                context.AccessObjects.Add(new AccessObjectEntity
                {
                    Id = new Guid("c28c097bd74e49d1a8cbbeb0ecf43b08"),
                    Title = "Довідник ОІВТ",
                    Kind = AccessKind.Wiki,
                    Category = AccessCategory.Entity,
                    CreateAllowed = true,
                    ReadAllowed = true,
                    UpdateAllowed = true,
                    AccessLevelUpdateAllowed = true,
                    CommentingAllowed = true,
                    SearchAllowed = true
                });
            }
        }

        public void AddTitlePhotosToObject(OntologyContext context, IOntologyNodesData data)
        {
            var photoType = data.Schema.GetEntityTypeByName("Photo");
            if (photoType == null) throw new Exception("Photo entity type is not found");
            var imageProperty = photoType.GetRelationByName("image");

            var objectType = data.Schema.GetEntityTypeByName(EntityTypeNames.Object.ToString());
            var titlePhotosProperty = objectType.GetProperty("titlePhotos");
            if (titlePhotosProperty == null)
            {
                var jsonMeta = @"{
    ""AcceptsEntityOperations"": [0,1,2],
    ""FormField"": {
        ""Type"": ""photo""
    }
}";

                titlePhotosProperty = data.Schema.CreateRelationTypeJson(
                    objectType.Id,
                    photoType.Id,
                    "titlePhotos",
                    "Фото в заголовку",
                    EmbeddingOptions.Multiple,
                    jsonMeta
                );

                SaveOntologySchema(data.Schema);
            }

            var objectNodes = data.GetAllNodes().Where(n => n.NodeType.IsObject);
            foreach (var node in objectNodes)
            {
                var photoProperty = node.GetSingleProperty("photo");
                if (photoProperty != null)
                {
                    var photoNode = data.CreateNode(photoType.Id);
                    data.CreateRelationWithAttribute(photoNode.Id, imageProperty.Id, photoProperty.Value);
                    data.CreateRelation(node.Id, photoNode.Id, titlePhotosProperty.Id);
                }
            }
        }

        public void RemoveMaterialLinkAccessObjects(OntologyContext context, IOntologyNodesData data)
        {
            var materialDorLinkId = new Guid("0971390a21fa4ab4ae277bb4c7c5bd45");
            var eventLinkId = new Guid("102617ecd2514b5f97e8be1a9bf99bc3");
            var enitiesToRemove = new[] { materialDorLinkId, eventLinkId };
            context.AccessObjects.RemoveRange(context.AccessObjects.Where(p => enitiesToRemove.Contains(p.Id)));
            context.SaveChanges();
        }
    }
}
