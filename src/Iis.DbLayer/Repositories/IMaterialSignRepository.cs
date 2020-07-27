using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Iis.DataModel.Materials;

namespace Iis.DbLayer.Repositories
{
    /// <summary>
    /// Defines repository that provides methods to work MaterialSign entity(s)
    /// </summary>
    public interface IMaterialSignRepository
    {
        /// <summary>
        /// Returns MaterialSign by identifier
        /// </summary>
        /// <param name="id">MaterialSign identifier</param>
        MaterialSignEntity GetById(Guid id);

        /// <summary>
        /// Returns MaterialSign by given value
        /// </summary>
        /// <param name="value">value</param>
        MaterialSignEntity GetByValue(string value);

        /// <summary>
        /// Returns list of MaterialSign for given Type name
        /// </summary>
        /// <param name="typeName">Type name</param>
        IReadOnlyCollection<MaterialSignEntity> GetAllByTypeName(string typeName);
    }
}