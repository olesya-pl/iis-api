using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using AutoMapper;

using Iis.Utility;
using Iis.Roles;
using Iis.Domain;
using Iis.Domain.Materials;
using Iis.Domain.MachineLearning;
using Iis.DataModel;
using Iis.DataModel.Cache;
using Iis.DataModel.Materials;
using Iis.DbLayer.Repositories;
using Iis.DbLayer.MaterialEnum;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;

namespace IIS.Core.Materials.EntityFramework
{
    public class MaterialProvider : IMaterialProvider
    {
        private readonly OntologyContext _context;
        private readonly IOntologyService _ontologyService;
        private readonly IOntologySchema _ontologySchema;
        private readonly IOntologyCache _cache;
        private readonly IElasticService _elasticService;
        private readonly IMapper _mapper;
        private readonly IMLResponseRepository _mLResponseRepository;
        private readonly IMaterialRepository _materialRepository;

        public MaterialProvider(OntologyContext context,
            IOntologyService ontologyService,
            IOntologySchema ontologySchema,
            IElasticService elasticService,
            IOntologyCache cache,
            IMLResponseRepository mLResponseRepository,
            IMaterialRepository materialRepository,
            IMapper mapper)
        {
            _context = context;
            _ontologyService = ontologyService;
            _ontologySchema = ontologySchema;
            _elasticService = elasticService;
            _cache = cache;

            _mLResponseRepository = mLResponseRepository;
            _materialRepository = materialRepository;
            _mapper = mapper;
        }

        public async Task<(
            IEnumerable<Material> Materials,
            int Count,
            Dictionary<Guid, SearchByConfiguredFieldsResultItem> Highlights)> GetMaterialsAsync(
            int limit, int offset, string filterQuery,
            IEnumerable<Guid> nodeIds = null, IEnumerable<string> types = null,
            string sortColumnName = null, string sortOrder = null)
        {
            await _context.Semaphore.WaitAsync();

            try
            {
                IQueryable<MaterialEntity> materialsQuery
                    = GetMaterialQuery()
                    .WithChildren()
                    .WithFeatures()
                    .GetParentMaterialsQuery()
                    .ApplySorting(sortColumnName, sortOrder);
                IQueryable<MaterialEntity> materialsCountQuery;
                IEnumerable<Task<Material>> mappingTasks;
                IEnumerable<Material> materials;
                
                if (!string.IsNullOrWhiteSpace(filterQuery))
                {
                    if (!_elasticService.UseElastic)
                    {
                        return (new List<Material>(), 0, new Dictionary<Guid, SearchByConfiguredFieldsResultItem>());
                    }

                    const int MaxResultWindow = 10000;

                    var searchResult = await _elasticService.SearchByConfiguredFieldsAsync(
                        _elasticService.MaterialIndexes,
                        new ElasticFilter { Limit = MaxResultWindow, Offset = 0, Suggestion = filterQuery });

                    var foundIds = searchResult.Items.Keys.ToList();

                    materialsCountQuery = materialsQuery = materialsQuery
                                        .Where(e => foundIds.Contains(e.Id));

                    mappingTasks = (await materialsQuery
                                        .Skip(offset)
                                        .Take(limit)
                                        .ToArrayAsync())
                                            .Select(async entity => await MapAsync(entity));

                    materials = await Task.WhenAll(mappingTasks);

                    var count = await materialsCountQuery.CountAsync();

                    return (materials, count, searchResult.Items);
                }

                (IEnumerable<MaterialEntity> Materials, int TotalCount) materialResult;

                if(types != null)
                {
                    materialResult = await _materialRepository.GetAllAsync(types, limit, offset);
                } 
                else
                {
                    materialResult = await _materialRepository.GetAllAsync(limit, offset);
                }

                mappingTasks = materialResult.Materials
                                    .Select(async entity => await MapAsync(entity));

                materials = await Task.WhenAll(mappingTasks);

                PopulateProcessedMlHandlersCount(materials);

                var materialsCount = materialResult.TotalCount;

                return (materials, materialsCount, new Dictionary<Guid, SearchByConfiguredFieldsResultItem>());
            }
            finally
            {
                _context.Semaphore.Release();
            }
        }

        public async Task<Material> GetMaterialAsync(Guid id)
        {
            var entity = await _materialRepository.GetByIdAsync(id, MaterialIncludeEnum.WithChildren, MaterialIncludeEnum.WithFeatures);

            return await MapAsync(entity);
        }

        public async Task<IEnumerable<MaterialEntity>> GetMaterialEntitiesAsync()
        {
            return await _materialRepository.GetAllAsync();
        }

