using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Iis.MaterialDistributor.DataModel.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static ModelBuilder ApplyConfigurationsFromAssembly<TDbContext>(this ModelBuilder modelBuilder, params Type[] markerTypes)
            where TDbContext : DbContext
        {
            foreach (var type in markerTypes)
            {
                modelBuilder.ApplyConfigurationsFromAssembly(type.Assembly, _ => _.GetInterfaces()
                    .Any(_ => _.IsGenericType && _.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                        && _.TryGetAttribute<DbContextAttribute>(out var attribute)
                        && attribute.ContextType == typeof(TDbContext));
            }

            return modelBuilder;
        }

        private static bool TryGetAttribute<TAttribute>(this Type type, out TAttribute attribute)
            where TAttribute : Attribute
        {
            attribute = type.GetCustomAttribute<TAttribute>();

            return attribute != null;
        }
    }
}