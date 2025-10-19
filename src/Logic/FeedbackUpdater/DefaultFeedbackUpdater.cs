using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Core.Domain.Enums;
using dotGeoMigrata.Interfaces.Logic;

namespace dotGeoMigrata.Logic.FeedbackUpdater;

/// <summary>
/// Default implementation of feedback updater.
/// Updates city factor values based on population changes after migration.
/// </summary>
internal sealed class DefaultFeedbackUpdater : IFeedbackUpdater
{
    private readonly double _smoothingAlpha;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="smoothingAlpha">
    /// Smoothing coefficient (0-1). Smaller values mean slower change, more stability.
    /// </param>
    public DefaultFeedbackUpdater(double smoothingAlpha = .3)
    {
        _smoothingAlpha = Math.Clamp(smoothingAlpha, .0, 1.0);
    }

    /// <summary>
    /// Updates city factor values after migration, using population-based feedback logic.
    /// </summary>
    /// <param name="world">The simulated world after migration.</param>
    /// <param name="migrationFlows">Migration flow data from the migration calculator.</param>
    public void UpdateFactors(
        World world,
        IReadOnlyDictionary<(City origin, City target, PopulationGroup group), double> migrationFlows)
    {
        // Step 1. Compute total population change for each city
        var deltaPop = new Dictionary<City, double>();
        foreach (var city in world.Cities)
            deltaPop[city] = .0;

        foreach (var ((origin, target, group), migrants) in migrationFlows)
        {
            deltaPop[origin] -= migrants;
            deltaPop[target] += migrants;
        }

        // Step 2. Apply feedback logic for each city
        foreach (var city in world.Cities)
        {
            double totalPop = city.PopulationGroups.Sum(g => g.Count);
            if (totalPop <= 0) continue;

            var popChangeRatio = deltaPop[city] / totalPop;

            foreach (var fv in city.FactorValues)
            {
                var oldValue = fv.Intensity;

                // Basic heuristic rules (can be customized per factor type)
                var newValue = fv.Factor.Type switch
                {
                    FactorType.Positive =>
                        // More population => slight decrease (pressure on resources)
                        oldValue * (1 - .15 * popChangeRatio),
                    FactorType.Negative =>
                        // More population => increase (pollution, congestion)
                        oldValue * (1 + .25 * popChangeRatio),
                    _ => oldValue * (1 + .05 * popChangeRatio)
                };

                // Apply exponential smoothing to stabilize dynamics
                city.UpdateFactorIntensity(fv.Factor, oldValue * (1 - _smoothingAlpha) + newValue * _smoothingAlpha);
            }
        }
    }
}