using System.Threading;
using Iis.DataModel.Analytics;
using Iis.DataModel.Materials;
using Iis.DataModel.Reports;
using Microsoft.EntityFrameworkCore;

namespace Iis.DataModel
{
    public class OntologyContext : DbContext
    {
        public SemaphoreSlim Semaphore = new SemaphoreSlim(1, int.MaxValue);

        public DbSet<NodeTypeEntity> NodeTypes { get; set; }
        public DbSet<RelationTypeEntity> RelationTypes { get; set; }
        public DbSet<AttributeTypeEntity> AttributeTypes { get; set; }

        public DbSet<NodeEntity> Nodes { get; set; }
        public DbSet<RelationEntity> Relations { get; set; }
        public DbSet<AttributeEntity> Attributes { get; set; }

        public DbSet<FileEntity> Files { get; set; }
        public DbSet<MaterialEntity> Materials { get; set; }
        public DbSet<MaterialInfoEntity> MaterialInfos { get; set; }
        public DbSet<MaterialFeatureEntity> MaterialFeatures { get; set; }
        public DbSet<MaterialSignTypeEntity> MaterialSignTypes { get; set; }
        public DbSet<MaterialSignEntity> MaterialSigns { get; set; }

        public DbSet<ReportEntity> Reports { get; set; }
        public DbSet<ReportEventEntity> ReportEvents { get; set; }

        public DbSet<UserEntity> Users { get; set; }

        public DbSet<AnalyticQueryEntity> AnalyticQueries { get; set; }
        public DbSet<AnalyticIndicatorEntity> AnalyticIndicators { get; set; }
        public DbSet<AnalyticQueryIndicatorEntity> AnalyticQueryIndicators { get; set; }

        public DbSet<OntologyMigrationsEntity> OntologyMigrations { get; set; }

        public OntologyContext(DbContextOptions<OntologyContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new NodeTypeConfiguration());
            modelBuilder.ApplyConfiguration(new RelationTypeConfiguration());
            modelBuilder.ApplyConfiguration(new AttributeTypeConfiguration());

            modelBuilder.ApplyConfiguration(new NodeConfiguration());
            modelBuilder.ApplyConfiguration(new RelationConfiguration());
            modelBuilder.ApplyConfiguration(new AttributeConfiguration());

            modelBuilder.ApplyConfiguration(new FileConfiguration());
            modelBuilder.ApplyConfiguration(new MaterialConfiguration());
            modelBuilder.ApplyConfiguration(new MaterialInfoConfiguration());
            modelBuilder.ApplyConfiguration(new MaterialFeatureConfiguration());
            modelBuilder.ApplyConfiguration(new MaterialSignTypeConfiguration());
            modelBuilder.ApplyConfiguration(new MaterialSignConfiguration());

            modelBuilder.ApplyConfiguration(new ReportEventConfiguration());

            modelBuilder.ApplyConfiguration(new UserConfiguration());

            modelBuilder.ApplyConfiguration(new AnalyticQueryEntityConfiguration());
            modelBuilder.ApplyConfiguration(new AnalyticQueryIndicatorConfiguration());
            modelBuilder.ApplyConfiguration(new AnalyticIndicatorConfiguration());
            modelBuilder.ApplyConfiguration(new OntologyMigrationsConfiguration());
        }
    }
}
