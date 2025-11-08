using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Logic.Interfaces;
using dotGeoMigrata.Simulation.Interfaces;

namespace dotGeoMigrata.Simulation.Pipeline.Stages;

/// <summary>
/// Pipeline stage that applies feedback effects to city factors based on population changes.
/// </summary>
public sealed class FeedbackStage : ISimulationStage
{
    private readonly IFeedbackCalculator _calculator;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedbackStage" /> class.
    /// </summary>
    /// <param name="calculator">The feedback calculator to use.</param>
    /// <exception cref="ArgumentNullException">Thrown when calculator is null.</exception>
    public FeedbackStage(IFeedbackCalculator calculator)
    {
        ArgumentNullException.ThrowIfNull(calculator);
        _calculator = calculator;
    }

    /// <inheritdoc />
    public string Name => "Feedback Application";

    /// <inheritdoc />
    public int Order => 400; // Fourth stage (after migration application)

    /// <inheritdoc />
    public SimulationStageResult Execute(SimulationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            // Retrieve previous populations from migration application stage
            if (!context.SharedData.TryGetValue("PreviousPopulations", out var populationsObj) ||
                populationsObj is not Dictionary<City, int> previousPopulations)
                return SimulationStageResult.Failed("Previous populations data not found from previous stage");

            // Apply feedback to each city
            var updatedCities = 0;
            foreach (var city in context.World.Cities)
            {
                if (!previousPopulations.TryGetValue(city, out var previousPopulation))
                    continue;
                _calculator.ApplyFeedback(city, previousPopulation, city.Population);
                updatedCities++;
            }

            return SimulationStageResult.Successful(
                $"Applied feedback to {updatedCities} cities");
        }
        catch (Exception ex)
        {
            return SimulationStageResult.Failed($"Feedback application failed: {ex.Message}");
        }
    }
}