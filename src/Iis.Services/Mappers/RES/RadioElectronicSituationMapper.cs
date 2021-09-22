using System;
using Iis.DataModel.Materials;
using Iis.DataModel.FlightRadar;
using Iis.Interfaces.Ontology.Data;
using Iis.Services.Contracts.Dtos;

namespace Iis.Services.Mappers.RadioElectronicSituation
{
    public static class RadioElectronicSituationMapper
    {
        private const string ValuePropertyName = "value";
        private const string TitlePropertyName = "__title";
        private const string SidcPropertyName = "affiliation.sidc";
        private const string NoValueFound = "значення відсутне";
        public static SituationNodeDto Map(
            LocationHistoryEntity locationEntity,
            INode signNode,
            INode objectNode,
            MaterialEntity materialEntity)
        {
            var geometry = new GeometryDto(locationEntity.Lat, locationEntity.Long);

            var objectOfStudy = MapObjectOfStudyDto(objectNode);

            var sign = MapObjectSign(signNode);

            var material = MapMaterial(materialEntity);

            var attributies = new AttributesDto(objectOfStudy, sign, material, locationEntity.RegisteredAt);

            return new SituationNodeDto(geometry, attributies);
        }

        private static ObjectDto MapObjectOfStudyDto(INode objectNode)
        {
            var title = objectNode.GetComputedValue(TitlePropertyName) ?? NoValueFound;
            var sidc = objectNode.GetSingleProperty(SidcPropertyName)?.Value ?? NoValueFound;
            var type = objectNode.NodeType;

            return new ObjectDto(objectNode.Id, type.Name, type.Title, title, sidc);

        }

        private static SignDto MapObjectSign(INode signNode)
        {
            var type = signNode.NodeType;
            var value = signNode.GetSingleProperty(ValuePropertyName)?.Value ?? NoValueFound;

            return new SignDto(signNode.Id, type.Name, type.Title, value);
        }

        private static MaterialDto MapMaterial(MaterialEntity entity)
        {
            if(entity is null) return null;

            var title = entity.File?.Name;

            return new MaterialDto(entity.Id, entity.Type, entity.Source, title, entity.CreatedDate, entity.RegistrationDate);
        }
    }
}