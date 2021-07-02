using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Iis.EventMaterialAutoAssignment
{
    public class AssignmentConfigContext : DbContext
    {
        public AssignmentConfigContext()
        {
        }

        public AssignmentConfigContext(DbContextOptions<AssignmentConfigContext> options)
            : base(options)
        {
        }

        public DbSet<AssignmentConfig> AssignmentConfigs { get; set; }
        public DbSet<AssignmentConfigKeyword> AssignmentConfigKeywords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AssignmentConfig>(entity => {
                entity.HasMany(e => e.Keywords)
                    .WithOne(e => e.AssignmentConfig)
                    .HasForeignKey(e => e.AssignmentConfigId);

                entity.Property(e => e.AccessLevel)
                    .HasMaxLength(127);

                entity.Property(e => e.Component)
                    .HasMaxLength(1023);

                entity.Property(e => e.EventType)
                    .HasMaxLength(1023);

                entity.Property(e => e.Importance)
                    .HasMaxLength(127);

                entity.Property(e => e.Name)
                    .HasMaxLength(255);

                entity.Property(e => e.RelatesToCountry)
                    .HasMaxLength(127);

                entity.Property(e => e.State)
                    .HasMaxLength(127);
            });            
        }
    }
}
