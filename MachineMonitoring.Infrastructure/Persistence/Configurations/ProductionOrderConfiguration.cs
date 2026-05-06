using MachineMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MachineMonitoring.Infrastructure.Persistence.Configurations;

public sealed class ProductionOrderConfiguration : IEntityTypeConfiguration<ProductionOrder>
{
    public void Configure(EntityTypeBuilder<ProductionOrder> builder)
    {
        builder.ToTable("production_orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.OrderCode)
            .IsUnique();

        builder.Property(x => x.OperatorName)
            .HasMaxLength(100);

        builder.Property(x => x.Batch)
            .HasMaxLength(50);

        builder.Property(x => x.Article)
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.BoxType)
            .HasMaxLength(100);

        builder.Property(x => x.BottleFormat)
            .HasMaxLength(100);

        builder.Property(x => x.ProductType)
            .HasMaxLength(100);

        builder.Property(x => x.StandardReference)
            .HasPrecision(18, 4);

        builder.Property(x => x.EstimatedMinutes)
            .HasPrecision(18, 2);
    }
}