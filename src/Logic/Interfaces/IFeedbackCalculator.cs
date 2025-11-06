using dotGeoMigrata.Core.Entities;

namespace dotGeoMigrata.Logic.Interfaces;

/// <summary>
/// Defines the contract for applying feedback effects to city factors after migration.
/// Implementations can use different feedback mechanisms and rules.
/// </summary>
public interface IFeedbackCalculator
{
    /// <summary>
    /// Gets or sets the smoothing factor for gradual updates (0-1).
    /// Higher values result in more gradual changes.
    /// </summary>
    double SmoothingFactor { get; set; }

    /// <summary>
    /// Applies feedback effects to city factors after migration.
    /// Updates factors based on population changes and migration patterns.
    /// </summary>
    /// <param name="city">The city to update.</param>
    /// <param name="previousPopulation">The population before migration.</param>
    /// <param name="currentPopulation">The population after migration.</param>
    /// <exception cref="ArgumentNullException">Thrown when city is null.</exception>
    /// <exception cref="ArgumentException">Thrown when population values are invalid.</exception>
    void ApplyFeedback(City city, int previousPopulation, int currentPopulation);
}