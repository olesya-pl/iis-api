using System;
using System.Linq;
using System.Collections.Generic;
using Iis.DataModel;
using Iis.DataModel.Roles;
using Iis.DataModel.Materials;
using Iis.DataModel.FlightRadar;
using Iis.Interfaces.Roles;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts.Interfaces;
using Iis.OntologySchema.ChangeParameters;
using Iis.OntologyData;
using Iis.Domain.Materials;

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

        public void AddPhotoType(OntologyContext context, IOntologyNodesData data)
        {
            var photoType = data.Schema.GetEntityTypeByName(EntityTypeNames.Photo.ToString());
            if (photoType == null)
            {
                photoType = data.Schema.CreateEntityType(EntityTypeNames.Photo.ToString(), "Зображення", false);
                data.Schema.CreateAttributeType(photoType.Id, "image", "Фото", ScalarType.File, EmbeddingOptions.Required);
                data.Schema.CreateAttributeType(photoType.Id, "title", "Заголовок");
            }
        }

        public void AddObjectType(OntologyContext context, IOntologyNodesData data)
        {
            var objectType = data.Schema.GetEntityTypeByName(EntityTypeNames.Object.ToString());
            if (objectType == null)
            {
                objectType = data.Schema.CreateEntityType(EntityTypeNames.Object.ToString(), "Базовий об'єкт", false);
                data.Schema.CreateAttributeType(objectType.Id, "title", "Заголовок");
                data.Schema.CreateAttributeTypeJson(
                    objectType.Id,
                    "__title",
                    "Повна назва",
                    ScalarType.String,
                    EmbeddingOptions.Optional,
                    "{\"Formula\": \"{title};\\\"Об'єкт без назви\\\"\"}"
                );
            }

            var objectOfStudyType = data.Schema.GetEntityTypeByName(EntityTypeNames.ObjectOfStudy.ToString());
            if (!objectOfStudyType.IsInheritedFrom(EntityTypeNames.Object.ToString()))
            {
                data.Schema.SetInheritance(objectOfStudyType.Id, objectType.Id);
            }

            var eventType = data.Schema.GetEntityTypeByName(EntityTypeNames.Event.ToString());
            if (eventType == null)
            {
                return;
            }
            if (!eventType.IsInheritedFrom(EntityTypeNames.Object.ToString()))
            {
                data.Schema.SetInheritance(eventType.Id, objectType.Id);
            }

            SaveOntologySchema(data.Schema);
        }

        public void AddAccessLevelAccessObject(OntologyContext context, IOntologyNodesData data)
        {
            var entityId = new Guid("a60af6c5d930476c96218ea5c0147fb7");

            var existingEntity = context.AccessObjects.Find(entityId);

            if (existingEntity != null) return;

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

            if (signType == null)
            {
                return;
            }

            var signList = data.GetNodesByTypeId(signType.Id)
                .Select(sign => sign.Id)
                .ToList();

            if (signList.Count == 0) return;

            var entityList = context.LocationHistory
                .Where(e => e.NodeId != null && e.NodeId != e.EntityId && signList.Contains(e.NodeId.Value))
                .ToList();

            if (entityList.Count == 0) return;

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

        private int FetchAndAddCoordinates(IReadOnlyList<INode> signList, OntologyContext context)
        {
            const string LocationXPropName = "locationX";
            const string LocationYPropName = "locationY";
            int recordAdded = 0;

            foreach (var sign in signList)
            {
                var latStringValue = sign.GetSingleProperty(LocationYPropName)?.Value;
                var lonStringValue = sign.GetSingleProperty(LocationXPropName)?.Value;

                var parseResult = Decimal.TryParse(latStringValue, out decimal latitude)
                                & Decimal.TryParse(lonStringValue, out decimal longitude);

                if (parseResult)
                {
                    var entity = new DataModel.FlightRadar.LocationHistoryEntity
                    {
                        Id = Guid.NewGuid(),
                        EntityId = sign.Id,
                        NodeId = sign.Id,
                        Lat = latitude,
                        Long = longitude,
                        RegisteredAt = sign.GetSingleProperty(LocationYPropName).CreatedAt
                    };

                    context.LocationHistory.Add(entity);
                    recordAdded++;
                }
            }
            return recordAdded;
        }

        private void UpdateRelationNodeTypeId(IReadOnlyCollection<Guid> relationIdList, Guid newRelationNodeTypeId, OntologyContext context)
        {
            foreach (var relationId in relationIdList)
            {
                var nodeEntity = context.Nodes.Find(relationId);

                if (nodeEntity is null) continue;

                nodeEntity.NodeTypeId = newRelationNodeTypeId;

                context.Nodes.Update(nodeEntity);
            }
        }

        private IReadOnlyCollection<Guid> GetRelationIdList(IReadOnlyList<INode> signList, string relationTypeName)
        {
            return signList
                    .Select(e => e.OutgoingRelations.FirstOrDefault(e => e.RelationTypeName == relationTypeName))
                    .Where(e => e != null)
                    .Select(e => e.Id)
                    .ToArray();
        }
        private INodeTypeLinked GetOrCreateSatelliteIridiumPhoneSignType(IOntologyNodesData data)
        {
            const string NAME = "SatelliteIridiumPhoneSign";
            var signType = data.Schema.GetEntityTypeByName(NAME);
            if (signType == null)
            {
                signType = data.Schema.CreateEntityType(NAME, "Супутниковий телефон Iridium", false);
                data.Schema.CreateAttributeType(signType.Id, "tmsi", "TMSI");
                data.Schema.CreateAttributeType(signType.Id, "imsi", "IMSI");
                data.Schema.CreateAttributeType(signType.Id, "imei", "IMEI");
            }
            return signType;
        }
        public void SetupNewTypesForPhoneSign(OntologyContext context, IOntologyNodesData data)
        {
            const string BeamPropName = "beam";
            const string DbObjectPropName = "dbObject";
            const string LocationXPropName = "locationX";
            const string LocationYPropName = "locationY";
            const string AbstractSatellitePhoneSignName = "AbstractSatellitePhoneSign";

            if (data.Schema.GetEntityTypeByName(AbstractSatellitePhoneSignName) != null) return;

            var objectSignType = data.Schema.GetEntityTypeByName(EntityTypeNames.ObjectSign.ToString());
            var satIridiumPhoneSignType = GetOrCreateSatelliteIridiumPhoneSignType(data);
            var satPhoneSignType = data.Schema.GetEntityTypeByName("SatellitePhoneSign");

            if (objectSignType == null || satIridiumPhoneSignType == null || satPhoneSignType == null)
            {
                return;
            }

            var iridiumSignList = data.GetNodesByTypeId(satIridiumPhoneSignType.Id);
            var satPhoneSignList = data.GetNodesByTypeId(satPhoneSignType.Id);

            var iridiumSingBeamRelationIdList = GetRelationIdList(iridiumSignList, BeamPropName);

            var satPhoneBeamRelationIdList = GetRelationIdList(satPhoneSignList, BeamPropName);

            var satPhoneDbObjectRelationIdList = GetRelationIdList(satPhoneSignList, DbObjectPropName);

            //migrate coordinates for SatelliteIridiumPhoneSign
            var recordsAdded = 0;
            recordsAdded += FetchAndAddCoordinates(iridiumSignList, context);

            recordsAdded += FetchAndAddCoordinates(satPhoneSignList, context);

            if (recordsAdded > 0) context.SaveChanges();

            //create and setup new abstract type AbstractSatellitePhoneSign
            var abstractSatPhoneSignType = data.Schema.CreateEntityType(AbstractSatellitePhoneSignName, "Супутниковий телефон (абстрактний)", true, objectSignType.Id);
            data.Schema.CreateAttributeType(abstractSatPhoneSignType.Id, BeamPropName, "Луч", ScalarType.String, EmbeddingOptions.Optional);
            data.Schema.CreateAttributeType(abstractSatPhoneSignType.Id, DbObjectPropName, "Об'єкт (локальний)", ScalarType.String, EmbeddingOptions.Optional);

            //change inheritance for iridium sat phone sign
            data.Schema.RemoveInheritance(satIridiumPhoneSignType.Id, satPhoneSignType.Id);
            data.Schema.SetInheritance(satIridiumPhoneSignType.Id, abstractSatPhoneSignType.Id);

            //change inheritance for generic sat phone sign and remove location properties
            data.Schema.RemoveInheritance(satPhoneSignType.Id, objectSignType.Id);
            data.Schema.SetInheritance(satPhoneSignType.Id, abstractSatPhoneSignType.Id);

            var locationXRelationType = satPhoneSignType.GetProperty(LocationXPropName);
            if (locationXRelationType != null)
            {
                data.Schema.RemoveRelation(locationXRelationType.Id);
            }

            var locationYRelationType = satPhoneSignType.GetProperty(LocationYPropName);
            if (locationYRelationType != null)
            {
                data.Schema.RemoveRelation(locationYRelationType.Id);
            }
            var beamRelationType = satPhoneSignType.GetProperty(BeamPropName);
            if (beamRelationType != null)
            {
                data.Schema.RemoveRelation(beamRelationType.Id);
            }

            var dbObjectRelationType = satPhoneSignType.GetProperty(DbObjectPropName);
            if (dbObjectRelationType != null)
            {
                data.Schema.RemoveRelation(dbObjectRelationType.Id);
            }


            SaveOntologySchema(data.Schema);

            //update beam relation node type
            beamRelationType = abstractSatPhoneSignType.GetProperty(BeamPropName);
            dbObjectRelationType = abstractSatPhoneSignType.GetProperty(DbObjectPropName);

            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    UpdateRelationNodeTypeId(iridiumSingBeamRelationIdList, beamRelationType.Id, context);

                    UpdateRelationNodeTypeId(satPhoneBeamRelationIdList, beamRelationType.Id, context);

                    UpdateRelationNodeTypeId(satPhoneDbObjectRelationIdList, dbObjectRelationType.Id, context);

                    var historyToRemoveEntities = context.ChangeHistory
                                                    .Where(e => e.PropertyName == LocationXPropName || e.PropertyName == LocationYPropName)
                                                    .ToArray();

                    context.ChangeHistory.RemoveRange(historyToRemoveEntities);

                    context.SaveChanges();

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
            }

            var rawData = new NodesRawData(context.Nodes, context.Relations, context.Attributes);
            data.ReloadData(rawData);
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

        public void AddEventTitle(OntologyContext context, IOntologyNodesData data)
        {
            var eventType = data.Schema.GetEntityTypeByName("Event");
            if (eventType == null) return;
            if (eventType.GetRelationByName("__title") != null) return;
            data.Schema.CreateAttributeTypeJson(
                eventType.Id,
                "__title",
                "Заголовок",
                ScalarType.String,
                EmbeddingOptions.Optional,
                "{\"Formula\": \"{name};\\\"Об'єкт без назви\\\"\"}"
            );
            SaveOntologySchema(data.Schema);
        }

        public void RemoveDuplicateRelations(OntologyContext context, IOntologyNodesData data)
        {
            var entityNodes = data.GetAllNodes()
                    .Where(n => n.NodeType.Kind == Kind.Entity)
                    .ToList();

            var duplicates = new List<Guid>();

            foreach (var node in entityNodes)
            {
                var relations = node.OutgoingRelations
                    .Where(r => r.RelationKind == RelationKind.Embedding)
                    .OrderBy(r => r.TargetNodeId)
                    .ThenBy(r => r.Node.NodeTypeId)
                    .ToList();

                if (relations.Count <= 1) continue;
                var prev = relations.First();
                for (int i = 1; i < relations.Count; i++)
                {
                    var current = relations[i];
                    if (prev.TargetNodeId == current.TargetNodeId
                        && prev.Node.NodeTypeId == current.Node.NodeTypeId)
                    {
                        duplicates.Add(current.Id);
                        continue;
                    }
                    prev = current;
                }
            }
            data.WriteLock(() =>
            {
                data.RemoveNodes(duplicates);
            });
        }
        public void ClosePersinMultiple(OntologyContext context, IOntologyNodesData data)
        {
            var person = data.Schema.GetEntityTypeByName("Person");
            var closePerson = person?.GetRelationTypeByName("closePersin");
            if (closePerson == null) return;

            data.Schema.UpdateNodeType(new NodeTypeUpdateParameter
            {
                Id = closePerson.Id,
                EmbeddingOptions = EmbeddingOptions.Multiple
            });
            SaveOntologySchema(data.Schema);
        }
        public void MaterialChannel(OntologyContext context, IOntologyNodesData data)
        {
            var materials = context.Materials
                .Where(m => (m.Source.StartsWith("sat.") || m.Source.StartsWith("cell."))
                  && m.Channel == null)
                .ToList();

            foreach (var material in materials)
            {
                var extractor = new MaterialMetadataExtractor(material.Metadata);
                var channel = extractor.Channel;
                if (channel != null)
                {
                    material.Channel = channel;
                }
            }
            context.SaveChanges();
        }

        public void SetupObjectImportanceSortOrder(OntologyContext context, IOntologyNodesData data)
        {
            const string ImportanceCodePropertyName = "code";
            const string SortOrderPropertyName = "sortOrder";
            const string ImportanceTypeName = "ObjectImportance";

            var objectImportanceType = data.Schema.GetEntityTypeByName(ImportanceTypeName);

            if (objectImportanceType == null)
            {
                return;
            }

            var importanceCollection = data.GetNodesByTypeId(objectImportanceType.Id);

            var sortOrderDictionary = new Dictionary<string, string>()
            {
                ["critical"] = "1",
                ["high"] = "2",
                ["medium"] = "3",
                ["normal"] = "4",
                ["low"] = "5",
                ["ignore"] = "6"
            };

            data.WriteLock(() =>
            {
                foreach (var node in importanceCollection)
                {
                    var code = node.GetSingleProperty(ImportanceCodePropertyName)?.Value;

                    if (code is null) continue;

                    if (sortOrderDictionary.TryGetValue(code, out string sortOrder))
                    {
                        data.AddValueByDotName(node.Id, sortOrder, SortOrderPropertyName);
                    }
                }
            });
        }

        public void SeedTestReoData(OntologyContext context, IOntologyNodesData data)
        {
            var phone_23621610312_material = new MaterialEntry
            {
                Id = new Guid("9f8574295fad4fb689487d9a86460390"),
                FileId = new Guid("508e67132a3a4c978754368463dca150"),
                FileName = "Voice_11-11-2023 11-11-21 (1).mp3",
                TimeStamp = DateTime.UtcNow,
                Point = new GeoPoint { Lat = 4.3728127m, Lon = 18.5441459m }
            };

            var phone_23621610534_material = new MaterialEntry
            {
                Id = new Guid("3656e14cdd8c4228970fe9140c28e037"),
                FileId = new Guid("6c66ce3184894f069c67a42bed0bfb52"),
                FileName = "Voice_11-11-2023 11-11-22 (2).mp3",
                TimeStamp = DateTime.UtcNow,
                Point = new GeoPoint { Lat = 4.3664549796904515m, Lon = 18.581554362535677m }
            };

            var phone_23621610711_material = new MaterialEntry
            {
                Id = new Guid("16089ed0828043458e21c1510b0307c3"),
                FileId = new Guid("28096ed1b550467dbb54a0a34e6a2e61"),
                FileName = "Voice_11-11-2023 11-11-23 (3).mp3",
                TimeStamp = DateTime.UtcNow,
                Point = new GeoPoint { Lat = 4.486280909861276m, Lon = 18.50683874821686m }
            };

            var phone_23621622511_material = new MaterialEntry
            {
                Id = new Guid("eabcda857fb14637a083baee1286b20a"),
                FileId = new Guid("4a614057979849678f6d05fae69c2d66"),
                FileName = "Voice_11-11-2023 11-11-24 (4).mp3",
                TimeStamp = DateTime.UtcNow,
                Point = new GeoPoint { Lat = 4.3417462m, Lon = 18.5238899m }
            };

            var phone_23621622512_material = new MaterialEntry
            {
                Id = new Guid("6473244a93554e3daf567f5a1a8befdc"),
                FileId = new Guid("2c2a8b446a8c4215b9a2bc7f011f87ec"),
                FileName = "Voice_11-11-2023 11-11-25 (5).mp3",
                TimeStamp = DateTime.UtcNow,
                Point = new GeoPoint { Lat = 4.3680810276585005m, Lon = 18.589472243547664m }
            };

            CreateMaterial(context, phone_23621610312_material);
            CreateMaterial(context, phone_23621610534_material);
            CreateMaterial(context, phone_23621610711_material);
            CreateMaterial(context, phone_23621622511_material);
            CreateMaterial(context, phone_23621622512_material);

            var phone_23621610312 = GetOrAddSingNode(data, new Guid("0da57278ed074a52bf76673321e5296e"), "+23621610312", "380672444445", "1122334455");
            var phone_23621610534 = GetOrAddSingNode(data, new Guid("5d7719b7db5041f2b048c5cc29fd78f6"), "+23621610534", "380672444448", "1122334459");
            var phone_23621610711 = GetOrAddSingNode(data, new Guid("9b0c60baf594401d95319a10c315f01d"), "+23621610711", "380672444449", "1122334459");
            var phone_23621622511 = GetOrAddSingNode(data, new Guid("ba3ca82f780345729061d66320e15534"), "+23621622511", "380672444446", "1122334456");
            var phone_23621622512 = GetOrAddSingNode(data, new Guid("6959d5ad96234900b8d43d7deacbfa65"), "+23621622512", "380672444447", "1122334457");

            AddSignLocation(context, phone_23621610312.Id, phone_23621610312_material);
            AddSignLocation(context, phone_23621610534.Id, phone_23621610534_material);
            AddSignLocation(context, phone_23621610711.Id, phone_23621610711_material);
            AddSignLocation(context, phone_23621622511.Id, phone_23621622511_material);
            AddSignLocation(context, phone_23621622512.Id, phone_23621622512_material);

            AddSignRelation(context, phone_23621610312_material.Id, new[] { phone_23621610312.Id, phone_23621610534.Id }, phone_23621610312.Id, phone_23621610534.Id);
            AddSignRelation(context, phone_23621622511_material.Id, new[] { phone_23621622511.Id, phone_23621610711.Id }, phone_23621622511.Id, phone_23621610711.Id);
            AddSignRelation(context, phone_23621622512_material.Id, new[] { phone_23621622512.Id }, phone_23621622511.Id, null);
            AddSignRelation(context, phone_23621610711_material.Id, new[] { phone_23621610711.Id }, phone_23621610711.Id, null);
            AddSignRelation(context, phone_23621610534_material.Id, new[] { phone_23621610534.Id }, phone_23621610534.Id, null);

            var firstCompany = GetOrAddObjectNode(data, new Guid("5b30b7dd53244e04aaa16f2977fc810d"), "1 рота 5 батальону ЧВК Вагнера", "hostile", "CF", "normal", "1", phone_23621610312.Id);
            var secondCompany = GetOrAddObjectNode(data, new Guid("46a0ace90e41453b9480ece345365945"), "2 рота 5 батальону ЧВК Вагнера", "hostile", "CF", "normal", "1", phone_23621610312.Id);
            var engineeringPlatoon = GetOrAddObjectNode(data, new Guid("669166d8f2da40c1953ca2e57302b6c2"), "Саперний взвод 5 батальону ЧВК Вагнер", "hostile", "CF", "normal", "1", phone_23621610711.Id);
            var supplyCompany = GetOrAddObjectNode(data, new Guid("bb87f9d1d9444ccf97818b2ebab3dc7d"), "Рота забезпечення 5 батальону ЧВК Вагнера", "hostile", "CF", "normal", "1", phone_23621610534.Id);
            var firstRepGuardPlatoon = GetOrAddObjectNode(data, new Guid("65108afab50947a89813a84def475c3d"), "1 plat. of Republic Guard", "neutral", "CF", "normal", "1", phone_23621622511.Id);
            var presPalaceGuard = GetOrAddObjectNode(data, new Guid("74b73118573b4bef8ff4b768fbe19f22"), "President Palace Guards", "neutral", "CF", "normal", "1", phone_23621622512.Id);
        }

        public void AddSecurityLevels(OntologyContext context, IOntologyNodesData data)
        {
            if (data.Schema.GetEntityTypeByName(EntityTypeNames.SecurityLevel.ToString()) != null) return;

            var securityLevelType = data.Schema.CreateEntityType(EntityTypeNames.SecurityLevel.ToString());
            data.Schema.CreateAttributeType(securityLevelType.Id, "name", "Назва", ScalarType.String, EmbeddingOptions.Required);
            data.Schema.CreateAttributeType(securityLevelType.Id, "uniqueIndex", "Унікальний індекс", ScalarType.Int, EmbeddingOptions.Required);
            data.Schema.CreateRelationType(securityLevelType.Id, securityLevelType.Id, "parent", "Група", EmbeddingOptions.Optional);

            var objectType = data.Schema.GetEntityTypeByName(EntityTypeNames.Object.ToString());
            data.Schema.CreateRelationType(objectType.Id, securityLevelType.Id, "securityLevels", "Рівні доступу", EmbeddingOptions.Multiple);

            SaveOntologySchema(data.Schema);
        }

        public void AddSecurityLevelsData(OntologyContext context, IOntologyNodesData data)
        {
            const string NAME = "name";
            const string PARENT = "parent";
            const string UNIQUE_INDEX = "uniqueIndex";

            var securityLevelType = data.Schema.GetEntityTypeByName(EntityTypeNames.SecurityLevel.ToString());
            var parentRelationType = securityLevelType.GetProperty(PARENT);

            data.WriteLock(() =>
            {
                var rootNode = data.CreateNode(securityLevelType.Id);
                data.AddValueByDotName(rootNode.Id, "Рівні доступу", NAME);
                data.AddValueByDotName(rootNode.Id, 0, UNIQUE_INDEX);

                var regionNode = data.CreateNode(securityLevelType.Id);
                data.AddValueByDotName(regionNode.Id, "Регіон", NAME);
                data.AddValueByDotName(regionNode.Id, 2, UNIQUE_INDEX);
                data.CreateRelation(regionNode.Id, rootNode.Id, parentRelationType.Id);

                var evropaNode = data.CreateNode(securityLevelType.Id);
                data.AddValueByDotName(evropaNode.Id, "Європа", NAME);
                data.AddValueByDotName(evropaNode.Id, 21, UNIQUE_INDEX);
                data.CreateRelation(evropaNode.Id, regionNode.Id, parentRelationType.Id);

                var evropaNode1 = data.CreateNode(securityLevelType.Id);
                data.AddValueByDotName(evropaNode1.Id, "Албанія", NAME);
                data.AddValueByDotName(evropaNode1.Id, 211, UNIQUE_INDEX);
                data.CreateRelation(evropaNode1.Id, evropaNode.Id, parentRelationType.Id);

                var evropaNode2 = data.CreateNode(securityLevelType.Id);
                data.AddValueByDotName(evropaNode2.Id, "Белорусія", NAME);
                data.AddValueByDotName(evropaNode2.Id, 212, UNIQUE_INDEX);
                data.CreateRelation(evropaNode2.Id, evropaNode.Id, parentRelationType.Id);

                var evropaNode3 = data.CreateNode(securityLevelType.Id);
                data.AddValueByDotName(evropaNode3.Id, "Польша", NAME);
                data.AddValueByDotName(evropaNode3.Id, 213, UNIQUE_INDEX);
                data.CreateRelation(evropaNode3.Id, evropaNode.Id, parentRelationType.Id);

                var asiaNode = data.CreateNode(securityLevelType.Id);
                data.AddValueByDotName(asiaNode.Id, "Азія", NAME);
                data.AddValueByDotName(asiaNode.Id, 22, UNIQUE_INDEX);
                data.CreateRelation(asiaNode.Id, regionNode.Id, parentRelationType.Id);

                var asiaNode1 = data.CreateNode(securityLevelType.Id);
                data.AddValueByDotName(asiaNode1.Id, "Туречина", NAME);
                data.AddValueByDotName(asiaNode1.Id, 221, UNIQUE_INDEX);
                data.CreateRelation(asiaNode1.Id, asiaNode.Id, parentRelationType.Id);

                var asiaNode2 = data.CreateNode(securityLevelType.Id);
                data.AddValueByDotName(asiaNode2.Id, "Сірія", NAME);
                data.AddValueByDotName(asiaNode2.Id, 222, UNIQUE_INDEX);
                data.CreateRelation(asiaNode2.Id, asiaNode.Id, parentRelationType.Id);

                var asiaNode3 = data.CreateNode(securityLevelType.Id);
                data.AddValueByDotName(asiaNode3.Id, "Бахрейн", NAME);
                data.AddValueByDotName(asiaNode3.Id, 223, UNIQUE_INDEX);
                data.CreateRelation(asiaNode3.Id, asiaNode.Id, parentRelationType.Id);

                var africaNode = data.CreateNode(securityLevelType.Id);
                data.AddValueByDotName(africaNode.Id, "Афріка", NAME);
                data.AddValueByDotName(africaNode.Id, 23, UNIQUE_INDEX);
                data.CreateRelation(africaNode.Id, regionNode.Id, parentRelationType.Id);

                var categoryNode = data.CreateNode(securityLevelType.Id);
                data.AddValueByDotName(categoryNode.Id, "Категорія", NAME);
                data.AddValueByDotName(categoryNode.Id, 3, UNIQUE_INDEX);
                data.CreateRelation(categoryNode.Id, rootNode.Id, parentRelationType.Id);

                var moveNode = data.CreateNode(securityLevelType.Id);
                data.AddValueByDotName(moveNode.Id, "Переміщення військ", NAME);
                data.AddValueByDotName(moveNode.Id, 31, UNIQUE_INDEX);
                data.CreateRelation(moveNode.Id, categoryNode.Id, parentRelationType.Id);

                var moveNode1 = data.CreateNode(securityLevelType.Id);
                data.AddValueByDotName(moveNode1.Id, "Сухопутні", NAME);
                data.AddValueByDotName(moveNode1.Id, 311, UNIQUE_INDEX);
                data.CreateRelation(moveNode1.Id, moveNode.Id, parentRelationType.Id);

                var moveNode2 = data.CreateNode(securityLevelType.Id);
                data.AddValueByDotName(moveNode2.Id, "Морські", NAME);
                data.AddValueByDotName(moveNode2.Id, 312, UNIQUE_INDEX);
                data.CreateRelation(moveNode2.Id, moveNode.Id, parentRelationType.Id);

                var moveNode3 = data.CreateNode(securityLevelType.Id);
                data.AddValueByDotName(moveNode3.Id, "Космічні", NAME);
                data.AddValueByDotName(moveNode3.Id, 313, UNIQUE_INDEX);
                data.CreateRelation(moveNode3.Id, moveNode.Id, parentRelationType.Id);
            });
        }

        private static INode GetOrAddSingNode(IOntologyNodesData data, Guid signId, string phoneNumber, string imei, string tmsi)
        {
            const string singTypeName = "CellphoneSign";

            var node = data.GetNode(signId);

            if (node != null) return node;

            var nodeType = data.Schema.GetEntityTypeByName(singTypeName);

            node = data.CreateNode(nodeType.Id, signId);

            data.WriteLock(() =>
            {
                data.AddValueByDotName(node.Id, phoneNumber, "value");
                data.AddValueByDotName(node.Id, imei, "imei");
                data.AddValueByDotName(node.Id, tmsi, "tmsi");
            });

            return node;
        }

        private static INode GetOrAddObjectNode(IOntologyNodesData data, Guid nodeId, string name, string affilationCode, string countryCode, string importanceCode, string accessLevelIndex, Guid signId)
        {
            const string militaryOrganizationTypeName = "MilitaryOrganization";
            const string objectAffiliationTypeName = "ObjectAffiliation";
            const string objectImportanceTypeName = "ObjectImportance";
            const string countryTypeName = "Country";
            const string CodeProperyName = "code";
            const string NumericIndexPropertyName = "numericIndex";

            var node = data.GetNode(nodeId);

            if (node != null) return node;

            var nodeType = data.Schema.GetEntityTypeByName(militaryOrganizationTypeName);
            var countryType = data.Schema.GetEntityTypeByName(countryTypeName);
            var objectAffiliationType = data.Schema.GetEntityTypeByName(objectAffiliationTypeName);
            var objectImportanceType = data.Schema.GetEntityTypeByName(objectImportanceTypeName);
            var accessLevelType = data.Schema.GetEntityTypeByName(EntityTypeNames.AccessLevel.ToString());

            var affilationRelationType = nodeType.GetProperty("affiliation");
            var countryRelationType = nodeType.GetProperty("country");
            var importanceRelationType = nodeType.GetProperty("importance");
            var accessLevelRelationType = nodeType.GetProperty("accessLevel");
            var signRelationType = nodeType.GetProperty("sign");

            var affilation = data.GetNodesByUniqueValue(objectAffiliationType.Id, affilationCode, CodeProperyName)
                                    .FirstOrDefault();

            var country = data.GetNodesByUniqueValue(countryType.Id, countryCode, CodeProperyName)
                                .FirstOrDefault();

            var importance = data.GetNodesByUniqueValue(objectImportanceType.Id, importanceCode, CodeProperyName)
                                .FirstOrDefault();

            var accessLevel = data.GetNodesByUniqueValue(accessLevelType.Id, accessLevelIndex, NumericIndexPropertyName)
                                .FirstOrDefault();

            node = data.CreateNode(nodeType.Id, nodeId);

            data.WriteLock(() =>
            {
                data.AddValueByDotName(node.Id, name, "commonInfo.OpenName");
                data.AddValueByDotName(node.Id, name, "commonInfo.RealNameExtended");

                data.CreateRelation(node.Id, affilation.Id, affilationRelationType.Id);
                data.CreateRelation(node.Id, country.Id, countryRelationType.Id);
                data.CreateRelation(node.Id, importance.Id, importanceRelationType.Id);
                data.CreateRelation(node.Id, accessLevel.Id, accessLevelRelationType.Id);
                data.CreateRelation(node.Id, signId, signRelationType.Id);
            });

            return node;
        }

        private static void CreateMaterial(OntologyContext context, MaterialEntry entry)
        {
            var file = CreateFileEntity(entry);

            context.Files.Add(file);

            var material = CreateMaterialEntity(entry);

            context.Materials.Add(material);

            context.SaveChanges();
        }

        private static FileEntity CreateFileEntity(MaterialEntry entry)
        {
            return new FileEntity
            {
                Id = entry.FileId,
                Name = entry.FileName,
                ContentType = "multipart/form-data",
                ContentHash = Guid.NewGuid(),
                UploadTime = entry.TimeStamp,
                IsTemporary = false
            };
        }

        private static MaterialEntity CreateMaterialEntity(MaterialEntry entry)
        {
            return new MaterialEntity
            {
                Id = entry.Id,
                FileId = entry.FileId,
                Source = "cell.voice",
                Type = "audio",
                Metadata = "{\"type\":\"audio\",\"source\":\"cell.voice\"}",
                CreatedDate = entry.TimeStamp,
                ProcessedStatusSignId = MaterialEntity.ProcessingStatusNotProcessedSignId,
                Content = entry.FileName,
                MlHandlersCount = 0,
                AccessLevel = 0,
                UpdatedAt = entry.TimeStamp
            };
        }

        private static void AddSignLocation(OntologyContext context, Guid signId, MaterialEntry materialEntry)
        {
            var location = new LocationHistoryEntity
            {
                Id = Guid.NewGuid(),
                Lat = materialEntry.Point.Lat,
                Long = materialEntry.Point.Lon,
                RegisteredAt = materialEntry.TimeStamp,
                EntityId = signId,
                NodeId = signId,
                MaterialId = materialEntry.Id,
                Type = Interfaces.Enums.LocationType.Node
            };

            context.LocationHistory.Add(location);

            context.SaveChanges();
        }

        private static void AddSignRelation(OntologyContext context, Guid materialId, Guid[] featureIdArray, Guid? callerSignId, Guid? receiverSignId)
        {
            foreach (var featureId in featureIdArray)
            {
                context.MaterialFeatures.Add(MaterialFeatureEntity.CreateFrom(materialId, featureId));
            }

            if (callerSignId.HasValue)
            {
                context.MaterialFeatures.Add(MaterialFeatureEntity.CreateFrom(materialId, callerSignId.Value, MaterialNodeLinkType.Caller));
            }

            if (receiverSignId.HasValue)
            {
                context.MaterialFeatures.Add(MaterialFeatureEntity.CreateFrom(materialId, receiverSignId.Value, MaterialNodeLinkType.Receiver));
            }

            context.SaveChanges();
        }

        private class MaterialEntry
        {
            public Guid Id { get; set; }
            public Guid FileId { get; set; }
            public string FileName { get; set; }
            public DateTime TimeStamp { get; set; }
            public GeoPoint Point { get; set; }
        }

        private class GeoPoint
        {
            public decimal Lat { get; set; }
            public decimal Lon { get; set; }
        }
    }
}
