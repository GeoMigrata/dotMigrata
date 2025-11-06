using dotGeoMigrata.Logic.Migration;
using dotGeoMigrata.Simulation.Pipeline;

namespace dotGeoMigrata.Simulation.Pipeline.Stages;

/// <summary>
/// Pipeline stage that applies calculated migration flows to update city populations.
/// </summary>
public sealed class MigrationApplicationStage : ISimulationStage
{
    /// <inheritdoc />
    public string Name => "Migration Application";

    /// <inheritdoc />
    public int Order => 300; // Third stage (after migration calculation)

    /// <inheritdoc />
    public SimulationStageResult Execute(SimulationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            // Retrieve migration flows from previous stage
            if (!context.SharedData.TryGetValue("MigrationFlows", out var flowsObj) ||
                flowsObj is not List<MigrationFlow> flows)
            {
                return SimulationStageResult.Failed("Migration flows data not found from previous stage");
            }

            // Track previous populations for feedback calculation
            var previousPopulations = context.World.Cities
                .ToDictionary(c => c, c => c.Population);

            // Apply migrations
            var totalMigrants = ApplyMigrations(flows);

            // Store previous populations for feedback stage
            context.SharedData["PreviousPopulations"] = previousPopulations;
            context.SharedData["TotalMigrants"] = totalMigrants;

            return SimulationStageResult.Successful(
                $"Applied {flows.Count} migration flows, total migrants: {totalMigrants}",
                totalMigrants);
        }
        catch (Exception ex)
        {
            return SimulationStageResult.Failed($"Migration application failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Applies migration flows to update city populations.
    /// </summary>
    /// <param name="flows">List of migration flows to apply.</param>
    /// <returns>Total number of migrants.</returns>
    private static int ApplyMigrations(List<MigrationFlow> flows)
    {
        // Group flows by source city and population group definition
        var flowsBySource = flows
            .GroupBy(f => (f.SourceCity, f.PopulationGroupDefinition))
            .ToList();

        // Apply outflows (reduce population in source cities)
        foreach (var sourceGroup in flowsBySource)
        {
            var (sourceCity, groupDefinition) = sourceGroup.Key;
            var totalOutflow = sourceGroup.Sum(f => f.MigrantCount);

            if (sourceCity.TryGetPopulationGroupValue(groupDefinition, out var groupValue) && groupValue is not null)
            {
                groupValue.Population = Math.Max(0, groupValue.Population - totalOutflow);
            }
        }

        // Group flows by destination city and population group definition
        var flowsByDestination = flows
            .GroupBy(f => (f.DestinationCity, f.PopulationGroupDefinition))
            .ToList();

        // Apply inflows (increase population in destination cities)
        foreach (var destGroup in flowsByDestination)
        {
            var (destCity, groupDefinition) = destGroup.Key;
            var totalInflow = destGroup.Sum(f => f.MigrantCount);

            if (destCity.TryGetPopulationGroupValue(groupDefinition, out var groupValue) && groupValue is not null)
            {
                groupValue.Population += totalInflow;
            }
        }

        return flows.Sum(f => f.MigrantCount);
    }
}