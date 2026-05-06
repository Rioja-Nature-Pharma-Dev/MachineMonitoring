using MachineMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MachineMonitoring.Infrastructure.Persistence.Configurations;

public sealed class ProductionMetricsConfiguration : IEntityTypeConfiguration<ProductionMetrics>
{
    public void Configure(EntityTypeBuilder<ProductionMetrics> builder)
    {
        builder.ToTable("production_metrics");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.ProductionOrderId)
            .IsUnique();

        builder.Property(x => x.TotalMinutes).HasPrecision(18, 2);
        builder.Property(x => x.PausedMinutes).HasPrecision(18, 2);
        builder.Property(x => x.ActiveMinutes).HasPrecision(18, 2);
        builder.Property(x => x.Availability).HasPrecision(18, 4);
        builder.Property(x => x.Performance).HasPrecision(18, 4);
        builder.Property(x => x.Quality).HasPrecision(18, 4);
        builder.Property(x => x.Oee).HasPrecision(18, 4);
        builder.Property(x => x.RealStandard).HasPrecision(18, 4);
        builder.Property(x => x.OrderFulfillment).HasPrecision(18, 4);
    }
}