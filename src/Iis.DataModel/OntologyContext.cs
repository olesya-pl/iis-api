using Iis.DataModel.Analytics;
using Iis.DataModel.ChangeHistory;
using Iis.DataModel.Elastic;
using Iis.DataModel.Materials;
using Iis.DataModel.Reports;
using Iis.DataModel.Roles;
using Iis.DataModel.Themes;
using Iis.DataModel.Annotations;
using Microsoft.EntityFrameworkCore;
using Iis.DataModel.FlightRadar;

namespace Iis.DataModel
{
    public class OntologyContext : DbContext
    {
        public OntologyContext(DbContextOptions<OntologyContext> options)
            : base(options)
        {
        }

        public DbSet<NodeTypeEntity> NodeTypes { get; set; }
        public DbSet<RelationTypeEntity> RelationTypes { get; set; }
        public DbSet<AttributeTypeEntity> AttributeTypes { get; set; }
        public DbSet<AliasEntity> Aliases { get; set; }

        public DbSet<NodeEntity> Nodes { get; set; }
        public DbSet<RelationEntity> Relations { get; set; }
        public DbSet<AttributeEntity> Attributes { get; set; }

        public DbSet<FileEntity> Files { get; set; }
        public DbSet<MaterialEntity> Materials { get; set; }
        public DbSet<MaterialAssigneeEntity> MaterialAssignees { get; set; }
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

        public DbSet<RoleEntity> Roles { get; set; }

        public DbSet<RoleAccessEntity> RoleAccess { get; set; }

        public DbSet<AccessObjectEntity> AccessObjects { get; set; }

        public DbSet<UserRoleEntity> UserRoles { get; set; }

        public DbSet<RoleActiveDirectoryGroupEntity> RoleGroups { get; set; }

        public DbSet<MLResponseEntity> MLResponses { get; set; }

        public DbSet<ElasticFieldEntity> ElasticFields { get; set; }

        public DbSet<ChangeHistoryEntity> ChangeHistory { get; set; }

        public DbSet<ThemeEntity> Themes { get; set; }

        public DbSet<ThemeTypeEntity> ThemeTypes { get; set; }

        public DbSet<AnnotationEntity> Annotations { get; set; }

        public DbSet<LocationHistoryEntity> LocationHistory { get; set; }

        public DbSet<FlightRadarHistorySyncJobConfig> FlightRadarHistorySyncJobConfig { get; set; }

        public DbSet<TowerLocationEntity> TowerLocations { get; set; }

        public DbSet<ModifyDataLogEntity> ModifyDataLogs { get; set; }
        public DbSet<MaterialChannelMappingEntity> MaterialChannelMappings { get; set; }

        public static OntologyContext GetContext(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<OntologyContext>()
                .UseNpgsql(connectionString);
            var context = new OntologyContext(optionsBuilder.Options);
            return context;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly<OntologyContext>(GetType());
        }
    }
}