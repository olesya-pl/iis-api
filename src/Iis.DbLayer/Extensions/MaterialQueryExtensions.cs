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

        public static IQueryable<MaterialEntity> WithNodes(
            this IQueryable<MaterialEntity> materialQuery)
        {
            return materialQuery
                .Include(m => m.MaterialInfos)
                .ThenInclude(m => m.MaterialFeatures)
                .ThenInclude(m => m.Node);
        }

        public static IQueryable<MaterialEntity> OnlyParent(
            this IQueryable<MaterialEntity> materialQuery)
        {
            return materialQuery
                    .Where(p => p.ParentId == null);
        }
        public static IQueryable<MaterialEntity> ApplySorting(
            this IQueryable<MaterialEntity> materialsQuery,
            string sortColumnName,
            string sortOrder)
        {
            var orderedQueryable = (sortColumnName, sortOrder)
            switch
            {
                ("type", "asc") => materialsQuery.OrderBy(p => p.Type),
                ("type", "desc") => materialsQuery.OrderByDescending(p => p.Type),
                ("source", "asc") => materialsQuery.OrderBy(p => p.Source),
                ("source", "desc") => materialsQuery.OrderByDescending(p => p.Source),
                ("title", "asc") => materialsQuery.OrderBy(p => p.Title),
                ("title", "desc") => materialsQuery.OrderByDescending(p => p.Title),
                ("importance", "asc") => materialsQuery.OrderBy(p => p.ImportanceSignId),
                ("importance", "desc") => materialsQuery.OrderByDescending(p => p.ImportanceSignId),

                ("nodes", "asc") => materialsQuery
                    .OrderBy(p => p.MaterialInfos.SelectMany(p => p.MaterialFeatures).Count()),
                ("nodes", "desc") => materialsQuery
                    .OrderByDescending(p => p.MaterialInfos.SelectMany(p => p.MaterialFeatures).Count()),

                ("createdDate", "asc") => materialsQuery.OrderBy(p => p.CreatedDate),
                _ => materialsQuery.OrderByDescending(p => p.CreatedDate),
            };
            return orderedQueryable.ThenBy(p => p.Id);
        }

    }
}