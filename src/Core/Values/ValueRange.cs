namespace dotMigrata.Core.Values;

/// <summary>
/// Represents a value range with minimum and maximum bounds.
/// </summary>
/// <param name="Min">The minimum value of the range.</param>
/// <param name="Max">The maximum value of the range.</param>
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
}