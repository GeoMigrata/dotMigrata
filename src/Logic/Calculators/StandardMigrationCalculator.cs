using dotMigrata.Core.Entities;
using dotMigrata.Logic.Common;
using dotMigrata.Logic.Interfaces;
using dotMigrata.Logic.Models;

namespace dotMigrata.Logic.Calculators;

/// <summary>
/// Standard migration calculator implementing optimized individual-based migration decisions.
/// Calculates migration decisions based on attraction differences and individual willingness.
/// Uses parallel processing for performance with large populations.
/// </summary>
public class StandardMigrationCalculator : IMigrationCalculator
{
    private readonly StandardModelConfig _config;
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of the StandardMigrationCalculator.
    /// </summary>
    /// <param name="config">Configuration parameters for the calculator. If null, uses default configuration.</param>
    public StandardMigrationCalculator(StandardModelConfig? config = null)
    {
        _config = config ?? StandardModelConfig.Default;
        _random = new Random();
    }

    /// <inheritdoc />
    public MigrationFlow? CalculateMigrationDecision(
        Person person,
        IEnumerable<City> destinationCities,
        IDictionary<City, AttractionResult> attractionResults)
    {
        var originCity = person.CurrentCity;
        if (originCity == null)
            return null;

        // Get origin city attraction
        if (!attractionResults.TryGetValue(originCity, out var originAttraction))
            return null;

        // Find best destination city
        City? bestDestination = null;
        var bestProbability = 0.0;
        var destinations = destinationCities.Where(c => c != originCity).ToList();

        foreach (var destCity in destinations)
        {
            if (!attractionResults.TryGetValue(destCity, out var destAttraction))
                continue;

            // Calculate attraction difference modified by move willingness
            var attractionDiff = (destAttraction.AdjustedAttraction - originAttraction.AdjustedAttraction)
                                 * person.MovingWillingness;

            // Skip if below person's attraction threshold
            if (attractionDiff < person.AttractionThreshold)
                continue;

            // Skip if destination attraction below minimum acceptable
            if (destAttraction.AdjustedAttraction < person.MinimumAcceptableAttraction)
                continue;

            // Convert to migration probability using sigmoid
            var probability = MathUtils.Sigmoid(
                attractionDiff,
                _config.MigrationProbabilitySteepness,
                _config.MigrationProbabilityThreshold);

            if (!(probability > bestProbability)) continue;
            bestProbability = probability;
            bestDestination = destCity;
        }

        // No suitable destination found
        if (bestDestination == null || bestProbability <= 0.0)
            return null;

        // Apply retention rate - person may decide to stay
        var retentionRoll = _random.NextDouble();
        if (retentionRoll < person.RetentionRate)
            return null;

        // Make probabilistic migration decision
        var migrationRoll = _random.NextDouble();
        if (migrationRoll > bestProbability)
            return null;

        // Person decides to migrate
        return new MigrationFlow
        {
            OriginCity = originCity,
            DestinationCity = bestDestination,
            Person = person,
            MigrationProbability = bestProbability
        };
    }

    /// <inheritdoc />
    public IEnumerable<MigrationFlow> CalculateAllMigrationFlows(
        World world,
        IAttractionCalculator attractionCalculator)
    {
        var allPersons = world.AllPersons.ToList();

        // Process persons in parallel for performance
        var flows = allPersons
            .AsParallel()
            .Select(person =>
            {
                // Calculate attractions for all cities for this person
                var attractions = attractionCalculator.CalculateAttractionForAllCities(
                    world.Cities,
                    person,
                    person.CurrentCity);

                // Calculate migration decision
                return CalculateMigrationDecision(person, world.Cities, attractions);
            })
            .Where(flow => flow != null)
            .Cast<MigrationFlow>()
            .ToList();

        return flows;
    }
}