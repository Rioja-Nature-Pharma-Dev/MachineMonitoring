using MachineMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MachineMonitoring.Infrastructure.Persistence.Configurations;

public sealed class MachineInputSourceConfiguration : IEntityTypeConfiguration<MachineInputSource>
{
    public void Configure(EntityTypeBuilder<MachineInputSource> builder)
    {
        builder.ToTable("machine_input_sources");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SourceType)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.EndpointOrTopic)
            .IsRequired()
            .HasMaxLength(300);

        builder.HasIndex(x => x.EndpointOrTopic);
    }
}
