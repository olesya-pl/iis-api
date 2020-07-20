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
        private const int MaxResultWindow = 10000;
        private readonly OntologyContext _context;
        private readonly IOntologyService _ontologyService;
        private readonly IOntologySchema _ontologySchema;
        private readonly IElasticService _elasticService;
        private readonly IMapper _mapper;
        private readonly IMLResponseRepository _mLResponseRepository;
        private readonly IMaterialRepository _materialRepository;
        private readonly IMaterialSignRepository _materialSignRepository;
        private readonly IOntologyRepository _ontologyRepository;

        public MaterialProvider(OntologyContext context,
            IOntologyService ontologyService,
            IOntologySchema ontologySchema,
            IElasticService elasticService,
            IMLResponseRepository mLResponseRepository,
            IMaterialRepository materialRepository,
            IMaterialSignRepository materialSignRepository,
            IOntologyRepository ontologyRepository,
            IMapper mapper)
        {
            _context = context;
            _ontologyService = ontologyService;
            _ontologySchema = ontologySchema;
            _elasticService = elasticService;

            _mLResponseRepository = mLResponseRepository;
            _materialRepository = materialRepository;
            _materialSignRepository = materialSignRepository;
            _ontologyRepository = ontologyRepository;
            _mapper = mapper;
        }

        public async Task<(IEnumerable<Material> Materials, int Count, Dictionary<Guid, SearchByConfiguredFieldsResultItem> Highlights)>
            GetMaterialsAsync(int limit, int offset, string filterQuery,
            IEnumerable<Guid> nodeIds = null, IEnumerable<string> types = null,
            string sortColumnName = null, string sortOrder = null)
        {
            await _context.Semaphore.WaitAsync();

            try
            {
                IEnumerable<Task<Material>> mappingTasks;
                IEnumerable<Material> materials;
                (IEnumerable<MaterialEntity> Materials, int TotalCount) materialResult;

                if (!string.IsNullOrWhiteSpace(filterQuery))
                {
                    if (!_elasticService.UseElastic)
                    {
                        return (new List<Material>(), 0, new Dictionary<Guid, SearchByConfiguredFieldsResultItem>());
                    }

                    var filter = new ElasticFilter { Limit = MaxResultWindow, Offset = 0, Suggestion = filterQuery };

                    var searchResult = await _elasticService.SearchByConfiguredFieldsAsync(_elasticService.MaterialIndexes, filter);

                    var matchedIdList = searchResult.Items.Keys.ToList();

                    materialResult = await _materialRepository.GetAllParentsAsync(matchedIdList, limit, offset, sortColumnName, sortOrder);

                    mappingTasks = materialResult.Materials
                                    .Select(async entity => await MapAsync(entity));

                    materials = await Task.WhenAll(mappingTasks);

                    return (materials, materialResult.TotalCount, searchResult.Items);
                }

                if(types != null)
                {
                    materialResult = await _materialRepository.GetAllParentsAsync(types, limit, offset, sortColumnName, sortOrder);
                }
                else
                {
                    materialResult = await _materialRepository.GetAllParentsAsync(limit, offset, sortColumnName, sortOrder);
                }

                mappingTasks = materialResult.Materials
                                    .Select(async entity => await MapAsync(entity));

                materials = await Task.WhenAll(mappingTasks);

                materials = await UpdateProcessedMLHandlersCount(materials);

                return (materials, materialResult.TotalCount, new Dictionary<Guid, SearchByConfiguredFieldsResultItem>());
            }
            finally
            {
                _context.Semaphore.Release();
            }
        }

        public async Task<IEnumerable<MaterialEntity>> GetMaterialEntitiesAsync()
        {
            return await _materialRepository.GetAllAsync();
        }

        public async Task<Material> GetMaterialAsync(Guid id)
        {
            var entity = await _materialRepository.GetByIdAsync(id, MaterialIncludeEnum.WithChildren, MaterialIncludeEnum.WithFeatures);
            
            return await MapAsync(entity);
        }

        public IReadOnlyCollection<MaterialSignEntity> GetMaterialSigns(string typeName)
        {
            return _materialSignRepository.GetAllByTypeName(typeName);
        }

        public MaterialSign GetMaterialSign(Guid id)
        {
            var entity = _materialSignRepository.GetById(id);

            return _mapper.Map<MaterialSign>(entity);
        }

        public MaterialSign GetMaterialSign(string signValue)
        {
            var entity = _materialSignRepository.GetByValue(signValue);

            if (entity is null) return null;

            return _mapper.Map<MaterialSign>(entity);
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

        public async Task<IEnumerable<MaterialsCountByType>> CountMaterialsByTypeAndNodeAsync(Guid nodeId)
        {
            var nodeIdList = await GetNodeIdRelatedToNodeIdAsync(nodeId);

            var materialEntities = await _materialRepository.GetAllParentsOnlyForRelatedNodeListAsync(nodeIdList);

            return materialEntities
                    .GroupBy(p => p.Type)
                    .Select(group => new MaterialsCountByType
                    {
                        Count = group.Count(),
                        Type = group.Key
                    })
                    .ToList();
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

            var nodeIdList = await GetNodeIdRelatedToNodeIdAsync(nodeId);

            var materialEntities = await _materialRepository.GetAllForRelatedNodeListAsync(nodeIdList);

            mappingTasks = materialEntities
                                 .Select(async e => await MapAsync(e));

            materials = await Task.WhenAll(mappingTasks);

            return (materials, materials.Count());
        }

        private async Task<Material> MapAsync(MaterialEntity material)
        {
            if (material == null) return null;

            var result = _mapper.Map<Material>(material);

            result.Infos.AddRange(await MapInfos(material));

            result.Children.AddRange(await MapChildren(material));

            result.Assignee = _mapper.Map<User>(material.Assignee);

            var nodes = result.Infos
                                .SelectMany(p => p.Features.Select(x => x.Node))
                                .ToList();

            result.Events = nodes.Where(x => IsEvent(x)).Select(x => EventToJObject(x));

            result.Features = nodes.Where(x => IsObjectSign(x)).Select(x => NodeToJObject(x));

            result.ObjectsOfStudy = await GetObjectOfStudyListForMaterial(nodes);

            return result;
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

        private JObject EventToJObject(Node node)
        {
            var result = new JObject(new JProperty(nameof(node.Id).ToLower(), node.Id.ToString("N")));

            var attributies = node.GetChildAttributes().Where(a => a.dotName == "name" || a.dotName == "description");

            foreach (var attribute in attributies)
            {
                result.Add(new JProperty(attribute.dotName, attribute.attribute.Value));
            }

            return result;
        }

        private async Task<IEnumerable<Material>> UpdateProcessedMLHandlersCount(IEnumerable<Material> materials)
        {
            var materialIds = Array.AsReadOnly(materials.Select(p => p.Id).ToArray());

            var mlResults = await _mLResponseRepository.GetAllForMaterialsAsync(materialIds);

            materials.Join(
                mlResults,
                m => m.Id,
                ml => ml.MaterialId,
                (material, result) => {
                    material.ProcessedMlHandlersCount = result.Count;
                    return material;
                }).ToList();

            return materials;
        }
        private async Task<IEnumerable<Guid>> GetNodeIdRelatedToNodeIdAsync(Guid nodeId)
        {
            var resultList = new List<Guid>
            {
                nodeId
            };

            var featureIdList = await _ontologyRepository.GetFeatureIdListRelatedToNodeIdAsync(nodeId);

            resultList.AddRange(featureIdList);

            return resultList; 
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
