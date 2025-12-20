using dotMigrata.Core.Entities;
using dotMigrata.Core.Values;

namespace dotMigrata.Logic.Models;

/// <summary>
/// Represents the result of an attraction calculation for a city and population group.
/// </summary>
/// <remarks>
/// All attraction and resistance values are type-safe <see cref="UnitValue"/> instances,
/// guaranteeing they remain in the [0, 1] range. The <see cref="AdjustedAttraction"/> property
/// is computed from the base values for consistency.
/// </remarks>
public sealed record AttractionResult
{
    /// <summary>
    /// Gets the city being evaluated.
    /// </summary>
    public required City City { get; init; }

    /// <summary>
    /// Gets the base attraction score (0-1) calculated from factor sensitivities.
    /// </summary>
    public required UnitValue BaseAttraction { get; init; }

    /// <summary>
    /// Gets the capacity resistance factor (0-1) applied to the attraction.
    /// Higher values indicate more resistance due to overcrowding.
    /// </summary>
    public required UnitValue CapacityResistance { get; init; }

    /// <summary>
    /// Gets the distance resistance factor (0-1) applied to the attraction.
    /// Lower values indicate greater distance decay.
    /// </summary>
    public required UnitValue DistanceResistance { get; init; }

    /// <summary>
    /// Gets the adjusted attraction score after applying capacity and distance resistance.
    /// Computed as: BaseAttraction × (1 - CapacityResistance) × DistanceResistance
    /// </summary>
    public UnitValue AdjustedAttraction =>
        BaseAttraction * ((UnitValue.One - CapacityResistance) * DistanceResistance);
}