using dotGeoMigrata.Logic.Models;
using dotGeoMigrata.Simulation.Interfaces;
using dotGeoMigrata.Simulation.Models;

namespace dotGeoMigrata.Simulation.Pipeline;

/// <summary>
/// Simulation stage that executes migration flows by updating city population counts.
/// Processes the migration flows calculated by MigrationDecisionStage.
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

        // Track population changes per city per group for net migration
        var cityGroupChanges = new Dictionary<(string cityName, string groupName), double>();

        // Calculate net changes from all flows
        foreach (var flow in flows)
        {
            var originKey = (flow.OriginCity.DisplayName, flow.Group.DisplayName);
            var destKey = (flow.DestinationCity.DisplayName, flow.Group.DisplayName);

            // Decrease origin
            cityGroupChanges.TryAdd(originKey, 0);
            cityGroupChanges[originKey] -= flow.MigrationCount;

            // Increase destination
            cityGroupChanges.TryAdd(destKey, 0);
            cityGroupChanges[destKey] += flow.MigrationCount;
        }

        // Apply the net changes
        var totalChange = 0;
        var maxCityChange = 0;

        foreach (var ((cityName, groupName), change) in cityGroupChanges)
        {
            // Find the city and group
            var city = context.World.Cities.FirstOrDefault(c => c.DisplayName == cityName);
            var group = context.World.GroupDefinitions.FirstOrDefault(g => g.DisplayName == groupName);

            if (city == null || group == null)
                continue;

            if (!city.TryGetPopulationGroupValue(group, out var gv))
                continue;

            // Calculate new population (rounded to integer)
            var currentPop = gv!.Population;
            var newPop = Math.Max(0, currentPop + (int)Math.Round(change));

            // Update population
            city.UpdatePopulationCount(group, newPop);

            // Track statistics
            var absChange = Math.Abs(newPop - currentPop);
            totalChange += absChange;
            maxCityChange = Math.Max(maxCityChange, absChange);
        }

        context.TotalPopulationChange = totalChange;
        context.MaxCityPopulationChange = maxCityChange;

        return Task.CompletedTask;
    }
}