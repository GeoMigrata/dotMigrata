using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Values;
using dotGeoMigrata.Logic.Common;
using dotGeoMigrata.Logic.Interfaces;
using dotGeoMigrata.Logic.Models;

namespace dotGeoMigrata.Logic.Calculators;

/// <summary>
/// Standard migration calculator implementing the model described in model.md.
/// Calculates migration flows based on attraction differences and population group willingness.
/// </summary>
public class StandardMigrationCalculator : IMigrationCalculator
{
    private readonly StandardModelConfig _config;

    /// <summary>
    /// Initializes a new instance of the StandardMigrationCalculator.
    /// </summary>
    /// <param name="config">Configuration parameters for the calculator. If null, uses default configuration.</param>
    public StandardMigrationCalculator(StandardModelConfig? config = null)
    {
        _config = config ?? StandardModelConfig.Default;
    }

    /// <inheritdoc />
    public IEnumerable<MigrationFlow> CalculateMigrationFlows(
        City originCity,
        IEnumerable<City> destinationCities,
        GroupDefinition group,
        int currentPopulation,
        IDictionary<City, AttractionResult> attractionResults)
    {
        if (currentPopulation <= 0)
            return [];

        // Get origin city attraction
        if (!attractionResults.TryGetValue(originCity, out var originAttraction))
            return [];

        // Calculate migration probabilities for each destination
        var destinations = destinationCities.Where(c => c != originCity).ToList();
        var migrationProbabilities = new Dictionary<City, double>();

        foreach (var destCity in destinations)
        {
            if (!attractionResults.TryGetValue(destCity, out var destAttraction))
                continue;

            // Calculate attraction difference modified by move willingness
            var attractionDiff = (destAttraction.AdjustedAttraction - originAttraction.AdjustedAttraction)
                                 * group.MovingWillingness;

            // Convert to migration probability using sigmoid
            var probability = MathUtils.Sigmoid(
                attractionDiff,
                _config.MigrationProbabilitySteepness,
                _config.MigrationProbabilityThreshold);

            migrationProbabilities[destCity] = probability;
        }

        if (migrationProbabilities.Count == 0)
            return [];

        // Calculate expected number of migrations
        var expectedMigrants = currentPopulation * group.MovingWillingness;

        // Normalize probabilities using softmax to get migration distribution
        var probValues = migrationProbabilities.Values.ToList();
        var normalizedProbs = MathUtils.Softmax(probValues);

        // Create migration flows
        var flows = new List<MigrationFlow>();
        var cities = migrationProbabilities.Keys.ToList();

        for (var i = 0; i < cities.Count; i++)
        {
            var destCity = cities[i];
            var migrationShare = normalizedProbs[i];
            var migrationCount = expectedMigrants * migrationShare;

            // Only create flow if migration count is meaningful
            if (migrationCount > .01)
                flows.Add(new MigrationFlow
                {
                    OriginCity = originCity,
                    DestinationCity = destCity,
                    Group = group,
                    MigrationCount = migrationCount,
                    MigrationProbability = migrationProbabilities[destCity]
                });
        }

        return flows;
    }

    /// <inheritdoc />
    public IEnumerable<MigrationFlow> CalculateAllMigrationFlows(
        World world,
        IAttractionCalculator attractionCalculator)
    {
        var allFlows = new List<MigrationFlow>();

        foreach (var group in world.GroupDefinitions)
        foreach (var originCity in world.Cities)
        {
            // get current population for this group in this city
            if (!originCity.TryGetPopulationGroupValue(group, out var gv) || gv!.Population <= 0)
                continue;

            // Calculate attractions for all cities from this origin
            var attractions = attractionCalculator.CalculateAttractionForAllCities(
                world.Cities,
                group,
                originCity);

            // Calculate migration flows
            var flows = CalculateMigrationFlows(
                originCity,
                world.Cities,
                group,
                gv.Population,
                attractions);

            allFlows.AddRange(flows);
        }

        return allFlows;
    }
}