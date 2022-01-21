using Iis.MaterialDistributor.DataModel.Entities;
using Iis.MaterialDistributor.DataModel.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Iis.MaterialDistributor.DataModel.Contexts
{
    public class MaterialDistributorContext : DbContext
    {
        public MaterialDistributorContext(DbContextOptions<MaterialDistributorContext> options)
            : base(options)
        {
        }

        public virtual DbSet<PermanentCriteriaEntity> PermanentCriterias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly<MaterialDistributorContext>(GetType());
        }
    }
}