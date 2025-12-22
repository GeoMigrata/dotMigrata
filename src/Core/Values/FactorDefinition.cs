using dotMigrata.Core.Enums;

namespace dotMigrata.Core.Values;

/// <summary>
/// Defines a factor's metadata including its direction and optional transformation.
/// Factors represent measurable characteristics of cities that influence migration decisions.
/// </summary>
/// <remarks>
/// <para>
/// Since city factor intensities are stored as <see cref="UnitValue"/> (0-1 range),
/// FactorDefinition only needs to define the factor's semantic meaning (type) and
/// optional transformation behavior.
/// </para>
/// <para>
/// Raw value normalization is no longer handled by FactorDefinition because all
/// factor intensities are already stored as normalized UnitValues.
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
    /// Determines whether high values attract or repel population.
    /// </summary>
    public required FactorType Type { get; init; }

    /// <summary>
    /// Gets or initializes the optional transformation function.
    /// When null, values are used as-is without transformation.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="UnitValuePromise.TransformFunc" /> delegate type
    /// for transformation behavior when needed in specific scenarios.
    /// </remarks>
    public UnitValuePromise.TransformFunc? TransformFunction { get; init; }
}