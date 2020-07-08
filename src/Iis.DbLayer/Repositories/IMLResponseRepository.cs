using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Iis.DataModel.Materials;

namespace Iis.DbLayer.Repositories
{
    /// <summary>
    /// Defines repository that provides methods to work MLRespo entity(s)
    /// </summary>
    public interface IMLResponseRepository
    {
        /// <summary>
        /// Returns all the Machine Learning Results for given Material
        /// </summary>
        /// <param name="materialId">given Material Id</param>
        Task<IEnumerable<MLResponseEntity>> GetAllForMaterialAsync(Guid materialId);

        /// <summary>
        /// Save Machine Learning Results
        /// </summary>
        /// <param name="entity">Machine Learning Results</param>
        Task<MLResponseEntity> SaveAsync(MLResponseEntity entity);
    }
}