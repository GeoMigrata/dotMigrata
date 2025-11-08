using dotGeoMigrata.Logic.Attraction;
using dotGeoMigrata.Logic.Interfaces;
using dotGeoMigrata.Logic.Migration;
using dotGeoMigrata.Simulation.Interfaces;

namespace dotGeoMigrata.Simulation.Pipeline.Stages;

/// <summary>
/// Pipeline stage that calculates migration flows between cities based on attraction differences.
/// </summary>
public sealed class MigrationStage : ISimulationStage
{
    private readonly IMigrationCalculator _calculator;

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationStage" /> class.
    /// </summary>
    /// <param name="calculator">The migration calculator to use.</param>
    /// <exception cref="ArgumentNullException">Thrown when calculator is null.</exception>
    public MigrationStage(IMigrationCalculator calculator)
    {
        ArgumentNullException.ThrowIfNull(calculator);
        _calculator = calculator;
    }

    /// <inheritdoc />
    public string Name => "Migration Calculation";

    /// <inheritdoc />
    public int Order => 200; // Second stage (after attraction)

    /// <inheritdoc />
    public SimulationStageResult Execute(SimulationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            // Retrieve attractions from previous stage
            if (!context.SharedData.TryGetValue("Attractions", out var attractionsObj) ||
                attractionsObj is not Dictionary<string, IReadOnlyList<AttractionResult>> attractionsDict)
                return SimulationStageResult.Failed("Attractions data not found from previous stage");

            // For each city and each population group definition, calculate migrations
            var allMigrationFlows = new List<MigrationFlow>();

            foreach (var city in context.World.Cities)
            foreach (var groupDefinition in context.World.PopulationGroupDefinitions)
            {
                if (!attractionsDict.TryGetValue(groupDefinition.DisplayName, out var attractions)) continue;
                var flows = _calculator.CalculateMigrationFlows(
                    city, groupDefinition, attractions, context.World, context.Random);
                allMigrationFlows.AddRange(flows);
            }

            // Store flows in shared data for next stages
            context.SharedData["MigrationFlows"] = allMigrationFlows;

            return SimulationStageResult.Successful(
                $"Calculated {allMigrationFlows.Count} migration flows",
                allMigrationFlows);
        }
        catch (Exception ex)
        {
            return SimulationStageResult.Failed($"Migration calculation failed: {ex.Message}");
        }
    }
}