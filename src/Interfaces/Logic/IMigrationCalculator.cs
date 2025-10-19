using dotGeoMigrata.Core.Domain.Entities;

namespace dotGeoMigrata.Interfaces.Logic;

/// <summary>
/// Interface for computing migration flows between cities.
/// </summary>
internal interface IMigrationCalculator
{
    /// <summary>
    /// Compute migration matrix for all population groups between cities.
    /// </summary>
    /// <param name="world">The world containing all cities and groups.</param>
    /// <param name="attractionFunc">Function to get precomputed attraction index for (city, group).</param>
    /// <returns>
    /// A data structure representing migration flows: 
    /// e.g. Dictionary&lt;(City origin, City target, PopulationGroup group), double&gt;
    /// </returns>
    IReadOnlyDictionary<(City origin, City target, PopulationGroup group), double>
        ComputeMigrations(World world, Func<City, PopulationGroup, double> attractionFunc);
}