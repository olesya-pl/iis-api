using IIS.Core.Files.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace IIS.Core.Ontology.EntityFramework.Context
{
    public class OntologyContext : DbContext
    {
        public OntologyContext() { }

        public OntologyContext(DbContextOptions options) : base(options) { }

        public virtual DbSet<Type> Types { get; set; }
        public virtual DbSet<RelationType> RelationTypes { get; set; }
        public virtual DbSet<AttributeType> AttributeTypes { get; set; }
        public virtual DbSet<Node> Nodes { get; set; }
        public virtual DbSet<Relation> Relations { get; set; }
        public virtual DbSet<Attribute> Attributes { get; set; }
        public virtual DbSet<File> Files { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var relationType = modelBuilder.Entity<RelationType>();
            relationType.HasOne(p => p.Type)
                .WithOne(p => p.RelationType)
                .HasForeignKey<RelationType>(p => p.Id)
                ;

            var attributeType = modelBuilder.Entity<AttributeType>();
            attributeType.HasOne(p => p.Type)
                .WithOne(p => p.AttributeType)
                .HasForeignKey<AttributeType>(p => p.Id)
                ;

            var type = modelBuilder.Entity<Type>();
            type.HasMany(e => e.IncomingRelations)
                .WithOne(e => e.TargetType)
                .HasForeignKey(e => e.TargetTypeId)
                ;
            type.HasMany(e => e.OutgoingRelations)
                .WithOne(e => e.SourceType)
                .HasForeignKey(e => e.SourceTypeId)
                ;
            type.HasMany(e => e.Nodes)
                .WithOne(e => e.Type)
                .HasForeignKey(e => e.TypeId)
                ;

            var relation = modelBuilder.Entity<Relation>();
            relation.HasOne(p => p.Node)
                .WithOne(e => e.Relation)
                .HasForeignKey<Relation>(p => p.Id)
                ;
            var node = modelBuilder.Entity<Node>();
            node.HasMany(e => e.IncomingRelations)
                .WithOne(e => e.TargetNode)
                .HasForeignKey(e => e.TargetNodeId)
                ;
            node.HasMany(e => e.OutgoingRelations)
                .WithOne(e => e.SourceNode)
                .HasForeignKey(e => e.SourceNodeId)
                ;

            var attribute = modelBuilder.Entity<Attribute>();
            attribute.HasOne(p => p.Node)
                .WithOne(p => p.Attribute)
                .HasForeignKey<Attribute>(p => p.Id)
                ;
        }
    }
}
