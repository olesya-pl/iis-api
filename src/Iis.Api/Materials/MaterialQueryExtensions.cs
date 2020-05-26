using System;
using System.Linq;
using System.Linq.Expressions;
using Iis.DataModel.Materials;

namespace IIS.Core.Materials.EntityFramework
{
    public static class MaterialQueryExtensions
    {
        public static IQueryable<MaterialEntity> ApplySorting(
            this IQueryable<MaterialEntity> materialsQuery,
            string sortColumnName,
            string sortOrder)
        {
            Expression<Func<MaterialEntity, IComparable>> compareExpression = null;

            compareExpression = sortColumnName switch
            {
                "type" => p => p.Type,
                "source" => p => p.Source,
                "title" => p => p.Title,
                "importance" => p => p.ImportanceSignId,
                "nodes" => p => p.MaterialInfos.SelectMany(p => p.MaterialFeatures).Count(),
                _ => p => p.CreatedDate,
            };
            return string.Equals(sortOrder, "asc", StringComparison.OrdinalIgnoreCase)
                ? materialsQuery.OrderBy(compareExpression)
                : materialsQuery.OrderByDescending(compareExpression);
        }

        public static IQueryable<MaterialEntity> GetParentMaterialsQuery(
            this IQueryable<MaterialEntity> materialQuery)
        {
            return materialQuery
                    .Where(p => p.ParentId == null);
        }
    }
}