        public IReadOnlyCollection<MaterialSignEntity> GetMaterialSigns(string typeName)
        {
            return _cache.MaterialSigns
                .Where(ms => ms.MaterialSignType.Name == typeName)
                .OrderBy(ms => ms.OrderNumber)
                .ToList();
        }

        public MaterialSign GetMaterialSign(Guid id)
        {
            var materialSignEntity = _cache.GetMaterialSign(id);
            return _mapper.Map<MaterialSign>(materialSignEntity);
        }

        public MaterialSign GetMaterialSign(string signValue)
        {
            if (string.IsNullOrWhiteSpace(signValue)) return null;

            var entity = _cache.MaterialSigns
                            .FirstOrDefault(ms => ms.Title == signValue);

            if (entity is null) return null;

            return _mapper.Map<MaterialSign>(entity);
        }

        public async Task<Material> MapAsync(MaterialEntity material)
        {
            if (material == null) return null;

            var result = _mapper.Map<Material>(material);

            result.Infos.AddRange(await MapInfos(material));

            result.Children.AddRange(await MapChildren(material));

            result.Assignee = _mapper.Map<User>(material.Assignee);

            var nodes = result.Infos
                                .SelectMany(p => p.Features.Select(x => x.Node))
                                .ToList();

            result.Events = nodes.Where(x => IsEvent(x));

            result.Events2 = nodes.Where(x => IsEvent(x)).Select(x => NodeToJObject(x));

            result.Features = nodes.Where(x => IsObjectSign(x)).Select(x => NodeToJObject(x));

            result.ObjectsOfStudy = await GetObjectOfStudyListForMaterial(nodes);

            return result;
        }

        public async Task<List<MlProcessingResult>> GetMLProcessingResultsAsync(Guid materialId)
        {
            var entities = await _mLResponseRepository.GetAllForMaterialAsync(materialId);

            return _mapper.Map<List<MlProcessingResult>>(entities);
        }

        public async Task<(List<Material> Materials, int Count)> GetMaterialsByAssigneeIdAsync(Guid assigneeId)
        {
            var entities = await _materialRepository.GetAllByAssigneeIdAsync(assigneeId);

            var materials = _mapper.Map<List<Material>>(entities);

            return (materials, materials.Count());
        }

        public Task<List<MaterialsCountByType>> CountMaterialsByTypeAndNodeAsync(Guid nodeId)
        {
            return GetMaterialByNodeIdQuery(nodeId)
                .GetParentMaterialsQuery()
                .GroupBy(p => p.Type)
                .Select(group => new MaterialsCountByType
                {
                    Count = group.Count(),
                    Type = group.Key
                })
                .ToListAsync();
        }

        public async Task<JObject> GetMaterialDocumentAsync(Guid materialId)
        {
            var materialTask = GetSimplifiedMaterialAsync(materialId);

            var mLResponsesTask = GetMLProcessingResultsAsync(materialId);

            await Task.WhenAll(materialTask, mLResponsesTask);

            var material = await materialTask;
            var mlResponses = await mLResponsesTask;

            var jDocument = new JObject(
                new JProperty(nameof(Material.Source).ToLower(), material.Source),
                new JProperty(nameof(Material.Type).ToLower(), material.Type),
                new JProperty(nameof(Material.Content).ToLower(), material.Content),
                new JProperty(nameof(Material.CreatedDate).ToLower(), material.CreatedDate),
                new JProperty(nameof(Material.Importance).ToLower(), material.Importance?.Title),
                new JProperty(nameof(Material.Reliability).ToLower(), material.Reliability?.Title),
                new JProperty(nameof(Material.Relevance).ToLower(), material.Relevance?.Title),
                new JProperty(nameof(Material.Completeness).ToLower(), material.Completeness?.Title),
                new JProperty(nameof(Material.SourceReliability).ToLower(), material.SourceReliability?.Title),
                new JProperty(nameof(Material.ProcessedStatus).ToLower(), material.ProcessedStatus?.Title),
                new JProperty(nameof(Material.SessionPriority).ToLower(), material.SessionPriority?.Title),
                new JProperty(nameof(Material.Id).ToLower(), material.Id.ToString("N"))
            );

            if (!string.IsNullOrWhiteSpace(material.Title))
            {
                jDocument.Add(nameof(material.Title), material.Title);
            }
            if (!(material.Data is null) && material.Data.HasValues)
            {
                var materialData = new JObject();
                material.Data
                    .Select(token => new JProperty(token.Value<string>("Type"), token.Value<string>("Text")))
                    .Select(property =>
                    {
                        if(materialData.ContainsKey(property.Name))
                        {
                            materialData[property.Name] = property.Value;
                        } else
                        {
                            materialData.Add(property);
                        }
                        return true;
                    })
                    .ToList();
                jDocument.Add(new JProperty(nameof(Material.Data), materialData));
            }

            if (!(material.Metadata is null))
            {
                jDocument.Add(new JProperty(nameof(Material.Metadata), material.Metadata));
            }

            if (!(material.LoadData is null))
            {
                jDocument.Add(new JProperty(nameof(Material.LoadData), JObject.Parse(material.LoadData.ToJson())));
            }

            if (mlResponses.Any())
            {
                var mlResponsesContainer = new JObject();
                jDocument.Add(new JProperty(nameof(mlResponses), mlResponsesContainer));
                var mlHandlers = mlResponses.GroupBy(_ => _.MlHandlerName).Select(_ => _.Key).ToArray();
                foreach (var mlHandler in mlHandlers)
                {
                    var responses = mlResponses.Where(_ => _.MlHandlerName == mlHandler).ToArray();
                    for (var i = 0; i < responses.Count(); i++)
                    {
                        var propertyName = $"{mlHandler}-{i + 1}";

                        mlResponsesContainer.Add(new JProperty(propertyName.ToLowerCamelCase().RemoveWhiteSpace(),
                            responses[i].ResponseText));
                    }
                }
            }

            if (material.Assignee != null)
            {
                jDocument.Add(
                    new JProperty(nameof(Material.Assignee),
                    SerializeAssignee(material.Assignee)));
            }


            return jDocument;
        }

