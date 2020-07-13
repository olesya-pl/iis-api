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
        /// Returns all the Materials with paggination
        /// </summary>
        /// <param name="limit">page limit</param>
        /// <param name="offset">page offset</param>
        /// <param name="sortColumnName">sorting ColumnName</param>
        /// <param name="offset">sorting Order</param>
        /// <returns></returns>
        Task<(IEnumerable<MaterialEntity> Entities, int TotalCount)> GetAllAsync(int limit, int offset, string sortColumnName = null, string sortOrder = null);
        
        /// <summary>
        /// Returns all the Materials for given list of Material Type with paggination
        /// </summary>
        /// <param name="types">given list of Material Type</param>
        /// <param name="limit">page limit</param>
        /// <param name="offset">page offset</param>
        /// <param name="sortColumnName">sorting ColumnName</param>
        /// <param name="offset">sorting Order</param>
        Task<(IEnumerable<MaterialEntity> Entities, int TotalCount)> GetAllAsync(IEnumerable<string> types, int limit, int offset, string sortColumnName = null, string sortOrder = null);
        
        /// <summary>
        /// Returns all the Materials for given Assignee
        /// </summary>
        /// <param name="assigneeId">Assignee Id</param>
        Task<IEnumerable<MaterialEntity>> GetAllByAssigneeIdAsync(Guid assigneeId);
    }
}