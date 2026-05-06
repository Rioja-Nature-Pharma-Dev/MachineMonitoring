using MachineMonitoring.Domain.Entities;
using MachineMonitoring.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MachineMonitoring.Infrastructure.Persistence.Configurations;

public sealed class CalculatedMetricDefinitionConfiguration : IEntityTypeConfiguration<CalculatedMetricDefinition>
{
    public void Configure(EntityTypeBuilder<CalculatedMetricDefinition> builder)
    {
        builder.ToTable("calculated_metric_definitions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion(v => v.Value, v => new ParameterCode(v));

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Unit)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion(v => v.Value, v => new MeasurementUnit(v));

        builder.Property(x => x.FormulaExpression)
            .IsRequired()
            .HasMaxLength(2000);
    }
}
