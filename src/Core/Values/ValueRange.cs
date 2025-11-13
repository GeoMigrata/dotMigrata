using dotGeoMigrata.Core.Enums;

namespace dotGeoMigrata.Core.Values;

/// <summary>
/// Represents a value range with minimum and maximum bounds.
/// Provides normalization capabilities with various transformation types.
/// </summary>
/// <param name="Min">Minimum value of the range.</param>
/// <param name="Max">Maximum value of the range.</param>
public readonly record struct ValueRange(double Min, double Max)
{
    /// <summary>
    /// Gets the size of the range (Max - Min).
    /// </summary>
    public double Size => Max - Min;

    /// <summary>
    /// Gets whether this range is valid (Min &lt; Max).
    /// </summary>
    public bool IsValid => Min < Max && !double.IsNaN(Min) && !double.IsNaN(Max) &&
                           !double.IsInfinity(Min) && !double.IsInfinity(Max);

    /// <summary>
    /// Normalizes a value to the 0-1 range using the specified transformation.
    /// </summary>
    /// <param name="value">The value to normalize.</param>
    /// <param name="transform">The transformation type to apply.</param>
    /// <returns>A normalized value between 0 and 1.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the range is invalid.</exception>
    public double Normalize(double value, TransformType? transform = null)
    {
        if (!IsValid)
            throw new InvalidOperationException("Cannot normalize with an invalid range.");

        var clamped = Math.Clamp(value, Min, Max);
        var range = Size;

        if (range == 0)
            return 0;

        return transform switch
        {
            TransformType.Linear => (clamped - Min) / range,
            TransformType.Log => NormalizeLogarithmic(clamped, range),
            TransformType.Sigmoid => NormalizeSigmoid(clamped, range),
            _ => (clamped - Min) / range
        };
    }

    /// <summary>
    /// Denormalizes a 0-1 value back to the original range.
    /// </summary>
    /// <param name="normalized">The normalized value (0-1).</param>
    /// <returns>The denormalized value in the original range.</returns>
    public double Denormalize(double normalized)
    {
        return Min + normalized * Size;
    }

    /// <summary>
    /// Clamps a value to this range.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <returns>The clamped value.</returns>
    public double Clamp(double value)
    {
        return Math.Clamp(value, Min, Max);
    }

    /// <summary>
    /// Checks if a value is within this range (inclusive).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is within the range; otherwise, false.</returns>
    public bool Contains(double value)
    {
        return value >= Min && value <= Max;
    }

    private double NormalizeLogarithmic(double clamped, double range)
    {
        const double delta = 1e-6; // small offset to avoid log(0)
        var numerator = Math.Log(clamped - Min + delta);
        var denominator = Math.Log(range + delta);
        return denominator != 0 ? numerator / denominator : 0.0;
    }

    private double NormalizeSigmoid(double clamped, double range)
    {
        const double steepness = 10.0;
        var linear = (clamped - Min) / range;
        var centered = (linear - 0.5) * steepness;
        return 1.0 / (1.0 + Math.Exp(-centered));
    }
}