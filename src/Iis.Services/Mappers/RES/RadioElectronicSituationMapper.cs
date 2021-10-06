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
        private const string NickNamePropertyName = "nicknameSign.value";
        private const string NoValueFound = "значення відсутне";

        public static SituationNodeDto MapSituationNode(GeometryDto geometry, AttributesDto attributes)
        {
            return new SituationNodeDto(geometry, attributes);
        }

        public static ObjectDto MapObjectOfStudy(INode objectNode, INode signNode, MaterialEntity material)
        {
            var title = objectNode.GetComputedValue(TitlePropertyName) ?? NoValueFound;
            var sidc = objectNode.GetSingleProperty(SidcPropertyName)?.Value ?? NoValueFound;
            var nickName = objectNode.GetSingleProperty(NickNamePropertyName)?.Value ?? NoValueFound;
            var type = objectNode.NodeType;

            return new ObjectDto(objectNode.Id, type.Name, type.Title, title, sidc, nickName, signNode.Id, material?.Id, material?.RegistrationDate);

        }

        public static SignDto MapObjectSign(INode signNode, MaterialEntity material)
        {
            var type = signNode.NodeType;
            var value = signNode.GetSingleProperty(ValuePropertyName)?.Value ?? NoValueFound;

            return new SignDto(signNode.Id, type.Name, type.Title, value, material?.Id, material?.RegistrationDate);
        }

        public static MaterialDto MapMaterial(MaterialEntity entity)
        {
            if(entity is null) return null;

            var title = entity.File?.Name;

            return new MaterialDto(entity.Id, entity.Type, entity.Source, title, entity.CreatedDate, entity.RegistrationDate);
        }
    }
}