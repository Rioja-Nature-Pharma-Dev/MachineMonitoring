using MachineMonitoring.Domain.Entities;
using MachineMonitoring.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MachineMonitoring.Infrastructure.Persistence.Configurations;

public sealed class MachineReadingNormalizedConfiguration : IEntityTypeConfiguration<MachineReadingNormalized>
{
    public void Configure(EntityTypeBuilder<MachineReadingNormalized> builder)
    {
        builder.ToTable("machine_readings_normalized");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ParameterCode)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion(v => v.Value, v => new ParameterCode(v));

        builder.OwnsOne(x => x.Value, owned =>
        {
            owned.Property(v => v.NumericValue).HasColumnName("ValueNumeric").HasPrecision(18, 6);
            owned.Property(v => v.TextValue).HasColumnName("ValueText").HasMaxLength(500);
            owned.Property(v => v.BooleanValue).HasColumnName("ValueBoolean");
        });

        builder.Property(x => x.SourceMessageId)
            .HasMaxLength(200);
    }
}
