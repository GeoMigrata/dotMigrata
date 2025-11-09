namespace dotGeoMigrata.Core.Values;

/// <summary>
/// Represents the intensity value for a specific factor in a city.
/// Each city has a FactorValue for every FactorDefinition in the world.
/// </summary>
public sealed record FactorValue
{
    private double _intensity;

    /// <summary>
    /// Gets or initializes the factor definition this value is for.
    /// </summary>
    public required FactorDefinition Definition { get; init; }

    /// <summary>
    /// Gets or sets the intensity value of the factor.
    /// This is the raw value that will be normalized during calculations.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is NaN or Infinity.</exception>
    public required double Intensity
    {
        get => _intensity;
        set
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentOutOfRangeException(nameof(Intensity), "Intensity must be a valid number.");
            _intensity = value;
        }
    }

    /// <summary>
    /// Normalizes the intensity value using the factor's normalization rules.
    /// </summary>
    /// <param name="fd">The factor definition containing normalization rules.</param>
    /// <returns>Normalized value between 0 and 1.</returns>
    internal double Normalize(FactorDefinition fd)
    {
        ArgumentNullException.ThrowIfNull(fd);
        return fd.Normalize(Intensity);
    }
}