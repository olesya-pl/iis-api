using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Iis.DataModel.Materials;
using Iis.DbLayer.Repositories;
using Iis.Domain;
using Iis.Domain.Materials;
using Iis.Domain.Users;
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

        public MaterialDocumentMapper(IMapper mapper,
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
                                            .Select(_mapper.Map<Material>)
                                            .ToList();

            var nodes = document.NodeIds
                                    .Select(_ontologyService.GetNode)
                                    .ToArray();

            material.Events = nodes
                                .Where(IsEvent)
                                .Select(_nodeToJObjectMapper.EventToJObject);

            material.Features = nodes
                                .Where(IsObjectSign)
                                .Select(_nodeToJObjectMapper.NodeToJObject);

            material.ObjectsOfStudy = GetObjectOfStudyListForMaterial(nodes);

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

            var nodes = result.Infos
                                .SelectMany(p => p.Features.Where(e => e.NodeLinkType == MaterialNodeLinkType.None).Select(e => e.Node))
                                .ToList();

            result.Events = nodes
                                .Where(IsEvent)
                                .Select(_nodeToJObjectMapper.EventToJObject);

            result.Features = nodes
                                .Where(IsObjectSign)
                                .Select(_nodeToJObjectMapper.NodeToJObject);

            result.ObjectsOfStudy = GetObjectOfStudyListForMaterial(nodes);

            return result;
        }

        private JObject GetObjectOfStudyListForMaterial(IReadOnlyCollection<Node> nodeList)
        {
            var result = new JObject();
            if (nodeList.Count == 0)
                return result;

            var directIdList = nodeList
                .Where(x => IsObjectOfStudy(x))
                .Select(x => x.Id)
                .ToArray();
            var featureIdList = nodeList
                .Where(x => IsObjectSign(x))
                .Select(x => x.Id)
                .ToArray();
            var featureList = _ontologyService.GetNodeIdListByFeatureIdList(featureIdList)
                .Except(directIdList)
                .Select(_ => CreateJProperty(_, EntityMaterialRelation.Feature));
            var directList = directIdList
                .Select(_ => CreateJProperty(_, EntityMaterialRelation.Direct));

            result.Add(featureList);
            result.Add(directList);

            return result;
        }

        private JProperty CreateJProperty(Guid id, string value)
        {
            return new JProperty(id.ToString("N"), value);
        }

        private IReadOnlyCollection<Material> MapChildren(MaterialEntity material)
        {
            if (material.Children == null)
            {
                return Array.Empty<Material>();
            }
            return material.Children.Select(child => Map(child)).ToArray();
        }

        public IReadOnlyCollection<MaterialInfo> MapInfos(MaterialEntity material)
        {
            var mapInfoTasks = new List<MaterialInfo>();
            foreach (var info in material.MaterialInfos ?? new List<MaterialInfoEntity>())
            {
                mapInfoTasks.Add(Map(info));
            }
            return mapInfoTasks;
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

        public bool IsEvent(Node node)
        {
            if (node is null) return false;

            var nodeType = _ontologySchema.GetNodeTypeById(node.Type.Id);

            return nodeType.IsEvent;
        }

        public bool IsObjectOfStudy(Node node)
        {
            if (node is null) return false;

            var nodeType = _ontologySchema.GetNodeTypeById(node.Type.Id);

            return nodeType.IsObjectOfStudy;
        }

        public bool IsObjectSign(Node node)
        {
            if (node is null) return false;

            var nodeType = _ontologySchema.GetNodeTypeById(node.Type.Id);

            return nodeType.IsObjectSign;
        }
    }
}
