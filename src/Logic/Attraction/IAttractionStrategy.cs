using dotGeoMigrata.Core.Entities;

namespace dotGeoMigrata.Logic.Attraction;

/// <summary>
/// Strategy interface for calculating city attraction.
/// Implement this to create custom attraction calculation strategies.
/// </summary>
public interface IAttractionStrategy
{
    /// <summary>
    /// Calculates the attraction score for a city and population group.
    /// </summary>
    /// <param name="city">The city to evaluate.</param>
    /// <param name="group">The population group.</param>
    /// <param name="world">The world context.</param>
    /// <returns>The calculated attraction score.</returns>
    double Calculate(City city, PopulationGroup group, World world);
}