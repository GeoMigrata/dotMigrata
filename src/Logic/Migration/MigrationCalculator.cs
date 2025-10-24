using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Logic.Attraction;

namespace dotGeoMigrata.Logic.Migration;

/// <summary>
/// Calculates migration flows between cities based on attraction differences.
/// </summary>
public sealed class MigrationCalculator
{
    /// <summary>
    /// Base migration cost per unit distance.
    /// </summary>
    public double BaseMigrationCost { get; init; } = 0.001;

    /// <summary>
    /// Minimum attraction difference required to trigger migration consideration.
    /// </summary>
    public double MinimumAttractionThreshold { get; init; } = 0.1;

    /// <summary>
    /// Calculates potential migration flows for a population group from a source city.
    /// </summary>
    /// <param name="sourceCity">The city from which migration originates.</param>
    /// <param name="group">The population group considering migration.</param>
    /// <param name="attractions">Attraction results for all cities.</param>
    /// <param name="world">The world context.</param>
    /// <param name="random">Random number generator for probabilistic sampling.</param>
    /// <returns>List of migration flows.</returns>
    public IReadOnlyList<MigrationFlow> CalculateMigrationFlows(
        City sourceCity,
        PopulationGroup group,
        IReadOnlyList<AttractionResult> attractions,
        World world,
        Random random)
    {
        if (sourceCity == null) throw new ArgumentNullException(nameof(sourceCity));
        if (group == null) throw new ArgumentNullException(nameof(group));
        if (attractions == null) throw new ArgumentNullException(nameof(attractions));
        if (world == null) throw new ArgumentNullException(nameof(world));
        if (random == null) throw new ArgumentNullException(nameof(random));

        var flows = new List<MigrationFlow>();

        // Find the attraction of the source city
        var sourceAttraction = attractions.FirstOrDefault(a => a.City == sourceCity);
        if (sourceAttraction == null) return flows;

        // Consider migration to each destination city
        foreach (var destAttraction in attractions)
        {
            if (destAttraction.City == sourceCity) continue;

            var attractionDiff = destAttraction.AttractionScore - sourceAttraction.AttractionScore;

            // Only consider migration if destination is more attractive
            if (attractionDiff <= MinimumAttractionThreshold) continue;

            // Calculate migration cost based on distance
            var distance = sourceCity.Position.DistanceTo(destAttraction.City.Position);
            var migrationCost = BaseMigrationCost * distance;

            // Net attraction after considering migration cost
            var netAttraction = attractionDiff - migrationCost;
            if (netAttraction <= 0) continue;

            // Calculate migration probability
            var baseProbability = group.MovingWillingness * (1.0 - group.RetentionRate);
            var attractionFactor = Math.Tanh(netAttraction); // Sigmoid-like function to bound probability
            var migrationProbability = baseProbability * attractionFactor;

            // Sample actual number of migrants
            var potentialMigrants = group.Count;
            var expectedMigrants = potentialMigrants * migrationProbability;

            // Use binomial-like sampling for more realistic partial migration
            var actualMigrants = SampleMigrants(potentialMigrants, migrationProbability, random);

            if (actualMigrants > 0)
            {
                flows.Add(new MigrationFlow
                {
                    SourceCity = sourceCity,
                    DestinationCity = destAttraction.City,
                    PopulationGroup = group,
                    MigrantCount = actualMigrants,
                    MigrationProbability = migrationProbability,
                    AttractionDifference = attractionDiff
                });
            }
        }

        return flows;
    }

    /// <summary>
    /// Samples the actual number of migrants using binomial-like distribution.
    /// </summary>
    private int SampleMigrants(int totalPopulation, double probability, Random random)
    {
        if (totalPopulation == 0 || probability <= 0) return 0;
        if (probability >= 1.0) return totalPopulation;

        // For small populations, sample each individual
        if (totalPopulation <= 100)
        {
            var count = 0;
            for (var i = 0; i < totalPopulation; i++)
            {
                if (random.NextDouble() < probability)
                    count++;
            }

            return count;
        }

        // For larger populations, use normal approximation to binomial
        var mean = totalPopulation * probability;
        var variance = totalPopulation * probability * (1 - probability);
        var stdDev = Math.Sqrt(variance);

        // Box-Muller transform for normal distribution
        var u1 = random.NextDouble();
        var u2 = random.NextDouble();
        var z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        var sample = mean + stdDev * z;

        return Math.Max(0, Math.Min(totalPopulation, (int)Math.Round(sample)));
    }
}