        public async Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsByNodeIdQuery(Guid nodeId)
        {
            IEnumerable<Task<Material>> mappingTasks;
            IEnumerable<Material> materials;

            IQueryable<MaterialEntity> materialsByNode = GetMaterialByNodeIdQuery(nodeId);

            mappingTasks = (await materialsByNode
                                 .ToArrayAsync())
                                 .Select(async e => await MapAsync(await _materialRepository.GetByIdAsync(e.Id, MaterialIncludeEnum.WithChildren, MaterialIncludeEnum.WithFeatures)));

            materials = await Task.WhenAll(mappingTasks);

            return (materials, materials.Count());
        }

        private async Task<Material> GetSimplifiedMaterialAsync(Guid id)
        {
            var material = await _materialRepository.GetByIdAsync(id, MaterialIncludeEnum.WithChildren);

            return MapSimplifiedMaterial(material);
        }

        private Material MapSimplifiedMaterial(MaterialEntity material)
        {
            if (material == null) return null;
            var result = _mapper.Map<Material>(material);
            result.Assignee = _mapper.Map<User>(material.Assignee);
            return result;
        }

        private JObject SerializeAssignee(User assignee)
        {
            var res = new JObject();
            res[nameof(User.Id)] = assignee.Id.ToString("N");
            res[nameof(User.UserName)] = assignee.UserName;
            res[nameof(User.FirstName)] = assignee.FirstName;
            res[nameof(User.LastName)] = assignee.LastName;
            res[nameof(User.Patronymic)] = assignee.Patronymic;
            return res;
        }

        private bool IsEvent(Node node)
        {
            var nodeType = _ontologySchema.GetNodeTypeById(node.Type.Id);

            return nodeType.IsEvent;
        }

        private bool IsObjectOfStudy(Node node)
        {
            var nodeType = _ontologySchema.GetNodeTypeById(node.Type.Id);

            return nodeType.IsObjectOfStudy;
        }

        private bool IsObjectSign(Node node)
        {
            var nodeType = _ontologySchema.GetNodeTypeById(node.Type.Id);

            return nodeType.IsObjectSign;
        }

        private JObject NodeToJObject(Node node)
        {
            var result = new JObject(new JProperty(nameof(node.Id).ToLower(), node.Id.ToString("N")));

            foreach (var attribute in node.GetChildAttributes())
            {
                result.Add(new JProperty(attribute.dotName, attribute.attribute.Value));
            }

            return result;
        }

        private void PopulateProcessedMlHandlersCount(IEnumerable<Material> materials)
        {
            var materialIds = materials.Select(p => p.Id).ToArray();

            foreach (var item in _context.MLResponses.Where(p => materialIds.Contains(p.MaterialId))
                 .GroupBy(p => p.MaterialId)
                 .Select(p => new { id = p.Key, count = p.Count() }))
            {
                var material = materials.FirstOrDefault(p => p.Id == item.id);
                if (material == null)
                {
                    continue;
                }
                material.ProcessedMlHandlersCount = item.count;
            }
        }

