using dotGeoMigrata.Core.Domain.Entities;

namespace dotGeoMigrata.Interfaces.Logic;

/// <summary>
/// Interface for updating city factors after population changes.
/// </summary>
public interface IFeedbackUpdater
{
    /// <summary>
    /// Update factor values of all cities based on migration results.
    /// </summary>
    /// <param name="world">The world after migration.</param>
    /// <param name="migrationFlows">
    /// The migration flow data produced by IMigrationCalculator.
    /// </param>
    void UpdateFactors(World world,
        IReadOnlyDictionary<(City origin, City target, PopulationGroup group), double> migrationFlows);
}