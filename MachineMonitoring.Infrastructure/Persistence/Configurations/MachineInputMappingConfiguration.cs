using MachineMonitoring.Domain.Entities;
using MachineMonitoring.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MachineMonitoring.Infrastructure.Persistence.Configurations;

public sealed class MachineInputMappingConfiguration : IEntityTypeConfiguration<MachineInputMapping>
{
    public void Configure(EntityTypeBuilder<MachineInputMapping> builder)
    {
        builder.ToTable("machine_input_mappings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ExternalFieldPath)
            .IsRequired()
            .HasMaxLength(500)
            .HasConversion(v => v.Value, v => new ExternalFieldPath(v));

        builder.Property(x => x.ParameterCode)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion(v => v.Value, v => new ParameterCode(v));

        builder.Property(x => x.TransformExpression)
            .HasMaxLength(2000);
    }
}
