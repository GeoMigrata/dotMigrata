using dotMigrata.Core.Entities;

namespace dotMigrata.Logic.Models;

/// <summary>
/// Represents the result of an attraction calculation for a city and population group.
/// </summary>
public sealed record AttractionResult
{
    /// <summary>
    /// Gets the city being evaluated.
    /// </summary>
    public required City City { get; init; }

    /// <summary>
    /// Gets the base attraction score (0-1) calculated from factor sensitivities.
    /// </summary>
    public required double BaseAttraction { get; init; }

    /// <summary>
    /// Gets the adjusted attraction score after applying capacity and distance resistance.
    /// </summary>
    public required double AdjustedAttraction { get; init; }

    /// <summary>
    /// Gets the capacity resistance factor (0-1) applied to the attraction.
    /// Higher values indicate more resistance due to overcrowding.
    /// </summary>
    public double CapacityResistance { get; init; }

    /// <summary>
    /// Gets the distance resistance factor (0-1) applied to the attraction.
    /// Lower values indicate greater distance decay.
    /// </summary>
    public double DistanceResistance { get; init; }
}