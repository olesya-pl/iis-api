using System.Linq;
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
            return (sortColumnName, sortOrder)
            switch
            {
                ("type", "asc") => materialsQuery.OrderBy(p => p.Type),
                ("type", "desc") => materialsQuery.OrderByDescending(p => p.Type),
                ("source", "asc") => materialsQuery.OrderBy(p => p.Source),
                ("source", "desc") => materialsQuery.OrderByDescending(p => p.Source),
                ("title", "asc") => materialsQuery.OrderBy(p => p.Title),
                ("title", "desc") => materialsQuery.OrderByDescending(p => p.Title),
                ("importance", "asc") => materialsQuery.OrderBy(p => p.Importance),
                ("importance", "desc") => materialsQuery.OrderByDescending(p => p.Importance),

                ("nodes", "asc") => materialsQuery
                    .OrderBy(p => p.MaterialInfos.SelectMany(p => p.MaterialFeatures).Count()),
                ("nodes", "desc") => materialsQuery
                    .OrderByDescending(p => p.MaterialInfos.SelectMany(p => p.MaterialFeatures).Count()),

                ("createdDate", "asc") => materialsQuery.OrderBy(p => p.CreatedDate),
                _ => materialsQuery.OrderByDescending(p => p.CreatedDate),
            };
        }

        public static IQueryable<MaterialEntity> GetParentMaterialsQuery(
            this IQueryable<MaterialEntity> materialQuery)
        {
            return materialQuery
                    .Where(p => p.ParentId == null);
        }
    }
}
