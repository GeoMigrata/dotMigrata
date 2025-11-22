namespace dotMigrata.Core.Values;

/// <summary>
/// Represents the intensity value for a specific factor in a city.
/// Each city has a FactorValue for every FactorDefinition in the world.
/// </summary>
public sealed record FactorValue
{
    /// <summary>
    /// Gets or initializes the factor definition this value is for.
    /// </summary>
    public required FactorDefinition Definition { get; init; }

    /// <summary>
    /// Gets or sets the intensity value of the factor.
    /// This is the typed value that will be normalized during calculations.
    /// Must be non-negative (>= 0).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is NaN, Infinity, or negative.</exception>
    public required IntensityValue Intensity { get; set; }

    /// <summary>
    /// Normalizes the intensity value using the factor's normalization rules.
    /// </summary>
    /// <param name="fd">The factor definition containing normalization rules.</param>
    /// <returns>Normalized value between 0 and 1.</returns>
    internal double Normalize(FactorDefinition fd)
    {
        ArgumentNullException.ThrowIfNull(fd);
        return fd.Normalize(Intensity.Value);
    }
}