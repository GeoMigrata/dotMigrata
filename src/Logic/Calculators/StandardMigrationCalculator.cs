using dotMigrata.Core.Entities;
using dotMigrata.Logic.Common;
using dotMigrata.Logic.Interfaces;
using dotMigrata.Logic.Models;

namespace dotMigrata.Logic.Calculators;

/// <summary>
/// Standard migration calculator implementing optimized individual-based migration decisions.
/// Calculates migration decisions based on attraction differences and individual willingness.
/// Uses parallel processing for performance with large populations.
/// Thread-safe implementation using ThreadLocal for random number generation.
/// </summary>
public class StandardMigrationCalculator : IMigrationCalculator
{
    private readonly StandardModelConfig _config;

    // Thread-local random for thread-safe parallel processing
    private readonly ThreadLocal<Random> _threadLocalRandom;

    /// <summary>
    /// Initializes a new instance of the StandardMigrationCalculator.
    /// </summary>
    /// <param name="config">Configuration parameters for the calculator. If null, uses default configuration.</param>
    /// <param name="seed">Optional seed for reproducible random number generation.</param>
    /// <exception cref="Core.Exceptions.ConfigurationException">Thrown when the configuration is invalid.</exception>
    public StandardMigrationCalculator(StandardModelConfig? config = null, int? seed = null)
    {
        _config = config ?? StandardModelConfig.Default.Validate();
        var seed1 = seed;

        // Create thread-local random with optional seeding for reproducibility
        _threadLocalRandom = new ThreadLocal<Random>(() =>
        {
            if (!seed1.HasValue) return new Random();
            // Use seed combined with thread ID for reproducible but different per-thread values
            var threadSeed = seed1.Value ^ Environment.CurrentManagedThreadId;
            return new Random(threadSeed);
        }, false);
    }

    private Random Random => _threadLocalRandom.Value!;

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

        // Find the best destination city
        City? bestDestination = null;
        var bestProbability = 0.0;
        var destinations = destinationCities.Where(c => c != originCity).ToList();

        foreach (var destCity in destinations)
        {
            if (!attractionResults.TryGetValue(destCity, out var destAttraction))
                continue;

            // Calculate attraction difference modified by move willingness
            var attractionDiff = (destAttraction.AdjustedAttraction - originAttraction.AdjustedAttraction)
                                 * person.MovingWillingness.Value;

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
        var retentionRoll = Random.NextDouble();
        if (retentionRoll < person.RetentionRate.Value)
            return null;

        // Make probabilistic migration decision
        var migrationRoll = Random.NextDouble();
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

        // Configure parallel or sequential processing based on config
        IEnumerable<Person> query = allPersons;

        if (_config.UseParallelProcessing)
        {
            var parallel = allPersons.AsParallel();
            if (_config.MaxDegreeOfParallelism.HasValue)
                parallel = parallel.WithDegreeOfParallelism(_config.MaxDegreeOfParallelism.Value);

            query = parallel;
        }

        // Process persons (parallel or sequential)
        var flows = query
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