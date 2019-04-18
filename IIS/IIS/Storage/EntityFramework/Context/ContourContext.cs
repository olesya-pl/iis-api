using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

//Enum column 'public.EntityAttributes.type' cannot be scaffolded, define a CLR enum type and add the property manually.
//Enum column 'public.EntityTypes.type' cannot be scaffolded, define a CLR enum type and add the property manually.
//Index 'entity_types_code_type' on table public.EntityTypes cannot be scaffolded because it includes a column that cannot be scaffolded(e.g. enum).

namespace IIS.Storage.EntityFramework.Context
{
    public partial class ContourContext : DbContext
    {
        public ContourContext(DbContextOptions options)
            : base(options)
        {
        }

        public virtual DbSet<Entities> Entities { get; set; }
        public virtual DbSet<EntityAttachments> EntityAttachments { get; set; }
        public virtual DbSet<EntityAttributeValues> EntityAttributeValues { get; set; }
        public virtual DbSet<EntityAttributes> EntityAttributes { get; set; }
        public virtual DbSet<EntityRelationRestrictions> EntityRelationRestrictions { get; set; }
        public virtual DbSet<EntityRelations> EntityRelations { get; set; }
        public virtual DbSet<EntityTypeAttributes> EntityTypeAttributes { get; set; }
        public virtual DbSet<EntityTypes> EntityTypes { get; set; }
        public virtual DbSet<SequelizeData> SequelizeData { get; set; }
        public virtual DbSet<SequelizeMeta> SequelizeMeta { get; set; }
        public virtual DbSet<TemporaryFiles> TemporaryFiles { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ForNpgsqlHasEnum(null, "enum_EntityAttributes_type", new[] { "string", "int", "decimal", "date", "boolean", "geo", "file", "json" })
                .ForNpgsqlHasEnum(null, "enum_EntityTypes_type", new[] { "entity", "relation" })
                .ForNpgsqlHasEnum(null, "enum_Proofs_relationType", new[] { "entity", "attribute" })
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity<Entities>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.DeletedAt)
                    .HasColumnName("deletedAt")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.TypeId).HasColumnName("typeId");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("timestamp with time zone");
            });

            modelBuilder.Entity<EntityAttachments>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DeletedAt)
                    .HasColumnName("deletedAt")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.File)
                    .IsRequired()
                    .HasColumnName("file");
            });

            modelBuilder.Entity<EntityAttributeValues>(entity =>
            {
                entity.HasIndex(e => new { e.EntityId, e.DeletedAt })
                    .HasName("entity_attribute_values_entity_id_deleted_at");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AttributeId).HasColumnName("attributeId");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.DeletedAt)
                    .HasColumnName("deletedAt")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.EntityId).HasColumnName("entityId");

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasColumnName("value");
            });

            modelBuilder.Entity<EntityAttributes>(entity =>
            {
                entity.HasIndex(e => e.Code)
                    .HasName("EntityAttributes_code_key")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasColumnName("code")
                    .HasMaxLength(255);

                entity.Property(e => e.Meta)
                    .HasColumnName("meta")
                    .HasColumnType("jsonb");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnName("title")
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<EntityRelationRestrictions>(entity =>
            {
                entity.HasIndex(e => new { e.RelationTypeId, e.InitiatorTypeId, e.TargetTypeId })
                    .HasName("entity_relation_restrictions_relation_type_id_initiator_type_id")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.InitiatorTypeId).HasColumnName("initiatorTypeId");

                entity.Property(e => e.Meta)
                    .HasColumnName("meta")
                    .HasColumnType("json");

                entity.Property(e => e.RelationTypeId).HasColumnName("relationTypeId");

                entity.Property(e => e.TargetTypeId).HasColumnName("targetTypeId");
            });

            modelBuilder.Entity<EntityRelations>(entity =>
            {
                entity.HasIndex(e => new { e.TypeId, e.InitiatorId, e.TargetId, e.DeletedAt })
                    .HasName("entity_relations_type_id_initiator_id_target_id_deleted_at")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.DeletedAt)
                    .HasColumnName("deletedAt")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.EndsAt)
                    .HasColumnName("endsAt")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.InitiatorId).HasColumnName("initiatorId");

                entity.Property(e => e.IsInferred).HasColumnName("isInferred");

                entity.Property(e => e.StartsAt)
                    .HasColumnName("startsAt")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.TargetId).HasColumnName("targetId");

                entity.Property(e => e.TypeId).HasColumnName("typeId");
            });

            modelBuilder.Entity<EntityTypeAttributes>(entity =>
            {
                entity.HasIndex(e => new { e.TypeId, e.AttributeId })
                    .HasName("entity_type_attributes_type_id_attribute_id")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AttributeId).HasColumnName("attributeId");

                entity.Property(e => e.Meta)
                    .HasColumnName("meta")
                    .HasColumnType("jsonb");

                entity.Property(e => e.TypeId).HasColumnName("typeId");
            });

            modelBuilder.Entity<EntityTypes>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasColumnName("code")
                    .HasMaxLength(255);

                entity.Property(e => e.IsAbstract).HasColumnName("isAbstract");

                entity.Property(e => e.Meta)
                    .HasColumnName("meta")
                    .HasColumnType("jsonb");

                entity.Property(e => e.ParentId).HasColumnName("parentId");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnName("title")
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<SequelizeData>(entity =>
            {
                entity.HasKey(e => e.Name)
                    .HasName("SequelizeData_pkey");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .ValueGeneratedNever();
            });

            modelBuilder.Entity<SequelizeMeta>(entity =>
            {
                entity.HasKey(e => e.Name)
                    .HasName("SequelizeMeta_pkey");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .ValueGeneratedNever();
            });

            modelBuilder.Entity<TemporaryFiles>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.File).HasColumnName("file");

                entity.Property(e => e.Meta)
                    .HasColumnName("meta")
                    .HasColumnType("json");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnName("title")
                    .HasMaxLength(255);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnName("type")
                    .HasMaxLength(255);

                entity.Property(e => e.UserId).HasColumnName("userId");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasIndex(e => e.Username)
                    .HasName("Users_username_key")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(255);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password")
                    .HasMaxLength(255);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasColumnName("username")
                    .HasMaxLength(255);
            });
        }
    }
}
