using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Iis.DataModel.Materials;

namespace Iis.DbLayer.Repositories
{
    /// <summary>
    /// Defines repository that provides methods to work ML Response entity(s)
    /// </summary>
    public interface IMLResponseRepository
    {
        /// <summary>
        /// Returns all the Machine Learning Results for given Material
        /// </summary>
        /// <param name="materialId">given Material Id</param>
        Task<IEnumerable<MLResponseEntity>> GetAllForMaterialAsync(Guid materialId);
        
        /// <summary>
        /// Returns all the Machine Learning Results for given Materials
        /// </summary>
        /// <param name="materialIdList">list of given Material Id</param>
        /// <returns></returns>
        Task<IEnumerable<(Guid MaterialId, int Count)>> GetAllForMaterialsAsync(IReadOnlyCollection<Guid> materialIdList); 
        
        /// <summary>
        /// Save Machine Learning Results
        /// </summary>
        /// <param name="entity">Machine Learning Results</param>
        Task<MLResponseEntity> SaveAsync(MLResponseEntity entity);
    }
}