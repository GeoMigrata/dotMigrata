using dotGeoMigrata.Logic.Interfaces;
using dotGeoMigrata.Simulation.Interfaces;
using dotGeoMigrata.Simulation.Models;

namespace dotGeoMigrata.Simulation.Pipeline;

/// <summary>
/// Simulation stage that calculates migration decisions for all persons based on attraction differences.
/// Uses the migration calculator to determine which persons will migrate and to which cities.
/// </summary>
public sealed class MigrationDecisionStage : ISimulationStage
{
    private readonly IAttractionCalculator _attractionCalculator;
    private readonly IMigrationCalculator _migrationCalculator;

    /// <summary>
    /// Initializes a new instance of the MigrationDecisionStage.
    /// </summary>
    /// <param name="migrationCalculator">The calculator to use for determining migration decisions.</param>
    /// <param name="attractionCalculator">The calculator to use for calculating attractions.</param>
    public MigrationDecisionStage(
        IMigrationCalculator migrationCalculator,
        IAttractionCalculator attractionCalculator)
    {
        _migrationCalculator = migrationCalculator ?? throw new ArgumentNullException(nameof(migrationCalculator));
        _attractionCalculator = attractionCalculator ?? throw new ArgumentNullException(nameof(attractionCalculator));
    }

    /// <inheritdoc />
    public string Name => "MigrationDecision";

    /// <inheritdoc />
    public Task ExecuteAsync(SimulationContext context)
    {
        // Calculate all migration decisions using parallel processing
        var flows = _migrationCalculator.CalculateAllMigrationFlows(context.World, _attractionCalculator);
        var flowList = flows.ToList();

        // Store flows in context for use by execution stage
        context.CurrentMigrationFlows = flowList;
        context.SetData("MigrationFlows", flowList);

        return Task.CompletedTask;
    }
}