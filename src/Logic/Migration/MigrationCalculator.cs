using dotGeoMigrata.Core.Entities;
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
    public double BaseMigrationCost { get; init; } = .001;

    /// <summary>
    /// Minimum attraction difference required to trigger migration consideration.
    /// </summary>
    public double MinimumAttractionThreshold { get; init; } = .1;

    /// <summary>
    /// Calculates potential migration flows for a population group from a source city.
    /// </summary>
    /// <param name="sourceCity">The city from which migration originates.</param>
    /// <param name="group">The population group considering migration.</param>
    /// <param name="attractions">Attraction results for all cities.</param>
    /// <param name="world">The world context.</param>
    /// <param name="random">Random number generator for probabilistic sampling.</param>
    /// <returns>List of migration flows.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public IReadOnlyList<MigrationFlow> CalculateMigrationFlows(
        City sourceCity,
        PopulationGroup group,
        IReadOnlyList<AttractionResult> attractions,
        World world,
        Random random)
    {
        ArgumentNullException.ThrowIfNull(sourceCity);
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(attractions);
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(random);

        // Find the attraction of the source city
        var sourceAttraction = attractions.FirstOrDefault(a => a.City == sourceCity);
        if (sourceAttraction is null)
            return [];

        // Consider migration to each destination city

        return (from destAttraction in attractions
                where destAttraction.City != sourceCity
                let attractionDiff = destAttraction.AttractionScore - sourceAttraction.AttractionScore
                where !(attractionDiff <= MinimumAttractionThreshold)
                let distance = sourceCity.Position.DistanceTo(destAttraction.City.Position)
                let migrationCost = BaseMigrationCost * distance
                let netAttraction = attractionDiff - migrationCost
                where !(netAttraction <= 0)
                let baseProbability = @group.MovingWillingness * (1.0 - @group.RetentionRate)
                let attractionFactor = Math.Tanh(netAttraction)
                let migrationProbability = baseProbability * attractionFactor
                let actualMigrants = SampleMigrants(@group.Count, migrationProbability, random)
                where actualMigrants > 0
                select new MigrationFlow
                {
                    SourceCity = sourceCity,
                    DestinationCity = destAttraction.City,
                    PopulationGroup = @group,
                    MigrantCount = actualMigrants,
                    MigrationProbability = migrationProbability,
                    AttractionDifference = attractionDiff
                })
            .ToList();
    }

    /// <summary>
    /// Samples the actual number of migrants using binomial-like distribution.
    /// </summary>
    /// <param name="totalPopulation">Total population that could migrate.</param>
    /// <param name="probability">Migration probability (0-1).</param>
    /// <param name="random">Random number generator.</param>
    /// <returns>Number of actual migrants.</returns>
    private static int SampleMigrants(int totalPopulation, double probability, Random random)
    {
        if (totalPopulation <= 0 || probability <= 0)
            return 0;

        if (probability >= 1.0)
            return totalPopulation;

        // For small populations, sample each individual
        if (totalPopulation <= 100)
        {
            return Enumerable.Range(0, totalPopulation)
                .Count(_ => random.NextDouble() < probability);
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

        return Math.Clamp((int)Math.Round(sample), 0, totalPopulation);
    }
}