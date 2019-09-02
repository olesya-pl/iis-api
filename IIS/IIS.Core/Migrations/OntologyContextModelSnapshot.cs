﻿// <auto-generated />
using System;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IIS.Core.Migrations
{
    [DbContext(typeof(OntologyContext))]
    partial class OntologyContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("IIS.Core.Files.EntityFramework.File", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ContentType");

                    b.Property<byte[]>("Contents");

                    b.Property<bool>("IsTemporary");

                    b.Property<string>("Name");

                    b.Property<DateTime>("UploadTime");

                    b.HasKey("Id");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("IIS.Core.Materials.EntityFramework.Material", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Data");

                    b.Property<Guid?>("FileId");

                    b.Property<Guid?>("ParentId");

                    b.Property<string>("Source");

                    b.Property<string>("Type");

                    b.HasKey("Id");

                    b.HasIndex("FileId")
                        .IsUnique();

                    b.HasIndex("ParentId");

                    b.ToTable("Materials");
                });

            modelBuilder.Entity("IIS.Core.Materials.EntityFramework.MaterialFeature", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("MaterialInfoId");

                    b.Property<Guid>("NodeId");

                    b.Property<string>("Relation");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.HasIndex("MaterialInfoId");

                    b.HasIndex("NodeId")
                        .IsUnique();

                    b.ToTable("MaterialFeatures");
                });

            modelBuilder.Entity("IIS.Core.Materials.EntityFramework.MaterialInfo", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Data");

                    b.Property<Guid>("MaterialId");

                    b.Property<string>("Source");

                    b.Property<string>("SourceType");

                    b.Property<string>("SourceVersion");

                    b.HasKey("Id");

                    b.HasIndex("MaterialId");

                    b.ToTable("MaterialInfos");
                });

            modelBuilder.Entity("IIS.Core.Ontology.EntityFramework.Context.Attribute", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.ToTable("Attributes");
                });

            modelBuilder.Entity("IIS.Core.Ontology.EntityFramework.Context.AttributeType", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<int>("ScalarType");

                    b.HasKey("Id");

                    b.ToTable("AttributeTypes");
                });

            modelBuilder.Entity("IIS.Core.Ontology.EntityFramework.Context.Node", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedAt");

                    b.Property<bool>("IsArchived");

                    b.Property<Guid>("TypeId");

                    b.Property<DateTime>("UpdatedAt");

                    b.HasKey("Id");

                    b.HasIndex("TypeId");

                    b.ToTable("Nodes");
                });

            modelBuilder.Entity("IIS.Core.Ontology.EntityFramework.Context.Relation", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<Guid>("SourceNodeId");

                    b.Property<Guid>("TargetNodeId");

                    b.HasKey("Id");

                    b.HasIndex("SourceNodeId");

                    b.HasIndex("TargetNodeId");

                    b.ToTable("Relations");
                });

            modelBuilder.Entity("IIS.Core.Ontology.EntityFramework.Context.RelationType", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<int>("EmbeddingOptions");

                    b.Property<int>("Kind");

                    b.Property<Guid>("SourceTypeId");

                    b.Property<Guid>("TargetTypeId");

                    b.HasKey("Id");

                    b.HasIndex("SourceTypeId");

                    b.HasIndex("TargetTypeId");

                    b.ToTable("RelationTypes");
                });

            modelBuilder.Entity("IIS.Core.Ontology.EntityFramework.Context.Type", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedAt");

                    b.Property<bool>("IsAbstract");

                    b.Property<bool>("IsArchived");

                    b.Property<int>("Kind");

                    b.Property<string>("Meta");

                    b.Property<string>("Name");

                    b.Property<string>("Title");

                    b.Property<DateTime>("UpdatedAt");

                    b.HasKey("Id");

                    b.ToTable("Types");
                });

            modelBuilder.Entity("IIS.Core.Materials.EntityFramework.Material", b =>
                {
                    b.HasOne("IIS.Core.Files.EntityFramework.File", "File")
                        .WithOne()
                        .HasForeignKey("IIS.Core.Materials.EntityFramework.Material", "FileId");

                    b.HasOne("IIS.Core.Materials.EntityFramework.Material", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentId");
                });

            modelBuilder.Entity("IIS.Core.Materials.EntityFramework.MaterialFeature", b =>
                {
                    b.HasOne("IIS.Core.Materials.EntityFramework.MaterialInfo", "Info")
                        .WithMany("Features")
                        .HasForeignKey("MaterialInfoId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("IIS.Core.Ontology.EntityFramework.Context.Node", "Node")
                        .WithOne()
                        .HasForeignKey("IIS.Core.Materials.EntityFramework.MaterialFeature", "NodeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("IIS.Core.Materials.EntityFramework.MaterialInfo", b =>
                {
                    b.HasOne("IIS.Core.Materials.EntityFramework.Material", "Material")
                        .WithMany("Infos")
                        .HasForeignKey("MaterialId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("IIS.Core.Ontology.EntityFramework.Context.Attribute", b =>
                {
                    b.HasOne("IIS.Core.Ontology.EntityFramework.Context.Node", "Node")
                        .WithOne("Attribute")
                        .HasForeignKey("IIS.Core.Ontology.EntityFramework.Context.Attribute", "Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("IIS.Core.Ontology.EntityFramework.Context.AttributeType", b =>
                {
                    b.HasOne("IIS.Core.Ontology.EntityFramework.Context.Type", "Type")
                        .WithOne("AttributeType")
                        .HasForeignKey("IIS.Core.Ontology.EntityFramework.Context.AttributeType", "Id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("IIS.Core.Ontology.EntityFramework.Context.Node", b =>
                {
                    b.HasOne("IIS.Core.Ontology.EntityFramework.Context.Type", "Type")
                        .WithMany("Nodes")
                        .HasForeignKey("TypeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("IIS.Core.Ontology.EntityFramework.Context.Relation", b =>
                {
                    b.HasOne("IIS.Core.Ontology.EntityFramework.Context.Node", "Node")
                        .WithOne("Relation")
                        .HasForeignKey("IIS.Core.Ontology.EntityFramework.Context.Relation", "Id")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("IIS.Core.Ontology.EntityFramework.Context.Node", "SourceNode")
                        .WithMany("OutgoingRelations")
                        .HasForeignKey("SourceNodeId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("IIS.Core.Ontology.EntityFramework.Context.Node", "TargetNode")
                        .WithMany("IncomingRelations")
                        .HasForeignKey("TargetNodeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("IIS.Core.Ontology.EntityFramework.Context.RelationType", b =>
                {
                    b.HasOne("IIS.Core.Ontology.EntityFramework.Context.Type", "Type")
                        .WithOne("RelationType")
                        .HasForeignKey("IIS.Core.Ontology.EntityFramework.Context.RelationType", "Id")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("IIS.Core.Ontology.EntityFramework.Context.Type", "SourceType")
                        .WithMany("OutgoingRelations")
                        .HasForeignKey("SourceTypeId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("IIS.Core.Ontology.EntityFramework.Context.Type", "TargetType")
                        .WithMany("IncomingRelations")
                        .HasForeignKey("TargetTypeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
