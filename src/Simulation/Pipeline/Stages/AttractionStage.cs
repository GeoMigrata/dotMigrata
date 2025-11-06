using dotGeoMigrata.Logic.Attraction;
using dotGeoMigrata.Logic.Interfaces;
using dotGeoMigrata.Simulation.Pipeline;

namespace dotGeoMigrata.Simulation.Pipeline.Stages;

/// <summary>
/// Pipeline stage that calculates attraction scores for all cities and population groups.
/// </summary>
public sealed class AttractionStage : ISimulationStage
{
    private readonly IAttractionCalculator _calculator;

    /// <summary>
    /// Initializes a new instance of the AttractionStage class.
    /// </summary>
    /// <param name="calculator">The attraction calculator to use.</param>
    /// <exception cref="ArgumentNullException">Thrown when calculator is null.</exception>
    public AttractionStage(IAttractionCalculator calculator)
    {
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
    }

    /// <inheritdoc />
    public string Name => "Attraction Calculation";

    /// <inheritdoc />
    public int Order => 100; // First stage

    /// <inheritdoc />
    public SimulationStageResult Execute(SimulationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            // Calculate attractions for all population group definitions
            var allAttractions = new Dictionary<string, IReadOnlyList<AttractionResult>>();

            foreach (var groupDefinition in context.World.PopulationGroupDefinitions)
            {
                var attractions = _calculator.CalculateAttractionForAllCities(
                    context.World,
                    groupDefinition);

                allAttractions[groupDefinition.DisplayName] = attractions;
            }

            // Store results in shared data for next stages
            context.SharedData["Attractions"] = allAttractions;

            return SimulationStageResult.Successful(
                $"Calculated attractions for {allAttractions.Count} population groups",
                allAttractions);
        }
        catch (Exception ex)
        {
            return SimulationStageResult.Failed($"Attraction calculation failed: {ex.Message}");
        }
    }
}