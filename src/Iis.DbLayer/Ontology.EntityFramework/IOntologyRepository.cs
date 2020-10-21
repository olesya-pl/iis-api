using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.Domain;
using Iis.Interfaces.Ontology.Data;

namespace Iis.DbLayer.Ontology.EntityFramework
{
    public interface IOntologyRepository
    {
        Task<List<NodeEntity>> GetNodesWithSuggestionAsync(IEnumerable<Guid> derived, ElasticFilter filter);
        Task<List<NodeEntity>> GetNodesByUniqueValue(Guid nodeTypeId, string value, string valueTypeName);
        Task<List<RelationEntity>> GetIncomingRelationsAsync(IEnumerable<Guid> entityIdList, IEnumerable<string> relationTypeNameList);
    }
}