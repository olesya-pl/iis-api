using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

//Enum column 'public.EntityAttributes.type' cannot be scaffolded, define a CLR enum type and add the property manually.
//Enum column 'public.EntityTypes.type' cannot be scaffolded, define a CLR enum type and add the property manually.
//done
//Constraint 'EntityTypes_code_type_key' on table public.EntityTypes cannot be scaffolded because it includes a column that cannot be scaffolded(e.g. enum).
//Index 'EntityTypes_code_type_key' on table public.EntityTypes cannot be scaffolded because it includes a column that cannot be scaffolded(e.g. enum).

namespace IIS.Storage.EntityFramework.Context
{
    public partial class ContourContext : DbContext
    {
        public ContourContext()
        {
        }

        public ContourContext(DbContextOptions options) : base(options) { }

        public virtual DbSet<Entity> Entities { get; set; }
        public virtual DbSet<Attachment> Attachments { get; set; }
        public virtual DbSet<AttributeValue> AttributeValues { get; set; }
        public virtual DbSet<EntityAttribute> EntityAttributes { get; set; }
        public virtual DbSet<RelationRestriction> RelationRestrictions { get; set; }
        public virtual DbSet<Relation> Relations { get; set; }
        public virtual DbSet<EntityTypeAttribute> EntityTypeAttributes { get; set; }
        public virtual DbSet<EntityType> EntityTypes { get; set; }
        public virtual DbSet<TemporaryFile> TemporaryFiles { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .ForNpgsqlHasEnum(null, "enum_EntityAttributes_type", new[] { "string", "int", "decimal", "date", "boolean", "geo", "file", "json" })
                .ForNpgsqlHasEnum(null, "enum_EntityTypes_type", new[] { "entity", "relation" })
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity<Entity>(entity =>
            {
                entity.ToTable("Entities");

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

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.Entities)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Entities_typeId_fkey");
            });

            modelBuilder.Entity<Attachment>(entity =>
            {
                entity.ToTable("EntityAttachments");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.File).HasColumnName("file");
            });

            modelBuilder.Entity<AttributeValue>(entity =>
            {
                entity.ToTable("EntityAttributeValues");

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

                entity.HasOne(d => d.Attribute)
                    .WithMany(p => p.EntityAttributeValues)
                    .HasForeignKey(d => d.AttributeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("EntityAttributeValues_attributeId_fkey");

                entity.HasOne(d => d.Entity)
                    .WithMany(p => p.EntityAttributeValues)
                    .HasForeignKey(d => d.EntityId)
                    .HasConstraintName("EntityAttributeValues_entityId_fkey");
            });

            modelBuilder.Entity<EntityAttribute>(entity =>
            {
                entity.ToTable("EntityAttributes");

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

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnName("type");

            });

            modelBuilder.Entity<RelationRestriction>(entity =>
            {
                entity.ToTable("EntityRelationRestrictions");

                entity.HasIndex(e => new { e.RelationTypeId, e.InitiatorTypeId, e.TargetTypeId })
                    .HasName("EntityRelationRestrictions_relationTypeId_initiatorTypeId_t_key")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.InitiatorTypeId).HasColumnName("initiatorTypeId");

                entity.Property(e => e.Meta)
                    .HasColumnName("meta")
                    .HasColumnType("json");

                entity.Property(e => e.RelationTypeId).HasColumnName("relationTypeId");

                entity.Property(e => e.TargetTypeId).HasColumnName("targetTypeId");

                entity.HasOne(d => d.Source)
                    .WithMany(p => p.BackwardRelationRestrictions)
                    .HasForeignKey(d => d.InitiatorTypeId)
                    .HasConstraintName("EntityRelationRestrictions_initiatorTypeId_fkey");

                entity.HasOne(d => d.RelationType)
                    .WithMany(p => p.EntityRelationRestrictionsRelationType)
                    .HasForeignKey(d => d.RelationTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("EntityRelationRestrictions_relationTypeId_fkey");

                entity.HasOne(d => d.Target)
                    .WithMany(p => p.ForwardRelationRestrictions)
                    .HasForeignKey(d => d.TargetTypeId)
                    .HasConstraintName("EntityRelationRestrictions_targetTypeId_fkey");
            });

            modelBuilder.Entity<Relation>(entity =>
            {
                entity.ToTable("EntityRelations");

                entity.HasIndex(e => new { e.TypeId, e.InitiatorId, e.TargetId })
                    .HasName("EntityRelations_typeId_initiatorId_targetId_key")
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

                entity.HasOne(d => d.Initiator)
                    .WithMany(p => p.EntityRelationsInitiator)
                    .HasForeignKey(d => d.InitiatorId)
                    .HasConstraintName("EntityRelations_initiatorId_fkey");

                entity.HasOne(d => d.Target)
                    .WithMany(p => p.EntityRelationsTarget)
                    .HasForeignKey(d => d.TargetId)
                    .HasConstraintName("EntityRelations_targetId_fkey");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.EntityRelations)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("EntityRelations_typeId_fkey");
            });

            modelBuilder.Entity<EntityTypeAttribute>(entity =>
            {
                entity.ToTable("EntityTypeAttributes");

                entity.HasIndex(e => new { e.TypeId, e.AttributeId })
                    .HasName("EntityTypeAttributes_typeId_attributeId_key")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AttributeId).HasColumnName("attributeId");

                entity.Property(e => e.Meta)
                    .HasColumnName("meta")
                    .HasColumnType("jsonb");

                entity.Property(e => e.TypeId).HasColumnName("typeId");

                entity.HasOne(d => d.Attribute)
                    .WithMany(p => p.EntityTypeAttributes)
                    .HasForeignKey(d => d.AttributeId)
                    .HasConstraintName("EntityTypeAttributes_attributeId_fkey");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.EntityTypeAttributes)
                    .HasForeignKey(d => d.TypeId)
                    .HasConstraintName("EntityTypeAttributes_typeId_fkey");
            });

            modelBuilder.Entity<EntityType>(entity =>
            {
                entity.ToTable("EntityTypes");

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

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("EntityTypes_parentId_fkey");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnName("type");
            });

            modelBuilder.Entity<TemporaryFile>(entity =>
            {
                entity.ToTable("TemporaryFiles");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.File).HasColumnName("file");

                entity.Property(e => e.Meta)
                    .HasColumnName("meta")
                    .HasColumnType("json");

                entity.Property(e => e.Title)
                    .HasColumnName("title")
                    .HasMaxLength(255);

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasMaxLength(255);

                entity.Property(e => e.UserId).HasColumnName("userId");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasIndex(e => e.Username)
                    .HasName("Users_username_key")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(255);

                entity.Property(e => e.Password)
                    .HasColumnName("password")
                    .HasMaxLength(255);

                entity.Property(e => e.Username)
                    .HasColumnName("username")
                    .HasMaxLength(255);
            });
        }
    }
}
