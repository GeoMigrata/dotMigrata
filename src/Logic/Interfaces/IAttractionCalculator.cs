using dotMigrata.Core.Entities;
using dotMigrata.Logic.Models;

namespace dotMigrata.Logic.Interfaces;

/// <summary>
/// Defines methods for calculating city attraction for individual persons.
/// </summary>
/// <remarks>
/// Implementations determine how attractive a city is to a specific person
/// based on city factors and person sensitivities.
/// </remarks>
public interface IAttractionCalculator
{
    /// <summary>
    /// Calculates the attraction score for a specific person at a city.
    /// </summary>
    /// <param name="city">The city to evaluate.</param>
    /// <param name="person">The person for whom to calculate attraction.</param>
    /// <param name="originCity">
    /// The city where the person currently resides, or <see langword="null" /> if not applicable.
    /// Used for distance and comparison calculations.
    /// </param>
    /// <returns>
    /// An <see cref="AttractionResult" /> containing the calculated attraction score and any additional metadata.
    /// </returns>
    AttractionResult CalculateAttraction(City city, PersonBase person, City? originCity = null);

    /// <summary>
    /// Calculates attraction scores for a person across all cities.
    /// </summary>
    /// <param name="cities">The cities to evaluate.</param>
    /// <param name="person">The person for whom to calculate attraction.</param>
    /// <param name="originCity">
    /// The city where the person currently resides, or <see langword="null" /> if not applicable.
    /// </param>
    /// <returns>
    /// A dictionary mapping each city to its <see cref="AttractionResult" />.
    /// </returns>
    IDictionary<City, AttractionResult> CalculateAttractionForAllCities(
        IEnumerable<City> cities,
        PersonBase person,
        City? originCity = null);
}