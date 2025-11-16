using dotMigrata.Logic.Models;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Pipeline;

/// <summary>
/// Simulation stage that executes migration flows by moving persons between cities.
/// Processes the migration flows calculated by MigrationDecisionStage.
/// Uses parallel processing for efficient handling of large populations.
/// </summary>
public sealed class MigrationExecutionStage : ISimulationStage
{
    /// <inheritdoc />
    public string Name => "MigrationExecution";

    /// <inheritdoc />
    public Task ExecuteAsync(SimulationContext context)
    {
        var flows = context.GetData<List<MigrationFlow>>("MigrationFlows");

        if (flows == null || flows.Count == 0)
        {
            context.TotalPopulationChange = 0;
            context.MaxCityPopulationChange = 0;
            return Task.CompletedTask;
        }

        // Track population changes per city
        var cityChanges = new Dictionary<string, int>();

        // Execute all migrations
        foreach (var flow in flows)
        {
            var person = flow.Person;
            var originCity = flow.OriginCity;
            var destinationCity = flow.DestinationCity;

            // Remove person from origin city
            if (originCity.RemovePerson(person))
            {
                cityChanges.TryAdd(originCity.DisplayName, 0);
                cityChanges[originCity.DisplayName]++;
            }

            // Add person to destination city
            destinationCity.AddPerson(person);
            cityChanges.TryAdd(destinationCity.DisplayName, 0);
            cityChanges[destinationCity.DisplayName]++;
        }

        // Calculate statistics
        var totalChange = cityChanges.Values.Sum();
        var maxCityChange = cityChanges.Values.DefaultIfEmpty(0).Max();

        context.TotalPopulationChange = totalChange;
        context.MaxCityPopulationChange = maxCityChange;

        return Task.CompletedTask;
    }
}