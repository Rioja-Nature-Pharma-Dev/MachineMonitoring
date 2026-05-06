using MachineMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MachineMonitoring.Infrastructure.Persistence.Configurations;

public sealed class ProductionCounterConfiguration : IEntityTypeConfiguration<ProductionCounter>
{
    public void Configure(EntityTypeBuilder<ProductionCounter> builder)
    {
        builder.ToTable("production_counters");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.ProductionOrderId)
            .IsUnique();
    }
}