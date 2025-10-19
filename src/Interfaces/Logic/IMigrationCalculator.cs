using dotGeoMigrata.Core.Domain.Entities;

namespace dotGeoMigrata.Interfaces.Logic;

/// <summary>
/// Interface for computing migration flows between cities.
/// The attraction function now receives (group, origin, target) so implementations
/// can compute distance-aware attraction values.
/// </summary>
public interface IMigrationCalculator
{
    /// <summary>
    /// Compute migration matrix for all population groups between cities.
    /// </summary>
    /// <param name="world">The world containing all cities and groups.</param>
    /// <param name="attractionFunc">
    /// Function to get attraction index for (group, origin, target).
    /// Signature: double Attraction(PopulationGroup group, City origin, City target)
    /// </param>
    /// <returns>
    /// A data structure representing migration flows:
    /// key = (origin city, target city, population group),
    /// value = expected migrants (double).
    /// </returns>
    IReadOnlyDictionary<(City origin, City target, PopulationGroup group), double>
        ComputeMigrations(World world, Func<PopulationGroup, City, City, double> attractionFunc);
}