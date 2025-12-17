using dotMigrata.Core.Entities;

namespace dotMigrata.Core.Interfaces;

/// <summary>
/// Defines a strategy for determining city capacity constraints and enforcement.
/// </summary>
/// <remarks>
/// This interface enables different capacity models to be implemented:
/// - Hard caps (no migration above capacity)
/// - Soft caps with resistance (current default)
/// - Dynamic capacity based on resources
/// - Multi-tier capacity with different service levels
/// Supports future extensibility for complex urban planning scenarios.
/// </remarks>
public interface ICityCapacityStrategy
{
    /// <summary>
    /// Gets the name of this capacity strategy.
    /// </summary>
    /// <remarks>
    /// Used for logging, configuration, and snapshot metadata.
    /// Examples: "SoftCap", "HardCap", "DynamicCapacity", "ResourceBased"
    /// </remarks>
    string StrategyName { get; }

    /// <summary>
    /// Determines whether a city can accept additional population.
    /// </summary>
    /// <param name="city">The city to check.</param>
    /// <param name="additionalPopulation">The number of persons attempting to migrate in.</param>
    /// <returns>True if the city can accommodate the additional population; otherwise false.</returns>
    bool CanAccommodate(City city, int additionalPopulation);

    /// <summary>
    /// Calculates the resistance factor for migration to a city based on current population.
    /// </summary>
    /// <param name="city">The city to calculate resistance for.</param>
    /// <returns>A resistance factor between 0 (no resistance) and 1 (maximum resistance).</returns>
    /// <remarks>
    /// This is used by attraction calculators to reduce city attractiveness as it approaches capacity.
    /// The resistance curve shape depends on the specific strategy implementation.
    /// </remarks>
    double CalculateCapacityResistance(City city);

    /// <summary>
    /// Gets the effective capacity utilization ratio for a city.
    /// </summary>
    /// <param name="city">The city to check.</param>
    /// <returns>The capacity utilization as a ratio (0.0 = empty, 1.0 = at capacity, >1.0 = over capacity).</returns>
    /// <remarks>
    /// Returns 0.0 if the city has no explicit capacity limit.
    /// Used for metrics, monitoring, and capacity planning.
    /// </remarks>
    double GetCapacityUtilization(City city);
}