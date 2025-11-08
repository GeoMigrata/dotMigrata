using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Values;
using dotGeoMigrata.Logic.Models;

namespace dotGeoMigrata.Logic.Interfaces;

/// <summary>
/// Interface for calculating migration flows between cities.
/// Implementations determine how many people from each group migrate from origin cities to destination cities.
/// </summary>
public interface IMigrationCalculator
{
    /// <summary>
    /// Calculates migration flows for a specific population group from an origin city to all potential destination cities.
    /// </summary>
    /// <param name="originCity">The city where the population group currently resides.</param>
    /// <param name="destinationCities">The potential destination cities.</param>
    /// <param name="group">The population group definition.</param>
    /// <param name="currentPopulation">Current population count of the group in the origin city.</param>
    /// <param name="attractionResults">Pre-calculated attraction results for all cities.</param>
    /// <returns>A collection of migration flow results indicating how many people migrate to each destination.</returns>
    IEnumerable<MigrationFlow> CalculateMigrationFlows(
        City originCity,
        IEnumerable<City> destinationCities,
        GroupDefinition group,
        int currentPopulation,
        IDictionary<City, AttractionResult> attractionResults);

    /// <summary>
    /// Calculates all migration flows across all cities for all population groups in a world.
    /// </summary>
    /// <param name="world">The world containing cities and population groups.</param>
    /// <param name="attractionCalculator">The attraction calculator to use for pre-calculating attractions.</param>
    /// <returns>A collection of all migration flows.</returns>
    IEnumerable<MigrationFlow> CalculateAllMigrationFlows(
        World world,
        IAttractionCalculator attractionCalculator);
}