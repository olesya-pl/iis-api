using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json.Linq;

// todo: Constraint 'EntityTypes_code_type_key' on table public.EntityTypes cannot be scaffolded because it includes a column that cannot be scaffolded(e.g. enum).
// todo: Index 'EntityTypes_code_type_key' on table public.EntityTypes cannot be scaffolded because it includes a column that cannot be scaffolded(e.g. enum).

namespace IIS.Legacy.EntityFramework
{
    public partial class ContourContext : DbContext
    {
        public ContourContext()
        {
        }

        public ContourContext(DbContextOptions options) : base(options) { }

        public virtual DbSet<OEntity> Entities { get; set; }
        public virtual DbSet<OAttributeValue> AttributeValues { get; set; }
        public virtual DbSet<OAttribute> Attributes { get; set; }
        public virtual DbSet<ORestriction> Restrictions { get; set; }
        public virtual DbSet<ORelation> Relations { get; set; }
        public virtual DbSet<OAttributeRestriction> AttributeRestrictions { get; set; }
        public virtual DbSet<OType> Types { get; set; }
        public virtual DbSet<OTypeEntity> EntityTypes { get; set; }
        public virtual DbSet<OTypeRelation> RelationTypes { get; set; }
        public virtual DbSet<Attachment> Attachments { get; set; }
        public virtual DbSet<TemporaryFile> TemporaryFiles { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .ForNpgsqlHasEnum(null, "enum_EntityAttributes_type", new[] { "string", "int", "decimal", "date", "boolean", "geo", "file", "json" })
                .ForNpgsqlHasEnum(null, "enum_EntityTypes_type", new[] { "entity", "relation" })
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity<OEntity>(entity =>
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

            modelBuilder.Entity<OAttributeValue>(entity =>
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
                    .WithMany(p => p.Values)
                    .HasForeignKey(d => d.AttributeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("EntityAttributeValues_attributeId_fkey");

                entity.HasOne(d => d.Entity)
                    .WithMany(p => p.AttributeValues)
                    .HasForeignKey(d => d.EntityId)
                    .HasConstraintName("EntityAttributeValues_entityId_fkey");
            });

            modelBuilder.Entity<OAttribute>(entity =>
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

            modelBuilder.Entity<ORestriction>(entity =>
            {
                entity.ToTable("EntityRelationRestrictions");

                entity.HasIndex(e => new { e.RelationTypeId, e.SourceId, e.TargetId })
                    .HasName("EntityRelationRestrictions_relationTypeId_initiatorTypeId_t_key")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.SourceId).HasColumnName("initiatorTypeId");

                entity.Property(e => e.Meta)
                    .HasColumnName("meta")
                    .HasColumnType("json");

                entity.Property(e => e.RelationTypeId).HasColumnName("relationTypeId");

                entity.Property(e => e.TargetId).HasColumnName("targetTypeId");

                entity.HasOne(d => d.Source)
                    .WithMany(p => p.ForwardRestrictions)
                    .HasForeignKey(d => d.SourceId)
                    .HasConstraintName("EntityRelationRestrictions_initiatorTypeId_fkey");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.Restrictions)
                    .HasForeignKey(d => d.RelationTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("EntityRelationRestrictions_relationTypeId_fkey");

                entity.HasOne(d => d.Target)
                    .WithMany(p => p.BackwardRestrictions)
                    .HasForeignKey(d => d.TargetId)
                    .HasConstraintName("EntityRelationRestrictions_targetTypeId_fkey");
            });

            modelBuilder.Entity<ORelation>(entity =>
            {
                entity.ToTable("EntityRelations");

                entity.HasIndex(e => new { e.TypeId, e.SourceId, e.TargetId })
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

                entity.Property(e => e.SourceId).HasColumnName("initiatorId");

                entity.Property(e => e.IsInferred).HasColumnName("isInferred");

                entity.Property(e => e.StartsAt)
                    .HasColumnName("startsAt")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.TargetId).HasColumnName("targetId");

                entity.Property(e => e.TypeId).HasColumnName("typeId");

                entity.HasOne(d => d.Source)
                    .WithMany(p => p.ForwardRelations)
                    .HasForeignKey(d => d.SourceId)
                    .HasConstraintName("EntityRelations_initiatorId_fkey");

                entity.HasOne(d => d.Target)
                    .WithMany(p => p.BackwardRelations)
                    .HasForeignKey(d => d.TargetId)
                    .HasConstraintName("EntityRelations_targetId_fkey");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.Relations)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("EntityRelations_typeId_fkey");
            });

            modelBuilder.Entity<OAttributeRestriction>(entity =>
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
                    .WithMany(p => p.Restrictions)
                    .HasForeignKey(d => d.AttributeId)
                    .HasConstraintName("EntityTypeAttributes_attributeId_fkey");

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.AttributeRestrictions)
                    .HasForeignKey(d => d.TypeId)
                    .HasConstraintName("EntityTypeAttributes_typeId_fkey");
            });

            modelBuilder.Entity<OType>(entity =>
            {
                entity.ToTable("EntityTypes");

                entity.HasDiscriminator(e => e.Kind)
                    .HasValue<OTypeEntity>(EntityKind.Entity)
                    .HasValue<OTypeRelation>(EntityKind.Relation);

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
                    .WithMany(p => p.DerivedTypes)
                    .HasForeignKey(d => d.ParentId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("EntityTypes_parentId_fkey");

                entity.Property(e => e.Kind)
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

            // JObject
            modelBuilder.RegisterObjectEnum(v => v.ToString(), v => JObject.Parse(v));
            // ScalarType
            modelBuilder.RegisterObjectEnum(v => (string)v, v => (ScalarType)v);
            // EntityType
            modelBuilder.RegisterObjectEnum(v => (string)v, v => (EntityKind)v);
        }
    }

    public static class ModelBuilderExtensions
    {
        public static void RegisterObjectEnum<T>(this ModelBuilder modelBuilder, Expression<Func<T, string>> to, Expression<Func<string, T>> from)
        {
            var valueConverter = new ValueConverter<T, string>(to, from);
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                foreach (var property in entityType.GetProperties())
                    if (property.ClrType == typeof(T)) property.SetValueConverter(valueConverter);
        }
    }
}
