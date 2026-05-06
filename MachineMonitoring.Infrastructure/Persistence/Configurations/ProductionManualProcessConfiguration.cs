using MachineMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MachineMonitoring.Infrastructure.Persistence.Configurations;

public sealed class ProductionManualProcessConfiguration : IEntityTypeConfiguration<ProductionManualProcess>
{
    public void Configure(EntityTypeBuilder<ProductionManualProcess> builder)
    {
        builder.ToTable("production_manual_processes");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.ProductionOrderId)
            .IsUnique();

        builder.Property(x => x.TotalMinutes)
            .HasPrecision(18, 2);
    }
}