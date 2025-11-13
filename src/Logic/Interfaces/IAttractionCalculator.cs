using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Values;
using dotGeoMigrata.Logic.Models;

namespace dotGeoMigrata.Logic.Interfaces;

/// <summary>
/// Interface for calculating city attraction for individual persons.
/// Implementations determine how attractive a city is to a specific person based on city factors and person sensitivities.
/// </summary>
public interface IAttractionCalculator
{
    /// <summary>
    /// Calculates the attraction score for a specific person at a city.
    /// </summary>
    /// <param name="city">The city to evaluate.</param>
    /// <param name="person">The person for whom to calculate attraction.</param>
    /// <param name="originCity">
    /// The city where the person currently resides (optional, for distance and comparison calculations).
    /// </param>
    /// <returns>An attraction result containing the calculated attraction score and any additional metadata.</returns>
    AttractionResult CalculateAttraction(City city, Person person, City? originCity = null);

    /// <summary>
    /// Calculates attraction scores for a person across all cities.
    /// </summary>
    /// <param name="cities">The cities to evaluate.</param>
    /// <param name="person">The person for whom to calculate attraction.</param>
    /// <param name="originCity">The city where the person currently resides (optional).</param>
    /// <returns>A dictionary mapping cities to their attraction results.</returns>
    IDictionary<City, AttractionResult> CalculateAttractionForAllCities(
        IEnumerable<City> cities,
        Person person,
        City? originCity = null);
}