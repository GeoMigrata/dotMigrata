using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Values;
using dotGeoMigrata.Logic.Models;

namespace dotGeoMigrata.Logic.Interfaces;

/// <summary>
/// Interface for calculating migration decisions for individual persons.
/// Implementations determine which persons migrate from their current cities to destination cities.
/// </summary>
public interface IMigrationCalculator
{
    /// <summary>
    /// Calculates migration decision for a specific person considering all potential destination cities.
    /// </summary>
    /// <param name="person">The person considering migration.</param>
    /// <param name="destinationCities">The potential destination cities.</param>
    /// <param name="attractionResults">Pre-calculated attraction results for all cities for this person.</param>
    /// <returns>A migration flow result if the person decides to migrate, null otherwise.</returns>
    MigrationFlow? CalculateMigrationDecision(
        Person person,
        IEnumerable<City> destinationCities,
        IDictionary<City, AttractionResult> attractionResults);

    /// <summary>
    /// Calculates all migration decisions for all persons in a world.
    /// </summary>
    /// <param name="world">The world containing cities and persons.</param>
    /// <param name="attractionCalculator">The attraction calculator to use for calculating attractions.</param>
    /// <returns>A collection of all migration flows for persons who decide to migrate.</returns>
    IEnumerable<MigrationFlow> CalculateAllMigrationFlows(
        World world,
        IAttractionCalculator attractionCalculator);
}