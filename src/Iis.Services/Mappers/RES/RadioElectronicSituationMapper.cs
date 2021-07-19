using System;
using Iis.DataModel.FlightRadar;
using Iis.Interfaces.Ontology.Data;
using Iis.Services.Contracts.Dtos;

namespace Iis.Services.Mappers.RadioElectronicSituation
{
    public static class RadioElectronicSituationMapper
    {
        private const string TitlePropertyName = "__title";
        private const string SidcPropertyName = "affiliation.sidc";
        private const string NoValueFound = "значення відсутне";
        public static SituationNodeDto Map(LocationHistoryEntity entity, INode node)
        {
            var title = node.GetComputedValue(TitlePropertyName) ?? NoValueFound;
            var sidc = node.GetSingleProperty(SidcPropertyName)?.Value ?? NoValueFound;
            var geometry = new GeometryDto(entity.Lat, entity.Long);
            var attributes = new AttributesDto(node.Id, node.NodeType.Name, node.NodeType.Title, title, sidc, entity.RegisteredAt);
            return new SituationNodeDto(attributes, geometry);
        }
    }
}