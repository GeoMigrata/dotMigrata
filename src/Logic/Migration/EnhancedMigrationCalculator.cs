using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Values;
using dotGeoMigrata.Logic.Attraction;
using dotGeoMigrata.Logic.Interfaces;

namespace dotGeoMigrata.Logic.Migration;

/// <summary>
/// Enhanced migration calculator implementing the algorithm specified in LogicModel.md.
/// Implements sigmoid-based probability, cost decay, capacity constraints, and improved sampling.
/// </summary>
public sealed class EnhancedMigrationCalculator : IMigrationCalculator
{
    /// <summary>
    /// Gets or sets the sigmoid steepness coefficient (k) for probability calculation.
    /// Default is 1.0. Higher values make the transition steeper.
    /// </summary>
    public double SigmoidSteepness { get; init; } = 1.0;

    /// <summary>
    /// Gets or sets the cost sensitivity coefficient (λ) for exponential decay.
    /// Default is 0.01. Higher values mean costs reduce migration probability more.
    /// </summary>
    public double CostSensitivity { get; init; } = 0.01;

    /// <summary>
    /// Gets or sets the base migration cost per unit distance.
    /// Default is 1.0 (represents cost per kilometer).
    /// </summary>
    public double BaseMigrationCost { get; init; } = 1.0;

