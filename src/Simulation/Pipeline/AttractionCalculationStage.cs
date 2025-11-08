using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Values;
using dotGeoMigrata.Logic.Interfaces;
using dotGeoMigrata.Logic.Models;
using dotGeoMigrata.Simulation.Interfaces;
using dotGeoMigrata.Simulation.Models;

namespace dotGeoMigrata.Simulation.Pipeline;

/// <summary>
/// Simulation stage that calculates attraction scores for all cities and population groups.
/// Stores results in the simulation context for use by subsequent stages.
/// </summary>
public sealed class AttractionCalculationStage : ISimulationStage
{
    private readonly IAttractionCalculator _attractionCalculator;

    /// <summary>
    /// Initializes a new instance of the AttractionCalculationStage.
    /// </summary>
    /// <param name="attractionCalculator">The calculator to use for computing attractions.</param>
    public AttractionCalculationStage(IAttractionCalculator attractionCalculator)
    {
        _attractionCalculator =
            attractionCalculator ?? throw new ArgumentNullException(nameof(attractionCalculator));
    }

    /// <inheritdoc />
    public string Name => "AttractionCalculation";

    /// <inheritdoc />
    public Task ExecuteAsync(SimulationContext context)
    {
        // Calculate attractions for all group-city combinations
        var allAttractions = new Dictionary<(GroupDefinition, City), IDictionary<City, AttractionResult>>();

        foreach (var group in context.World.GroupDefinitions)
        foreach (var originCity in context.World.Cities)
        {
            // Calculate attractions from this origin city
            var attractions = _attractionCalculator.CalculateAttractionForAllCities(
                context.World.Cities,
                group,
                originCity);

            allAttractions[(group, originCity)] = attractions;
        }

        // Store in context for use by migration stage
        context.SetData("Attractions", allAttractions);

        return Task.CompletedTask;
    }
}