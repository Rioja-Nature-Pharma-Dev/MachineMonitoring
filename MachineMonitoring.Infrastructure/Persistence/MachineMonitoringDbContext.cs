using MachineMonitoring.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace MachineMonitoring.Infrastructure.Persistence;

public sealed class MachineMonitoringDbContext : DbContext
{
    public MachineMonitoringDbContext(DbContextOptions<MachineMonitoringDbContext> options)
        : base(options)
    {
    }

    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<MachineInputSource> MachineInputSources => Set<MachineInputSource>();
    public DbSet<MachineParameterDefinition> MachineParameterDefinitions => Set<MachineParameterDefinition>();
    public DbSet<MachineInputMapping> MachineInputMappings => Set<MachineInputMapping>();
    public DbSet<MachineReadingRaw> MachineReadingRaws => Set<MachineReadingRaw>();
    public DbSet<MachineReadingNormalized> MachineReadingNormalizeds => Set<MachineReadingNormalized>();
    public DbSet<MachineStateSnapshot> MachineStateSnapshots => Set<MachineStateSnapshot>();
    public DbSet<MachineAlert> MachineAlerts => Set<MachineAlert>();

    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();
    public DbSet<ProductionPause> ProductionPauses => Set<ProductionPause>();
    public DbSet<ProductionCounter> ProductionCounters => Set<ProductionCounter>();
    public DbSet<ProductionMetrics> ProductionMetrics => Set<ProductionMetrics>();
    public DbSet<ProductionManualProcess> ProductionManualProcesses => Set<ProductionManualProcess>();
    public DbSet<OrderAudit> OrderAudits => Set<OrderAudit>();

    public DbSet<MachineRuleDefinition> MachineRuleDefinitions => Set<MachineRuleDefinition>();
    public DbSet<CalculatedMetricDefinition> CalculatedMetricDefinitions => Set<CalculatedMetricDefinition>();
    public DbSet<AlertRuleDefinition> AlertRuleDefinitions => Set<AlertRuleDefinition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MachineMonitoringDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}