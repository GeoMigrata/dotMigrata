namespace dotGeoMigrata.Core.Values;

public sealed record FactorValue
{
    public required FactorDefinition Definition { get; init; }

    private double _intensity;

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