namespace dotMigrata.Core.Values;

/// <summary>
/// Represents the intensity value for a specific factor in a city.
/// Each city has a FactorIntensity for every FactorDefinition in the world.
/// </summary>
/// <remarks>
///     <para>
///     Intensity values are stored as <see cref="UnitValue" /> (0-1 range)
///     for type safety and optimal runtime performance during simulation.
///     </para>
///     <para>
///     The sign of a factor's effect is determined by <see cref="Type" />,
///     not by the intensity value itself.
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
    /// Values must be in the 0-1 range as a <see cref="UnitValue"/>.
    /// </summary>
    public required UnitValue Value { get; init; }
}