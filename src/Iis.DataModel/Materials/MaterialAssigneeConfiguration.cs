using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Iis.DataModel.Materials
{
    [DbContext(typeof(OntologyContext))]
    internal class MaterialAssigneeConfiguration : IEntityTypeConfiguration<MaterialAssigneeEntity>
    {
        public void Configure(EntityTypeBuilder<MaterialAssigneeEntity> builder)
        {
            builder.HasKey(_ => new { _.MaterialId, _.AssigneeId });

            builder
                .HasOne(_ => _.Material)
                .WithMany(_ => _.MaterialAssignees)
                .HasForeignKey(_ => _.MaterialId);

            builder
                .HasOne(_ => _.Assignee)
                .WithMany(_ => _.MaterialAssignees)
                .HasForeignKey(_ => _.AssigneeId);
        }
    }
}