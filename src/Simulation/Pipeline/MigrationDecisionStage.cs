using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Values;
using dotGeoMigrata.Logic.Interfaces;
using dotGeoMigrata.Logic.Models;
using dotGeoMigrata.Simulation.Interfaces;
using dotGeoMigrata.Simulation.Models;

namespace dotGeoMigrata.Simulation.Pipeline;

/// <summary>
/// Simulation stage that calculates migration flows based on attraction differences.
/// Uses pre-calculated attraction results from the AttractionCalculationStage.
/// </summary>
public sealed class MigrationDecisionStage : ISimulationStage
{
    private readonly IMigrationCalculator _migrationCalculator;

    /// <summary>
    /// Initializes a new instance of the MigrationDecisionStage.
    /// </summary>
    /// <param name="migrationCalculator">The calculator to use for determining migration flows.</param>
    public MigrationDecisionStage(IMigrationCalculator migrationCalculator)
    {
        _migrationCalculator = migrationCalculator ?? throw new ArgumentNullException(nameof(migrationCalculator));
    }

    /// <inheritdoc />
    public string Name => "MigrationDecision";

    /// <inheritdoc />
    public Task ExecuteAsync(SimulationContext context)
    {
        // Retrieve attractions from previous stage
        var allAttractions =
            context.GetData<Dictionary<(GroupDefinition, City), IDictionary<City, AttractionResult>>>("Attractions");
        if (allAttractions == null)
            throw new InvalidOperationException(
                "Attraction data not found. AttractionCalculationStage must run before MigrationDecisionStage.");

        var allFlows = new List<MigrationFlow>();

        // Calculate migration flows for each group-origin combination
        foreach (var group in context.World.GroupDefinitions)
        foreach (var originCity in context.World.Cities)
        {
            // Get current population
            if (!originCity.TryGetPopulationGroupValue(group, out var gv) || gv!.Population <= 0)
                continue;

            // Get pre-calculated attractions
            if (!allAttractions.TryGetValue((group, originCity), out var attractions))
                continue;

            // Calculate migration flows
            var flows = _migrationCalculator.CalculateMigrationFlows(
                originCity,
                context.World.Cities,
                group,
                gv.Population,
                attractions);

            allFlows.AddRange(flows);
        }

        // Store flows in context for use by execution stage
        context.CurrentMigrationFlows = allFlows;
        context.SetData("MigrationFlows", allFlows);

        return Task.CompletedTask;
    }
}