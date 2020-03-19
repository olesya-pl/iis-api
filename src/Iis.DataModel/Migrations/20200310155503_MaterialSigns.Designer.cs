﻿// <auto-generated />
using System;
using Iis.DataModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IIS.Core.Migrations
{
    [DbContext(typeof(OntologyContext))]
    [Migration("20200310155503_MaterialSigns")]
    partial class MaterialSigns
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Iis.DataModel.Analytics.AnalyticIndicatorEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ParentId")
                        .HasColumnType("uuid");

                    b.Property<string>("Query")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("character varying(200)")
                        .HasMaxLength(200);

                    b.HasKey("Id");

                    b.HasIndex("ParentId");

                    b.ToTable("AnalyticIndicators");
                });

            modelBuilder.Entity("Iis.DataModel.Analytics.AnalyticQueryEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid>("CreatorId")
                        .HasColumnType("uuid");

                    b.Property<string>("DateRanges")
                        .HasColumnType("jsonb");

                    b.Property<string>("Description")
                        .HasColumnType("character varying(1000)")
                        .HasMaxLength(1000);

                    b.Property<Guid>("LastUpdaterId")
                        .HasColumnType("uuid");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("character varying(500)")
                        .HasMaxLength(500);

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.HasIndex("LastUpdaterId");

                    b.ToTable("AnalyticQueries");
                });

            modelBuilder.Entity("Iis.DataModel.Analytics.AnalyticQueryIndicatorEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("IndicatorId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("QueryId")
                        .HasColumnType("uuid");

                    b.Property<int>("SortOrder")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("IndicatorId");

                    b.HasIndex("QueryId");

                    b.ToTable("AnalyticQueryIndicators");
                });

            modelBuilder.Entity("Iis.DataModel.AttributeEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Attributes");
                });

            modelBuilder.Entity("Iis.DataModel.AttributeTypeEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<int>("ScalarType")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("AttributeTypes");
                });

            modelBuilder.Entity("Iis.DataModel.Materials.FileEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("ContentHash")
                        .HasColumnType("uuid");

                    b.Property<string>("ContentType")
                        .HasColumnType("text");

                    b.Property<byte[]>("Contents")
                        .HasColumnType("bytea");

                    b.Property<bool>("IsTemporary")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<DateTime>("UploadTime")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("ContentHash");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("Iis.DataModel.Materials.MaterialEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("CompletenessSignId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Data")
                        .HasColumnType("text");

                    b.Property<Guid?>("FileId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ImportanceSignId")
                        .HasColumnType("uuid");

                    b.Property<string>("Metadata")
                        .HasColumnType("text");

                    b.Property<Guid?>("ParentId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("RelevanceSignId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ReliabilitySignId")
                        .HasColumnType("uuid");

                    b.Property<string>("Source")
                        .HasColumnType("text");

                    b.Property<Guid?>("SourceReliabilitySignId")
                        .HasColumnType("uuid");

                    b.Property<string>("Type")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CompletenessSignId");

                    b.HasIndex("FileId");

                    b.HasIndex("ImportanceSignId");

                    b.HasIndex("ParentId");

                    b.HasIndex("RelevanceSignId");

                    b.HasIndex("ReliabilitySignId");

                    b.HasIndex("SourceReliabilitySignId");

                    b.ToTable("Materials");
                });

            modelBuilder.Entity("Iis.DataModel.Materials.MaterialFeatureEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("MaterialInfoId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("NodeId")
                        .HasColumnType("uuid");

                    b.Property<string>("Relation")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("MaterialInfoId");

                    b.HasIndex("NodeId");

                    b.ToTable("MaterialFeatures");
                });

            modelBuilder.Entity("Iis.DataModel.Materials.MaterialInfoEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Data")
                        .HasColumnType("text");

                    b.Property<Guid>("MaterialId")
                        .HasColumnType("uuid");

                    b.Property<string>("Source")
                        .HasColumnType("text");

                    b.Property<string>("SourceType")
                        .HasColumnType("text");

                    b.Property<string>("SourceVersion")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("MaterialId");

                    b.ToTable("MaterialInfos");
                });

            modelBuilder.Entity("Iis.DataModel.Materials.MaterialSignEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<Guid>("MaterialSignTypeId")
                        .HasColumnType("uuid");

                    b.Property<int>("OrderNumber")
                        .HasColumnType("integer");

                    b.Property<string>("ShortTitle")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("MaterialSignTypeId");

                    b.ToTable("MaterialSigns");

                    b.HasData(
                        new
                        {
                            Id = new Guid("1107a504-c2a7-4f8b-a218-e5bbf5f281c4"),
                            MaterialSignTypeId = new Guid("10170a81-2916-420b-8bd7-5688cb43b82f"),
                            OrderNumber = 1,
                            ShortTitle = "1",
                            Title = "Перша категорія"
                        },
                        new
                        {
                            Id = new Guid("1240c504-8ecd-4aca-9b75-24f0c6304426"),
                            MaterialSignTypeId = new Guid("10170a81-2916-420b-8bd7-5688cb43b82f"),
                            OrderNumber = 2,
                            ShortTitle = "2",
                            Title = "Друга категорія"
                        },
                        new
                        {
                            Id = new Guid("1356a6b3-c63f-4985-8b74-372236fe744f"),
                            MaterialSignTypeId = new Guid("10170a81-2916-420b-8bd7-5688cb43b82f"),
                            OrderNumber = 3,
                            ShortTitle = "3",
                            Title = "Третя категорія"
                        },
                        new
                        {
                            Id = new Guid("211f5765-0867-4d04-976a-70f3e34bf153"),
                            MaterialSignTypeId = new Guid("202f605f-4fb2-49a7-beb8-e40cd41f2b83"),
                            OrderNumber = 1,
                            ShortTitle = "Д",
                            Title = "Достовірна"
                        },
                        new
                        {
                            Id = new Guid("225f189b-9ad2-4687-9624-0d4c991a3d6b"),
                            MaterialSignTypeId = new Guid("202f605f-4fb2-49a7-beb8-e40cd41f2b83"),
                            OrderNumber = 2,
                            ShortTitle = "Й",
                            Title = "Ймовірна"
                        },
                        new
                        {
                            Id = new Guid("2326d6ef-5542-42a8-83eb-0c2b92d188f1"),
                            MaterialSignTypeId = new Guid("202f605f-4fb2-49a7-beb8-e40cd41f2b83"),
                            OrderNumber = 3,
                            ShortTitle = "С",
                            Title = "Сумнівна"
                        },
                        new
                        {
                            Id = new Guid("2475d991-b09e-4997-9d0a-2fc0bf07b1eb"),
                            MaterialSignTypeId = new Guid("202f605f-4fb2-49a7-beb8-e40cd41f2b83"),
                            OrderNumber = 4,
                            ShortTitle = "Н",
                            Title = "Недостовірна"
                        },
                        new
                        {
                            Id = new Guid("25007914-d5cb-4def-8162-c12b4aa7038c"),
                            MaterialSignTypeId = new Guid("202f605f-4fb2-49a7-beb8-e40cd41f2b83"),
                            OrderNumber = 5,
                            ShortTitle = "НД",
                            Title = "Невизначеної достовірності"
                        },
                        new
                        {
                            Id = new Guid("2616aa7d-c379-452b-8c1a-c815f9b989bc"),
                            MaterialSignTypeId = new Guid("202f605f-4fb2-49a7-beb8-e40cd41f2b83"),
                            OrderNumber = 6,
                            ShortTitle = "ДІ",
                            Title = "Дезінформація"
                        },
                        new
                        {
                            Id = new Guid("313d1f5b-7b3a-446f-ab92-e4046930a599"),
                            MaterialSignTypeId = new Guid("30106ade-e768-438a-b736-5c19df3ffd52"),
                            OrderNumber = 1,
                            ShortTitle = "У",
                            Title = "Упереджувальна"
                        },
                        new
                        {
                            Id = new Guid("320c2a19-ed1b-4250-bb02-eb4f7391165b"),
                            MaterialSignTypeId = new Guid("30106ade-e768-438a-b736-5c19df3ffd52"),
                            OrderNumber = 2,
                            ShortTitle = "ДА",
                            Title = "Дуже актуальна"
                        },
                        new
                        {
                            Id = new Guid("3317a961-1929-4957-9ef0-08b3007648a6"),
                            MaterialSignTypeId = new Guid("30106ade-e768-438a-b736-5c19df3ffd52"),
                            OrderNumber = 3,
                            ShortTitle = "А",
                            Title = "Актуальна"
                        },
                        new
                        {
                            Id = new Guid("341892c9-3918-4a7f-bf61-d5b9050de7f4"),
                            MaterialSignTypeId = new Guid("30106ade-e768-438a-b736-5c19df3ffd52"),
                            OrderNumber = 4,
                            ShortTitle = "П",
                            Title = "Посередня"
                        },
                        new
                        {
                            Id = new Guid("354436fe-8c25-4352-8b89-3e94bf5828e2"),
                            MaterialSignTypeId = new Guid("30106ade-e768-438a-b736-5c19df3ffd52"),
                            OrderNumber = 5,
                            ShortTitle = "НI",
                            Title = "Неактуальна інформація"
                        },
                        new
                        {
                            Id = new Guid("4124de78-0877-40b4-834a-f892060ea3f5"),
                            MaterialSignTypeId = new Guid("4061d06f-c14a-454e-9247-ccdd6d9388f0"),
                            OrderNumber = 1,
                            ShortTitle = "П",
                            Title = "Повна"
                        },
                        new
                        {
                            Id = new Guid("422914a7-f761-4075-a91e-4d34d33aedff"),
                            MaterialSignTypeId = new Guid("4061d06f-c14a-454e-9247-ccdd6d9388f0"),
                            OrderNumber = 2,
                            ShortTitle = "ДП",
                            Title = "Достатньо повна"
                        },
                        new
                        {
                            Id = new Guid("431a888f-406b-458a-9905-abc752710659"),
                            MaterialSignTypeId = new Guid("4061d06f-c14a-454e-9247-ccdd6d9388f0"),
                            OrderNumber = 3,
                            ShortTitle = "Ч",
                            Title = "Часткова"
                        },
                        new
                        {
                            Id = new Guid("44ddf35a-eeee-4aa3-9f3c-9b73dc1d63ee"),
                            MaterialSignTypeId = new Guid("4061d06f-c14a-454e-9247-ccdd6d9388f0"),
                            OrderNumber = 4,
                            ShortTitle = "НРІ",
                            Title = "Недостатня розвідувальна інформація"
                        },
                        new
                        {
                            Id = new Guid("513de8b4-5c99-414f-94f1-513a716fc01c"),
                            MaterialSignTypeId = new Guid("5023bb79-f987-48fe-a86c-38b7aa8495c4"),
                            OrderNumber = 1,
                            ShortTitle = "A",
                            Title = "Повністю надійне"
                        },
                        new
                        {
                            Id = new Guid("521ad86b-af5d-4731-b5e7-e3e69ef23fc7"),
                            MaterialSignTypeId = new Guid("5023bb79-f987-48fe-a86c-38b7aa8495c4"),
                            OrderNumber = 2,
                            ShortTitle = "B",
                            Title = "Здебільшего надійне"
                        },
                        new
                        {
                            Id = new Guid("5342ead6-d478-4abc-b8d1-fd5d6a741706"),
                            MaterialSignTypeId = new Guid("5023bb79-f987-48fe-a86c-38b7aa8495c4"),
                            OrderNumber = 3,
                            ShortTitle = "C",
                            Title = "Відносно надійне"
                        },
                        new
                        {
                            Id = new Guid("5406768c-581d-4b95-a549-b2cd1d09cfd8"),
                            MaterialSignTypeId = new Guid("5023bb79-f987-48fe-a86c-38b7aa8495c4"),
                            OrderNumber = 4,
                            ShortTitle = "D",
                            Title = "Не завжди надійне"
                        },
                        new
                        {
                            Id = new Guid("55b0a038-2347-4fb0-82e1-6081933ac9e1"),
                            MaterialSignTypeId = new Guid("5023bb79-f987-48fe-a86c-38b7aa8495c4"),
                            OrderNumber = 5,
                            ShortTitle = "E",
                            Title = "Ненадійне"
                        },
                        new
                        {
                            Id = new Guid("56365559-24fb-42f2-8305-bbef01fd6e3e"),
                            MaterialSignTypeId = new Guid("5023bb79-f987-48fe-a86c-38b7aa8495c4"),
                            OrderNumber = 6,
                            ShortTitle = "F",
                            Title = "Неможливо оцінити надійність"
                        });
                });

            modelBuilder.Entity("Iis.DataModel.Materials.MaterialSignTypeEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("MaterialSignTypes");

                    b.HasData(
                        new
                        {
                            Id = new Guid("10170a81-2916-420b-8bd7-5688cb43b82f"),
                            Name = "Importance",
                            Title = "Важливість"
                        },
                        new
                        {
                            Id = new Guid("202f605f-4fb2-49a7-beb8-e40cd41f2b83"),
                            Name = "Reliability",
                            Title = "Достовірність"
                        },
                        new
                        {
                            Id = new Guid("30106ade-e768-438a-b736-5c19df3ffd52"),
                            Name = "Relevance",
                            Title = "Актуальність"
                        },
                        new
                        {
                            Id = new Guid("4061d06f-c14a-454e-9247-ccdd6d9388f0"),
                            Name = "Completeness",
                            Title = "Повнота"
                        },
                        new
                        {
                            Id = new Guid("5023bb79-f987-48fe-a86c-38b7aa8495c4"),
                            Name = "SourceReliability",
                            Title = "Надійність джерела"
                        });
                });

            modelBuilder.Entity("Iis.DataModel.NodeEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("IsArchived")
                        .HasColumnType("boolean");

                    b.Property<Guid>("NodeTypeId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("NodeTypeId");

                    b.ToTable("Nodes");
                });

            modelBuilder.Entity("Iis.DataModel.NodeTypeEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("IsAbstract")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsArchived")
                        .HasColumnType("boolean");

                    b.Property<int>("Kind")
                        .HasColumnType("integer");

                    b.Property<string>("Meta")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.ToTable("NodeTypes");
                });

            modelBuilder.Entity("Iis.DataModel.OntologyMigrationsEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("EndTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("IsSuccess")
                        .HasColumnType("boolean");

                    b.Property<string>("Log")
                        .HasColumnType("text");

                    b.Property<string>("MigrationRules")
                        .HasColumnType("text");

                    b.Property<int>("OrderNumber")
                        .HasColumnType("integer");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("StructureAfter")
                        .HasColumnType("text");

                    b.Property<string>("StructureBefore")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("OntologyMigrations");
                });

            modelBuilder.Entity("Iis.DataModel.RelationEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<Guid>("SourceNodeId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("TargetNodeId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("SourceNodeId");

                    b.HasIndex("TargetNodeId");

                    b.ToTable("Relations");
                });

            modelBuilder.Entity("Iis.DataModel.RelationTypeEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<int>("EmbeddingOptions")
                        .HasColumnType("integer");

                    b.Property<int>("Kind")
                        .HasColumnType("integer");

                    b.Property<Guid>("SourceTypeId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("TargetTypeId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("SourceTypeId");

                    b.HasIndex("TargetTypeId");

                    b.ToTable("RelationTypes");
                });

            modelBuilder.Entity("Iis.DataModel.Reports.ReportEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Recipient")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Reports");
                });

            modelBuilder.Entity("Iis.DataModel.Reports.ReportEventEntity", b =>
                {
                    b.Property<Guid>("ReportId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("EventId")
                        .HasColumnType("uuid");

                    b.HasKey("ReportId", "EventId");

                    b.HasIndex("EventId");

                    b.ToTable("ReportEvents");
                });

            modelBuilder.Entity("Iis.DataModel.UserEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<bool>("IsBlocked")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasAlternateKey("Username");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Iis.DataModel.Analytics.AnalyticIndicatorEntity", b =>
                {
                    b.HasOne("Iis.DataModel.Analytics.AnalyticIndicatorEntity", "Parent")
                        .WithMany()
                        .HasForeignKey("ParentId");
                });

            modelBuilder.Entity("Iis.DataModel.Analytics.AnalyticQueryEntity", b =>
                {
                    b.HasOne("Iis.DataModel.UserEntity", "Creator")
                        .WithMany()
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Iis.DataModel.UserEntity", "LastUpdater")
                        .WithMany()
                        .HasForeignKey("LastUpdaterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Iis.DataModel.Analytics.AnalyticQueryIndicatorEntity", b =>
                {
                    b.HasOne("Iis.DataModel.Analytics.AnalyticIndicatorEntity", "Indicator")
                        .WithMany("QueryIndicators")
                        .HasForeignKey("IndicatorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Iis.DataModel.Analytics.AnalyticQueryEntity", "Query")
                        .WithMany("Indicators")
                        .HasForeignKey("QueryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Iis.DataModel.AttributeEntity", b =>
                {
                    b.HasOne("Iis.DataModel.NodeEntity", "Node")
                        .WithOne("Attribute")
                        .HasForeignKey("Iis.DataModel.AttributeEntity", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Iis.DataModel.AttributeTypeEntity", b =>
                {
                    b.HasOne("Iis.DataModel.NodeTypeEntity", "NodeType")
                        .WithOne("AttributeType")
                        .HasForeignKey("Iis.DataModel.AttributeTypeEntity", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Iis.DataModel.Materials.MaterialEntity", b =>
                {
                    b.HasOne("Iis.DataModel.Materials.MaterialSignEntity", "Completeness")
                        .WithMany()
                        .HasForeignKey("CompletenessSignId");

                    b.HasOne("Iis.DataModel.Materials.FileEntity", "File")
                        .WithMany("Materials")
                        .HasForeignKey("FileId");

                    b.HasOne("Iis.DataModel.Materials.MaterialSignEntity", "Importance")
                        .WithMany()
                        .HasForeignKey("ImportanceSignId");

                    b.HasOne("Iis.DataModel.Materials.MaterialEntity", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentId");

                    b.HasOne("Iis.DataModel.Materials.MaterialSignEntity", "Relevance")
                        .WithMany()
                        .HasForeignKey("RelevanceSignId");

                    b.HasOne("Iis.DataModel.Materials.MaterialSignEntity", "Reliability")
                        .WithMany()
                        .HasForeignKey("ReliabilitySignId");

                    b.HasOne("Iis.DataModel.Materials.MaterialSignEntity", "SourceReliability")
                        .WithMany()
                        .HasForeignKey("SourceReliabilitySignId");
                });

            modelBuilder.Entity("Iis.DataModel.Materials.MaterialFeatureEntity", b =>
                {
                    b.HasOne("Iis.DataModel.Materials.MaterialInfoEntity", "MaterialInfo")
                        .WithMany("MaterialFeatures")
                        .HasForeignKey("MaterialInfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Iis.DataModel.NodeEntity", "Node")
                        .WithMany("MaterialFeatures")
                        .HasForeignKey("NodeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Iis.DataModel.Materials.MaterialInfoEntity", b =>
                {
                    b.HasOne("Iis.DataModel.Materials.MaterialEntity", "Material")
                        .WithMany("MaterialInfos")
                        .HasForeignKey("MaterialId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Iis.DataModel.Materials.MaterialSignEntity", b =>
                {
                    b.HasOne("Iis.DataModel.Materials.MaterialSignTypeEntity", "MaterialSignType")
                        .WithMany("MaterialSigns")
                        .HasForeignKey("MaterialSignTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Iis.DataModel.NodeEntity", b =>
                {
                    b.HasOne("Iis.DataModel.NodeTypeEntity", "NodeType")
                        .WithMany("Nodes")
                        .HasForeignKey("NodeTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Iis.DataModel.RelationEntity", b =>
                {
                    b.HasOne("Iis.DataModel.NodeEntity", "Node")
                        .WithOne("Relation")
                        .HasForeignKey("Iis.DataModel.RelationEntity", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Iis.DataModel.NodeEntity", "SourceNode")
                        .WithMany("OutgoingRelations")
                        .HasForeignKey("SourceNodeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Iis.DataModel.NodeEntity", "TargetNode")
                        .WithMany("IncomingRelations")
                        .HasForeignKey("TargetNodeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Iis.DataModel.RelationTypeEntity", b =>
                {
                    b.HasOne("Iis.DataModel.NodeTypeEntity", "NodeType")
                        .WithOne("RelationType")
                        .HasForeignKey("Iis.DataModel.RelationTypeEntity", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Iis.DataModel.NodeTypeEntity", "SourceType")
                        .WithMany("OutgoingRelations")
                        .HasForeignKey("SourceTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Iis.DataModel.NodeTypeEntity", "TargetType")
                        .WithMany("IncomingRelations")
                        .HasForeignKey("TargetTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Iis.DataModel.Reports.ReportEventEntity", b =>
                {
                    b.HasOne("Iis.DataModel.NodeEntity", "Node")
                        .WithMany("ReportEvents")
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Iis.DataModel.Reports.ReportEntity", "Report")
                        .WithMany("ReportEvents")
                        .HasForeignKey("ReportId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
