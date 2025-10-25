using dotGeoMigrata.Core.Entities;

namespace dotGeoMigrata.Logic.Feedback;


/// <summary>
/// Strategy interface for applying feedback to city factors after migration.
/// Implement this to create custom feedback mechanisms.
/// </summary>
public interface IFeedbackStrategy
{
    /// <summary>
    /// Applies feedback effects to a city's factors based on migration.
    /// </summary>
    /// <param name="city">The city to update.</param>
    /// <param name="previousPopulation">Population before migration.</param>
    /// <param name="currentPopulation">Population after migration.</param>
    void ApplyFeedback(City city, int previousPopulation, int currentPopulation);
}