        private IQueryable<MaterialEntity> GetMaterialByNodeIdQuery(Guid nodeId)
        {
            var nodeIdList = GetFeatureIdListThatRealtesToObjectId(nodeId);

            nodeIdList.Add(nodeId);

            return _context.Materials
                        .Join(_context.MaterialInfos, m => m.Id, mi => mi.MaterialId,
                            (Material, MaterialInfo) => new { Material, MaterialInfo })
                        .Join(_context.MaterialFeatures, m => m.MaterialInfo.Id, mf => mf.MaterialInfoId,
                            (MaterialInfoJoined, MaterialFeature) => new { MaterialInfoJoined, MaterialFeature })
                        .Where(m => nodeIdList.Contains(m.MaterialFeature.NodeId))
                        .Select(m => m.MaterialInfoJoined.Material);
        }
        
        private IList<Guid> GetFeatureIdListThatRealtesToObjectId(Guid nodeId)
        {
            var type = _ontologySchema.GetEntityTypeByName("ObjectSign");

            var typeIdList = new List<Guid>();

            if (type != null)
            {
                typeIdList = type.IncomingRelations
                                    .Select(p => p.SourceTypeId)
                                    .ToList();
            }

            return _context.Nodes
                                .Join(_context.Relations, n => n.Id, r => r.TargetNodeId, (node, relation) => new { Node = node, Relation = relation })
                                .Where(e => (!typeIdList.Any() ? true : typeIdList.Contains(e.Node.NodeTypeId)) && e.Relation.SourceNodeId == nodeId)
                                .AsNoTracking()
                                .Select(e => e.Node.Id)
                                .ToList();
        }

        private async Task<JObject> GetObjectOfStudyListForMaterial(List<Node> nodeList)
        {
            var result = new JObject();

            if (!nodeList.Any()) return result;

            var directIdList = nodeList
                                .Where(x => IsObjectOfStudy(x))
                                .Select(x => x.Id)
                                .ToList();

            var featureIdList = nodeList
                                    .Where(x => IsObjectSign(x))
                                    .Select(x => x.Id)
                                    .ToList();

            var featureList = await _ontologyService.GetNodeIdListByFeatureIdListAsync(featureIdList);

            featureList = featureList.Except(directIdList).ToList();

            result.Add(featureList.Select(i => new JProperty(i.ToString("N"), EntityMaterialRelation.Feature)));
            result.Add(directIdList.Select(i => new JProperty(i.ToString("N"), EntityMaterialRelation.Direct)));

            return result;
        }

        private IQueryable<MaterialEntity> GetMaterialQuery()
        {
            return _context.Materials
                    .AsNoTracking()
                    .Include(m => m.Importance)
                    .Include(m => m.Reliability)
                    .Include(m => m.Relevance)
                    .Include(m => m.Completeness)
                    .Include(m => m.SourceReliability)
                    .Include(m => m.ProcessedStatus)
                    .Include(m => m.SessionPriority)
                    .Include(m => m.Assignee)
                    .AsNoTracking();
        }

        private async Task<Material[]> MapChildren(MaterialEntity material)
        {
            var mapChildrenTasks = new List<Task<Material>>();
            foreach (var child in material.Children ?? new List<MaterialEntity>())
            {
                mapChildrenTasks.Add(MapAsync(child));
            }
            return await Task.WhenAll(mapChildrenTasks);
        }

        private async Task<MaterialInfo[]> MapInfos(MaterialEntity material)
        {
            var mapInfoTasks = new List<Task<MaterialInfo>>();
            foreach (var info in material.MaterialInfos ?? new List<MaterialInfoEntity>())
            {
                mapInfoTasks.Add(MapAsync(info));
            }
            return await Task.WhenAll(mapInfoTasks);
        }

        private async Task<MaterialInfo> MapAsync(MaterialInfoEntity info)
        {
            var result = new MaterialInfo(info.Id, JObject.Parse(info.Data), info.Source, info.SourceType, info.SourceVersion);
            foreach (var feature in info.MaterialFeatures)
                result.Features.Add(await MapAsync(feature));
            return result;
        }

        private async Task<MaterialFeature> MapAsync(MaterialFeatureEntity feature)
        {
            var result = new MaterialFeature(feature.Id, feature.Relation, feature.Value);
            result.Node = await _ontologyService.LoadNodesAsync(feature.NodeId, null);
            return result;
        }
    }
}
