using System;
using System.Threading.Tasks;
using Iis.DataModel.Materials;

using Iis.DbLayer.MaterialEnum;

namespace Iis.DbLayer.Repository
{
    /// <summary>
    /// Defines repository that provides methods to work Material entity(s)
    /// </summary>
    public interface IMaterialRepository
    {
        /// <summary>
        /// Returns entity of Material by its ID
        /// </summary>
        /// <param name="id">Material ID</param>
        /// <param name="includes">define relation include</param>
        Task<MaterialEntity> GetByIdAsync(Guid id, params MaterialIncludeEnum[] includes);
    }
}