    /// <summary>
    /// Calculates potential migration flows for a population group definition from a source city
    /// using the enhanced algorithm with sigmoid probability and cost decay.
    /// </summary>
    /// <param name="sourceCity">The city from which migration originates.</param>
    /// <param name="groupDefinition">The population group definition considering migration.</param>
    /// <param name="attractions">Attraction results for all cities.</param>
    /// <param name="world">The world context.</param>
    /// <param name="random">Random number generator for probabilistic sampling.</param>
    /// <returns>List of migration flows.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public IReadOnlyList<MigrationFlow> CalculateMigrationFlows(
        City sourceCity,
        PopulationGroupDefinition groupDefinition,
        IReadOnlyList<AttractionResult> attractions,
        World world,
        Random random)
    {
        ArgumentNullException.ThrowIfNull(sourceCity, nameof(sourceCity));
        ArgumentNullException.ThrowIfNull(groupDefinition, nameof(groupDefinition));
        ArgumentNullException.ThrowIfNull(attractions, nameof(attractions));
        ArgumentNullException.ThrowIfNull(world, nameof(world));
        ArgumentNullException.ThrowIfNull(random, nameof(random));

        // Get the population count for this group in the source city
        if (!sourceCity.TryGetPopulationGroupValue(groupDefinition, out var groupValue) || groupValue is null)
            return [];

        var populationCount = groupValue.Population;
        if (populationCount <= 0)
            return [];

        // Find the attraction of the source city
        var sourceAttraction = attractions.FirstOrDefault(a => a.City == sourceCity);
        if (sourceAttraction is null)
            return [];

        var flows = new List<MigrationFlow>();

        // For each destination city, calculate migration flow
        foreach (var destAttraction in attractions)
        {
            if (destAttraction.City == sourceCity)
                continue;

            // Calculate attraction difference: ΔA = A(C,G) - A(O,G)
            var attractionDiff = destAttraction.AttractionScore - sourceAttraction.AttractionScore;

            // Check migration threshold: ΔA > τ
            if (attractionDiff <= groupDefinition.AttractionThreshold)
                continue;

            // Check minimum acceptable attraction: A(C,G) > α_min
            if (destAttraction.AttractionScore <= groupDefinition.MinimumAcceptableAttraction)
                continue;

            // Calculate migration cost based on distance
            var distance = sourceCity.Location.DistanceTo(destAttraction.City.Location);
            var migrationCost = BaseMigrationCost * distance;

            // Calculate raw migration rate using sigmoid function:
            // rawRate = 1 / (1 + exp(-k * (ΔA - 0)))
            var rawRate = CalculateSigmoidProbability(attractionDiff, SigmoidSteepness);

            // Apply cost decay factor: p_C = rawRate × e^(-λ * d)
            var migrationProbability = rawRate * Math.Exp(-CostSensitivity * migrationCost);

            // Ensure probability is in valid range [0, 1]
            migrationProbability = Math.Clamp(migrationProbability, 0.0, 1.0);

            if (migrationProbability <= 0)
                continue;

            // Calculate potential migrants considering moving willingness and retention
            // M = N_G × m (where m is moving willingness upper limit)
            var potentialMigrants = (int)(populationCount * groupDefinition.MovingWillingness);

            // Apply retention rate: effective migration = (1 - r) × potential
            var effectiveProbability = (1.0 - groupDefinition.RetentionRate) * migrationProbability;
            effectiveProbability = Math.Clamp(effectiveProbability, 0.0, 1.0);

            // Sample actual migrants using probabilistic sampling
            var actualMigrants = SampleMigrants(potentialMigrants, effectiveProbability, random);

            if (actualMigrants > 0)
                flows.Add(new MigrationFlow
                {
                    SourceCity = sourceCity,
                    DestinationCity = destAttraction.City,
                    PopulationGroupDefinition = groupDefinition,
                    MigrantCount = actualMigrants,
                    MigrationProbability = effectiveProbability,
                    AttractionDifference = attractionDiff
                });
        }

        // Apply capacity constraints if destinations have capacity limits
        flows = ApplyCapacityConstraints(flows).ToList();

        return flows;
    }

    /// <summary>
    /// Calculates sigmoid probability: 1 / (1 + exp(-k * x))
    /// </summary>
    /// <param name="value">The input value (attraction difference).</param>
    /// <param name="steepness">The steepness coefficient.</param>
    /// <returns>Sigmoid probability in range [0, 1].</returns>
    private static double CalculateSigmoidProbability(double value, double steepness)
    {
        return 1.0 / (1.0 + Math.Exp(-steepness * value));
    }

    /// <summary>
    /// Samples the actual number of migrants using binomial-like distribution.
    /// For small populations, uses individual sampling; for large populations, uses normal approximation.
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

        // For small populations (≤100), sample each individual independently
        if (totalPopulation <= 100)
            return Enumerable.Range(0, totalPopulation)
                .Count(_ => random.NextDouble() < probability);

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

    /// <summary>
    /// Applies capacity constraints to migration flows.
    /// If a city's capacity is exceeded, scales down migrations proportionally.
    /// </summary>
    /// <param name="flows">The initial migration flows.</param>
    /// <returns>Adjusted migration flows respecting capacity constraints.</returns>
    private static IEnumerable<MigrationFlow> ApplyCapacityConstraints(List<MigrationFlow> flows)
    {
        // Group flows by destination city
        var flowsByDestination = flows
            .GroupBy(f => f.DestinationCity)
            .ToList();

        foreach (var destGroup in flowsByDestination)
        {
            var destCity = destGroup.Key;
            var destFlows = destGroup.ToList();

            // Check if city has capacity constraint
            if (destCity.Capacity is null or <= 0)
            {
                // No capacity constraint, return all flows as-is
                foreach (var flow in destFlows)
                    yield return flow;
                continue;
            }

            var currentPopulation = destCity.Population;
            var totalInflow = destFlows.Sum(f => f.MigrantCount);
            var remainingCapacity = destCity.Capacity.Value - currentPopulation;

            // If within capacity, return all flows as-is
            if (totalInflow <= remainingCapacity)
            {
                foreach (var flow in destFlows)
                    yield return flow;
                continue;
            }

            // Capacity exceeded - scale down proportionally
            var scalingFactor = (double)remainingCapacity / totalInflow;

            foreach (var flow in destFlows)
            {
                var adjustedCount = (int)Math.Floor(flow.MigrantCount * scalingFactor);
                if (adjustedCount > 0) yield return flow with { MigrantCount = adjustedCount };
            }
        }
    }
}