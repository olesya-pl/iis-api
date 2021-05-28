﻿// <auto-generated />
using System;
using Iis.EventMaterialAutoAssignment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Iis.EventMaterialAutoAssignment.DataModel.Migrations
{
    [DbContext(typeof(AssignmentConfigContext))]
    [Migration("20210513152726_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Iis.EventMaterialAutoAssignment.AssignmentConfig", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AccessLevel")
                        .HasColumnType("character varying(127)")
                        .HasMaxLength(127);

                    b.Property<string>("Component")
                        .HasColumnType("character varying(1023)")
                        .HasMaxLength(1023);

                    b.Property<string>("EventType")
                        .HasColumnType("character varying(1023)")
                        .HasMaxLength(1023);

                    b.Property<string>("Importance")
                        .HasColumnType("character varying(127)")
                        .HasMaxLength(127);

                    b.Property<string>("Name")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.Property<string>("RelatesToCountry")
                        .HasColumnType("character varying(127)")
                        .HasMaxLength(127);

                    b.Property<string>("State")
                        .HasColumnType("character varying(127)")
                        .HasMaxLength(127);

                    b.HasKey("Id");

                    b.ToTable("AssignmentConfigs");
                });

            modelBuilder.Entity("Iis.EventMaterialAutoAssignment.AssignmentConfigKeyword", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AssignmentConfigId")
                        .HasColumnType("uuid");

                    b.Property<string>("Keyword")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("AssignmentConfigId");

                    b.ToTable("AssignmentConfigKeywords");
                });

            modelBuilder.Entity("Iis.EventMaterialAutoAssignment.AssignmentConfigKeyword", b =>
                {
                    b.HasOne("Iis.EventMaterialAutoAssignment.AssignmentConfig", "AssignmentConfig")
                        .WithMany("Keywords")
                        .HasForeignKey("AssignmentConfigId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
