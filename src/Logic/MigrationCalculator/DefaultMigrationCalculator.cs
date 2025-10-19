using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Core.Domain.Values;
using dotGeoMigrata.Interfaces.Logic;

namespace dotGeoMigrata.Logic.MigrationCalculator;

/// <summary>
/// Default implementation of the migration calculator.
/// Computes migration flows based on attraction differences,
/// thresholds, and distance-based decay.
/// </summary>
public sealed class DefaultMigrationCalculator : IMigrationCalculator
{
    private readonly double _sigmoidSteepness;
    private readonly double _distanceCostFactor;
    private readonly Random _rng = new();

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="sigmoidSteepness">
    /// Controls how sharply migration probability increases with attraction difference.
    /// Typical range: 2.0 - 8.0.
    /// </param>
    /// <param name="distanceCostFactor">
    /// Controls how strongly distance reduces migration probability.
    /// Set to 0 to ignore distance.
    /// </param>
    public DefaultMigrationCalculator(double sigmoidSteepness = 5.0, double distanceCostFactor = 0.001)
    {
        _sigmoidSteepness = sigmoidSteepness;
        _distanceCostFactor = distanceCostFactor;
    }

    /// <summary>
    /// Computes expected migration numbers for all population groups between cities,
    /// using an origin-aware attraction function.
    /// </summary>
    /// <param name="world">The simulated world containing all cities and groups.</param>
    /// <param name="attractionFunc">
    /// A delegate returning attraction index for (group, origin, target).
    /// </param>
    /// <returns>
    /// Dictionary of migration results:
    /// key = (origin city, target city, group), value = expected migrants (double).
    /// </returns>
    public IReadOnlyDictionary<(City origin, City target, PopulationGroup group), double>
        ComputeMigrations(World world, Func<PopulationGroup, City, City, double> attractionFunc)
    {
        var result = new Dictionary<(City, City, PopulationGroup), double>();

        foreach (var origin in world.Cities)
        {
            foreach (var group in origin.PopulationGroups)
            {
                // Compute attraction for the origin city (self-attraction)
                var aOrigin = attractionFunc(group, origin, origin);

                foreach (var target in world.Cities)
                {
                    if (target == origin)
                        continue;

                    // Compute attraction for the target city relative to this origin
                    var aTarget = attractionFunc(group, origin, target);
                    
                    Console.WriteLine($"[{group.DisplayName}] {origin.DisplayName}->{target.DisplayName} = {(aTarget - aOrigin):F3}");

                    // Trigger condition: only consider migration if thresholds are met
                    if (aOrigin > group.AcceptanceThreshold || aTarget < group.MigrationThreshold)
                        continue;

                    var delta = aTarget - aOrigin;

                    // Step 1. Convert attraction difference into a probability (sigmoid)
                    var rawProb = 1.0 / (1.0 + Math.Exp(-_sigmoidSteepness * delta));

                    // Step 2. Apply distance decay (if enabled)
                    var distance = Coordinate.CalculateDistance(origin.Position, target.Position);
                    var prob = _distanceCostFactor > 0
                        ? rawProb * Math.Exp(-_distanceCostFactor * distance)
                        : rawProb;

                    // Step 3. Compute expected migrants
                    var movable = group.Count * group.MovingWillingness;
                    var migrants = movable * prob;

                    // (Optional) Sampling could be done here if stochastic mode is desired
                    // int actualMigrants = BinomialSample(movable, prob);

                    if (migrants > 0.0)
                        result[(origin, target, group)] = migrants;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Simple binomial sampling (optional stochastic mode).
    /// </summary>
    private int BinomialSample(double n, double p)
    {
        var successes = 0;
        for (var i = 0; i < (int)n; i++)
        {
            if (_rng.NextDouble() < p)
                successes++;
        }

        return successes;
    }
}