using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Values;
using dotGeoMigrata.Logic.Models;

namespace dotGeoMigrata.Logic.Interfaces;

/// <summary>
/// Interface for calculating city attraction for population groups.
/// Implementations determine how attractive a city is to a specific group based on city factors and group sensitivities.
/// </summary>
public interface IAttractionCalculator
{
    /// <summary>
    /// Calculates the attraction score for a specific population group at a city.
    /// </summary>
    /// <param name="city">The city to evaluate.</param>
    /// <param name="group">The population group definition.</param>
    /// <param name="originCity">
    /// The city where the group currently resides (optional, for distance and comparison
    /// calculations).
    /// </param>
    /// <returns>An attraction result containing the calculated attraction score and any additional metadata.</returns>
    AttractionResult CalculateAttraction(City city, GroupDefinition group, City? originCity = null);

    /// <summary>
    /// Calculates attraction scores for a population group across all cities.
    /// </summary>
    /// <param name="cities">The cities to evaluate.</param>
    /// <param name="group">The population group definition.</param>
    /// <param name="originCity">The city where the group currently resides (optional).</param>
    /// <returns>A dictionary mapping cities to their attraction results.</returns>
    IDictionary<City, AttractionResult> CalculateAttractionForAllCities(
        IEnumerable<City> cities,
        GroupDefinition group,
        City? originCity = null);
}