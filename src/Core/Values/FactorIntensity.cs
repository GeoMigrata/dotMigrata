namespace dotMigrata.Core.Values;

/// <summary>
/// Represents the intensity value for a specific factor in a city.
/// Each city has a FactorIntensity for every FactorDefinition in the world.
/// </summary>
/// <remarks>
///     <para>
///     intensity values are stored directly as <see cref="UnitValue" /> for optimal
///     runtime performance. This ensures type safety and eliminates overhead during simulation.
///     </para>
///     <para>
///     Intensity values are always in the [0, 1] range. The sign of the factor's effect
///     is determined by the <see cref="FactorDefinition.Type" /> property, not by the intensity value.
///     </para>
/// </remarks>
public sealed class FactorIntensity
{
    /// <summary>
    /// Gets or initializes the factor definition this intensity is for.
    /// </summary>
    public required FactorDefinition Definition { get; init; }

    /// <summary>
    /// Gets or initializes the intensity value of the factor.
    /// Value is guaranteed to be in the [0, 1] range.
    /// </summary>
    public required UnitValue Value { get; init; }

    /// <summary>
    /// Normalizes the intensity value using the factor's normalization rules.
    /// </summary>
    /// <returns>Normalized value between 0 and 1.</returns>
    internal UnitValue Normalize()
    {
        return Definition.Normalize(Value);
    }
}