using dotMigrata.Core.Enums;

namespace dotMigrata.Core.Values;

/// <summary>
/// Represents a value range with minimum and maximum bounds.
/// </summary>
/// <param name="Min">The minimum value of the range.</param>
/// <param name="Max">The maximum value of the range.</param>
/// <remarks>
/// This type provides normalization capabilities with various transformation types.
/// Use the <see cref="IsValid" /> property to check if the range is valid before use.
/// </remarks>
public readonly record struct ValueRange(double Min, double Max)
{
    /// <summary>
    /// Gets the size of the range (Max - Min).
    /// </summary>
    public double Size => Max - Min;

    /// <summary>
    /// Gets a value indicating whether this range is valid.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if Min is less than Max and neither value is NaN or Infinity;
    /// otherwise, <see langword="false" />.
    /// </value>
    public bool IsValid => Min < Max
                           && !double.IsNaN(Min)
                           && !double.IsNaN(Max)
                           && !double.IsInfinity(Min)
                           && !double.IsInfinity(Max);

    /// <summary>
    /// Determines whether the specified double value is finite (not NaN or Infinity).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>
    /// <see langword="true" /> if the value is a valid finite number; otherwise, <see langword="false" />.
    /// </returns>
    public static bool IsFinite(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }

    /// <summary>
    /// Normalizes the specified value to the 0-1 range using the specified transformation.
    /// </summary>
    /// <param name="value">The value to normalize.</param>
    /// <param name="transform">The transformation type to apply, or <see langword="null" /> for linear transformation.</param>
    /// <returns>A normalized value between 0 and 1.</returns>
    /// <exception cref="InvalidOperationException">Thrown when this range is invalid.</exception>
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
    /// <param name="normalized">The normalized value in the range 0-1.</param>
    /// <returns>The denormalized value within this range.</returns>
    public double Denormalize(double normalized)
    {
        return Min + normalized * Size;
    }

    /// <summary>
    /// Clamps the specified value to this range.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <returns>The value clamped to this range.</returns>
    public double Clamp(double value)
    {
        return Math.Clamp(value, Min, Max);
    }

    /// <summary>
    /// Determines whether the specified value is within this range (inclusive).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>
    /// <see langword="true" /> if the value is within the range; otherwise, <see langword="false" />.
    /// </returns>
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