using dotGeoMigrata.Core.Domain.Entities;

namespace dotGeoMigrata.Logic.Feedback;

/// <summary>
/// Updates city factors based on migration feedback mechanisms.
/// </summary>
public sealed class FeedbackCalculator
{
    private double _smoothingFactor;

    /// <summary>
    /// Smoothing factor for gradual updates (0-1).
    /// Higher values result in more gradual changes.
    /// </summary>
    public double SmoothingFactor
    {
        get => _smoothingFactor;
        init => _smoothingFactor = value is >= 0 and <= 1
            ? value
            : throw new ArgumentException("SmoothingFactor must be between 0 and 1.", nameof(value));
    }

    /// <summary>
    /// Applies feedback effects to city factors after migration.
    /// Updates factors based on population changes and migration patterns.
    /// </summary>
    /// <param name="city">The city to update.</param>
    /// <param name="previousPopulation">The population before migration.</param>
    /// <param name="currentPopulation">The population after migration.</param>
    /// <exception cref="ArgumentNullException">Thrown when city is null.</exception>
    public void ApplyFeedback(City city, int previousPopulation, int currentPopulation)
    {
        ArgumentNullException.ThrowIfNull(city);

        if (previousPopulation < 0)
            throw new ArgumentException("Previous population cannot be negative.", nameof(previousPopulation));

        if (currentPopulation < 0)
            throw new ArgumentException("Current population cannot be negative.", nameof(currentPopulation));

        if (previousPopulation == 0)
            return; // Avoid division by zero

        var populationChange = currentPopulation - previousPopulation;
        var populationRatio = (double)currentPopulation / previousPopulation;

        // Apply feedback effects based on population density changes
        UpdatePerCapitaResources(city, populationRatio);
        UpdateHousingAndCongestion(city, populationRatio);
        UpdateEconomicFactors(city, populationChange, currentPopulation);
    }

    /// <summary>
    /// Updates per-capita resource factors (e.g., public services, healthcare).
    /// These typically decrease with population growth.
    /// </summary>
    private void UpdatePerCapitaResources(City city, double populationRatio)
    {
        // As population increases, per-capita resources decrease
        // This is a simplified model; actual implementation would identify specific factors

        // Example: If a city has factors that represent per-capita resources,
        // they would be adjusted here based on population ratio
        // For now, this is a placeholder for the feedback mechanism
    }

    /// <summary>
    /// Updates housing costs and congestion factors.
    /// These typically increase with population growth.
    /// </summary>
    private void UpdateHousingAndCongestion(City city, double populationRatio)
    {
        // Higher population density typically leads to:
        // - Higher housing costs
        // - More congestion/pollution
        // - Reduced quality of life factors

        // This would be implemented with specific factor updates
        // based on the city's defined factors
    }

    /// <summary>
    /// Updates economic factors based on population changes.
    /// </summary>
    private void UpdateEconomicFactors(City city, int populationChange, int currentPopulation)
    {
        // Population growth can affect:
        // - Economic output (more workers)
        // - Innovation (larger talent pool)
        // - Infrastructure strain

        // These effects would be modeled based on the specific factors
        // defined in the simulation
    }

    /// <summary>
    /// Applies a smoothed update to a factor value.
    /// </summary>
    /// <param name="currentValue">The current factor value.</param>
    /// <param name="targetValue">The target factor value.</param>
    /// <returns>The smoothed new value.</returns>
    private double SmoothUpdate(double currentValue, double targetValue)
    {
        return currentValue + SmoothingFactor * (targetValue - currentValue);
    }
}