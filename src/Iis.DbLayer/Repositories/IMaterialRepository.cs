using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Iis.DataModel.Materials;
using Iis.DbLayer.MaterialEnum;

namespace Iis.DbLayer.Repositories
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
        
        /// <summary>
        /// Returns all the Materials
        /// </summary>
        /// <param name="includes">define relation include</param>
        Task<IEnumerable<MaterialEntity>> GetAllAsync(params MaterialIncludeEnum[] includes);
        
        /// <summary>
        /// Returns all the Materials for given Assignee
        /// </summary>
        /// <param name="assigneeId">Assignee Id</param>
        Task<IEnumerable<MaterialEntity>> GetAllByAssigneeIdAsync(Guid assigneeId);

    }
}