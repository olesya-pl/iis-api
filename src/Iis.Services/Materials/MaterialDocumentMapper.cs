using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Iis.DataModel.Materials;
using Iis.DbLayer.Repositories;
using Iis.Domain;
using Iis.Domain.Materials;
using Iis.Domain.Users;
using Iis.Interfaces.Common;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services;
using Newtonsoft.Json.Linq;

namespace IIS.Services.Materials
{
    public class MaterialDocumentMapper
    {
        private readonly IMapper _mapper;
        private readonly IOntologyService _ontologyService;
        private readonly IOntologySchema _ontologySchema;
        private readonly NodeToJObjectMapper _nodeToJObjectMapper;

        public MaterialDocumentMapper(
            IMapper mapper,
            IOntologySchema ontologySchema,
            IOntologyService ontologyService,
            NodeToJObjectMapper nodeToJObjectMapper)
        {
            _mapper = mapper;
            _ontologyService = ontologyService;
            _ontologySchema = ontologySchema;
            _nodeToJObjectMapper = nodeToJObjectMapper;
        }

        public Material Map(MaterialDocument document)
        {
            var material = _mapper.Map<Material>(document);

            material.Children = document.Children
                                            .Select(_ => _mapper.Map<Material>(_))
                                            .ToList();

            var nodeCollection = document.NodeIds
                                    .Select(_ => _ontologyService.GetNode(_))
                                    .Where(_ => _ != null)
                                    .ToArray();

            material.Events = nodeCollection
                                .Where(_ => _.OriginalNode.NodeType.IsEvent)
                                .Select(_ => _nodeToJObjectMapper.EventToJObject(_));

            material.Features = nodeCollection
                                .Where(_ => _.OriginalNode.NodeType.IsObjectSign)
                                .Select(_nodeToJObjectMapper.NodeToJObject);

            var (ObjectsOfStudy, ObjectsOfStudyCount) = GetObjectOfStudyListForMaterial(nodeCollection);

            material.ObjectsOfStudy = ObjectsOfStudy;

            return material;
        }

        public Material Map(MaterialDocument document, Guid userId)
        {
            var material = Map(document);

            material.CanBeEdited = document.ProcessedStatus.Id == MaterialEntity.ProcessingStatusProcessingSignId
                ? (document.Editor == null || document.Editor.Id == userId)
                : true;

            return material;
        }

        public Material Map(MaterialEntity material)
        {
            if (material == null) return null;

            var result = _mapper.Map<Material>(material);

            result.Infos.AddRange(MapInfos(material));

            result.Children.AddRange(MapChildren(material));

            result.Editor = _mapper.Map<User>(material.Editor);

            var featureCollection = result.Infos
                                    .SelectMany(_ => _.Features)
                                    .ToArray();

            var nodeCollection = featureCollection
                                    .Select(_ => _.Node)
                                    .ToArray();

            result.Caller = GetIdTitleForLinkType(featureCollection, MaterialNodeLinkType.Caller);

            result.Receiver = GetIdTitleForLinkType(featureCollection, MaterialNodeLinkType.Receiver);

            result.Events = nodeCollection
                                .Where(_ => _.OriginalNode.NodeType.IsEvent)
                                .Select(_ => _nodeToJObjectMapper.EventToJObject(_));

            result.Features = nodeCollection
                                .Where(_ => _.OriginalNode.NodeType.IsObjectSign)
                                .Select(_ => _nodeToJObjectMapper.NodeToJObject(_));

            var (ObjectsOfStudy, ObjectsOfStudyCount) = GetObjectOfStudyListForMaterial(nodeCollection);

            result.ObjectsOfStudy = ObjectsOfStudy;

            result.ObjectsOfStudyCount = ObjectsOfStudyCount;

            return result;
        }

        public IReadOnlyCollection<MaterialInfo> MapInfos(MaterialEntity material) => material.MaterialInfos?.Select(_ => Map(_)).ToArray() ?? Array.Empty<MaterialInfo>();

        private static JProperty CreateJProperty(Guid id, string value)
        {
            return new JProperty(id.ToString("N"), value);
        }

        private static SubscriberDto GetIdTitleForLinkType(IReadOnlyCollection<MaterialFeature> materialFeatureCollection, MaterialNodeLinkType linkType)
        {
            var node = materialFeatureCollection
                        .FirstOrDefault(_ => _.NodeLinkType == linkType)
                        ?.Node.OriginalNode;

            return node is null ? null :
                new SubscriberDto
                {
                    Id = node.Id,
                    Title = node.GetTitleValue(),
                    NodeTypeName = node.NodeType.Name
                };
        }

        private (JObject List, int Count) GetObjectOfStudyListForMaterial(IReadOnlyCollection<Node> nodeList)
        {
            var result = new JObject();
            if (nodeList.Count == 0) return (result, 0);

            var directIdList = nodeList
                .Where(_ => _.OriginalNode.NodeType.IsObjectOfStudy)
                .Select(_ => _.Id)
                .ToArray();

            var featureIdList = nodeList
                .Where(_ => _.OriginalNode.NodeType.IsObjectSign)
                .Select(_ => _.Id)
                .ToArray();

            var relatedIdList = _ontologyService.GetNodeIdListByFeatureIdList(featureIdList)
                .Except(directIdList)
                .ToArray();

            var featureList = relatedIdList
                .Select(_ => CreateJProperty(_, EntityMaterialRelation.Feature));

            var directList = directIdList
                .Select(_ => CreateJProperty(_, EntityMaterialRelation.Direct));

            result.Add(featureList);
            result.Add(directList);

            return (List: result, directIdList.Length + relatedIdList.Length);
        }

        private IReadOnlyCollection<Material> MapChildren(MaterialEntity material)
        {
            if (material.Children == null)
            {
                return Array.Empty<Material>();
            }
            return material.Children.Select(child => Map(child)).ToArray();
        }

        private MaterialInfo Map(MaterialInfoEntity info)
        {
            var result = new MaterialInfo(info.Id, JObject.Parse(info.Data), info.Source, info.SourceType, info.SourceVersion);
            foreach (var feature in info.MaterialFeatures)
                result.Features.Add(Map(feature));
            return result;
        }

        private MaterialFeature Map(MaterialFeatureEntity feature)
        {
            var result = _mapper.Map<MaterialFeature>(feature);
            result.Node = _ontologyService.GetNode(feature.NodeId);
            return result;
        }
    }
}
