using MachineMonitoring.Infrastructure;
using MachineMonitoring.Api.Seeds;
using MachineMonitoring.Api.Mqtt;
using MachineMonitoring.Application.Abstractions.Repositories;
using MachineMonitoring.Application.Abstractions.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

// MQTT dynamic listener - reads configured GPIO mappings and processes messages
builder.Services.AddHostedService<DynamicMqttListener>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Seed database with Cremer machine on startup
    using (var scope = app.Services.CreateScope())
    {
        var machineRepo = scope.ServiceProvider.GetRequiredService<IMachineRepository>();
        var orderRepo = scope.ServiceProvider.GetRequiredService<IProductionOrderRepository>();
        var counterRepo = scope.ServiceProvider.GetRequiredService<IProductionCounterRepository>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();

        var seeder = new CremerMachineSeeder(machineRepo, orderRepo, counterRepo, clock);
        await seeder.SeedAsync();
    }
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();