using System.Linq;
using Microsoft.EntityFrameworkCore;

using Iis.DataModel.Materials;

namespace Iis.DbLayer.Extensions
{
    internal static class MaterialQueryExtensions
    {
        public static IQueryable<MaterialEntity> WithChildren(
            this IQueryable<MaterialEntity> materialQuery)
        {
            return materialQuery.Include(m => m.Children);
        }

        public static IQueryable<MaterialEntity> WithFeatures(
            this IQueryable<MaterialEntity> materialQuery)
        {
            return materialQuery
                .Include(m => m.MaterialInfos)
                .ThenInclude(m => m.MaterialFeatures);
        }
    }
}