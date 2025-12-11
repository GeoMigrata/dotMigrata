using dotMigrata.Core.Entities;
using dotMigrata.Logic.Models;

namespace dotMigrata.Logic.Interfaces;

/// <summary>
/// Defines methods for calculating migration decisions for individual persons.
/// </summary>
/// <remarks>
/// Implementations determine which persons migrate from their current cities to destination cities.
/// </remarks>
public interface IMigrationCalculator
{
    /// <summary>
    /// Calculates migration decision for a specific person considering all potential destination cities.
    /// </summary>
    /// <param name="person">The person considering migration.</param>
    /// <param name="destinationCities">The potential destination cities.</param>
    /// <param name="attractionResults">Pre-calculated attraction results for all cities for this person.</param>
    /// <returns>
    /// A <see cref="MigrationFlow" /> if the person decides to migrate;
    /// otherwise, <see langword="null" />.
    /// </returns>
    MigrationFlow? CalculateMigrationDecision(
        PersonBase person,
        IEnumerable<City> destinationCities,
        IDictionary<City, AttractionResult> attractionResults);

    /// <summary>
    /// Calculates all migration decisions for all persons in a world.
    /// </summary>
    /// <param name="world">The world containing cities and persons.</param>
    /// <param name="attractionCalculator">The attraction calculator to use for calculating attractions.</param>
    /// <returns>
    /// A collection of <see cref="MigrationFlow" /> instances for persons who decide to migrate.
    /// </returns>
    IEnumerable<MigrationFlow> CalculateAllMigrationFlows(
        World world,
        IAttractionCalculator attractionCalculator);
}