using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Values;
using dotGeoMigrata.Logic.Attraction;

namespace dotGeoMigrata.Logic.Interfaces;

/// <summary>
/// Defines the contract for calculating city attraction scores for population groups.
/// Implementations can use different algorithms and strategies for attraction calculation.
/// </summary>
public interface IAttractionCalculator
{
    /// <summary>
    /// Calculates the attraction score of a city for a specific population group definition.
    /// </summary>
    /// <param name="city">The city to evaluate.</param>
    /// <param name="groupDefinition">The population group definition.</param>
    /// <param name="world">The world context for factor definitions.</param>
    /// <returns>The attraction result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    AttractionResult CalculateAttraction(City city, PopulationGroupDefinition groupDefinition, World world);

    /// <summary>
    /// Calculates attraction scores for a population group definition across all cities.
    /// </summary>
    /// <param name="world">The world containing cities.</param>
    /// <param name="groupDefinition">The population group definition.</param>
    /// <returns>Collection of attraction results for all cities.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    IReadOnlyList<AttractionResult> CalculateAttractionForAllCities(World world,
        PopulationGroupDefinition groupDefinition);
}