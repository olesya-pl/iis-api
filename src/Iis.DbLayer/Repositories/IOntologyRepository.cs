using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Iis.DbLayer.Repositories
{
    /// <summary>
    /// Defines repository that provides methods to work with Ontology
    /// </summary>
    public interface IOntologyRepository
    {
        /// <summary>
        /// Returns list of Feature Id that related to given Node Id
        /// </summary>
        /// <param name="nodeId"></param>
        Task<IEnumerable<Guid>> GetFeatureIdListRelatedToNodeIdAsync(Guid nodeId);
        
        /// <summary>
        /// Returns list of pair Feature Id with appropriate NodeId for given list of FeatureId
        /// </summary>
        /// <param name="featureIdList">list of FeatureId</param>
        /// <returns></returns>
        Task<IEnumerable<(Guid FeatureId, Guid NodeId)>> GetNodeIdListRelatedToFeatureIdListAsync(IEnumerable<Guid> featureIdList);
    }
}