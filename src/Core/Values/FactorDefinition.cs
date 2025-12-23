using dotMigrata.Core.Enums;

namespace dotMigrata.Core.Values;

/// <summary>
/// Defines a migration factor (e.g., income, pollution, education) that influences person migration decisions.
/// </summary>
/// <remarks>
/// <para>
/// Since city factor intensities are stored as <see cref="UnitValue"/> (0-1 range),
/// <see cref="FactorDefinition"/> only needs to define the factor's semantic meaning (<see cref="Type"/>) and
/// optional transformation behavior (<see cref="TransformFunction"/>).
/// </para>
/// <para>
/// Raw value normalization is no longer handled by <see cref="FactorDefinition"/> because all
/// factor intensities are already stored as normalized <see cref="UnitValue"/> instances.
/// </para>
/// <para>
/// Factor definitions are reference-equal: use the same <see cref="FactorDefinition"/> instance
/// across cities, persons, and factor intensities for correct behavior.
/// </para>
/// </remarks>
public record FactorDefinition
{
    /// <summary>
    /// Gets or initializes the display name of the factor.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets or initializes the factor type (Positive or Negative).
    /// </summary>
    /// <remarks>
    /// Determines whether high values attract or repel population.
    /// See <see cref="FactorType"/> for available options.
    /// </remarks>
    public required FactorType Type { get; init; }

    /// <summary>
    /// Gets or initializes the optional transformation function.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, values are used as-is without transformation.
    /// Uses <see cref="UnitValuePromise.TransformFunc"/> delegate type for transformation behavior.
    /// </remarks>
    public UnitValuePromise.TransformFunc? TransformFunction { get; init; }
}