using MachineMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MachineMonitoring.Infrastructure.Persistence.Configurations;

public sealed class ProductionPauseConfiguration : IEntityTypeConfiguration<ProductionPause>
{
    public void Configure(EntityTypeBuilder<ProductionPause> builder)
    {
        builder.ToTable("production_pauses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.OperatorName)
            .HasMaxLength(100);

        builder.Property(x => x.TotalMinutes)
            .HasPrecision(18, 2);

        builder.HasIndex(x => x.ProductionOrderId);
    }
}