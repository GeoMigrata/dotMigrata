namespace dotMigrata.Core.Values;

/// <summary>
/// Represents the intensity value for a specific factor in a city.
/// </summary>
/// <remarks>
/// <para>
/// Each city has a <see cref="FactorIntensity"/> for every <see cref="FactorDefinition"/> in the world.
/// Intensity values are stored as <see cref="UnitValue"/> (0-1 range) for type safety and optimal runtime performance.
/// </para>
/// <para>
/// The sign of a factor's effect is determined by <see cref="FactorDefinition.Type"/>, not by the intensity value itself.
/// </para>
/// </remarks>
public sealed class FactorIntensity
{
    /// <summary>
    /// Gets or initializes the factor definition this intensity represents.
    /// </summary>
    public required FactorDefinition Definition { get; init; }

    /// <summary>
    /// Gets or initializes the intensity value of the factor.
    /// </summary>
    /// <remarks>
    /// Values must be in the 0-1 range as a <see cref="UnitValue"/>.
    /// </remarks>
    public required UnitValue Value { get; init; }
}