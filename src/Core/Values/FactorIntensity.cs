namespace dotMigrata.Core.Values;

/// <summary>
/// Represents the intensity value for a specific factor in a city.
/// Each city has a FactorIntensity for every FactorDefinition in the world.
/// </summary>
/// <remarks>
///     <para>
///     Intensity values are stored as <see cref="UnitValue" /> for type safety
///     and optimal runtime performance during simulation.
///     </para>
///     <para>
///     The sign of a factor's effect is determined by <see cref="FactorDefinition.Type" />,
///     not by the intensity value.
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
    /// </summary>
    public required UnitValue Value { get; init; }

    /// <summary>
    /// Normalizes the intensity value using the factor's normalization rules.
    /// </summary>
    /// <returns>Normalized value.</returns>
    internal UnitValue Normalize() => Definition.Normalize(Value);
}