namespace dotMigrata.Core.Values;

/// <summary>
/// Represents a factor sensitivity value with a configurable range.
/// Default range is -10 to +10, representing negative to positive sensitivity.
/// Implements as a readonly record struct for zero-allocation overhead.
/// </summary>
public readonly record struct SensitivityValue : IRangedValue, INormalizable
{
    private readonly ValueRange _range;
    private readonly double _value;

    /// <summary>
    /// Default minimum sensitivity value.
    /// </summary>
    public const double DefaultMinValue = -10.0;

    /// <summary>
    /// Default maximum sensitivity value.
    /// </summary>
    public const double DefaultMaxValue = 10.0;

    private static readonly ValueRange DefaultRange = new(DefaultMinValue, DefaultMaxValue);

    /// <summary>
    /// Initializes a new instance of the SensitivityValue struct.
    /// </summary>
    /// <param name="value">The sensitivity value.</param>
    /// <param name="range">The valid range for this sensitivity value.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is outside the specified range or is NaN/Infinity.</exception>
    private SensitivityValue(double value, ValueRange range)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be a valid number.");

        if (!range.IsValid)
            throw new ArgumentException("Range must be valid (Min < Max).");

        if (!range.Contains(value))
            throw new ArgumentOutOfRangeException(nameof(value),
                $"Value must be between {range.Min} and {range.Max}.");

        _value = value;
        _range = range;
    }

    /// <summary>
    /// Gets the raw sensitivity value.
    /// </summary>
    public double Value => _value;

    /// <summary>
    /// Gets the minimum allowed value for this sensitivity.
    /// </summary>
    public double MinValue => _range.Min;

    /// <summary>
    /// Gets the maximum allowed value for this sensitivity.
    /// </summary>
    public double MaxValue => _range.Max;

    /// <summary>
    /// Gets the valid range for this sensitivity.
    /// </summary>
    public ValueRange Range => _range;

    /// <summary>
    /// Validates that this value is within its range.
    /// </summary>
    /// <returns>True if the value is valid; otherwise, false.</returns>
    public bool IsValid() => _range.Contains(_value) &&
                             !double.IsNaN(_value) &&
                             !double.IsInfinity(_value);

    /// <summary>
    /// Checks if a value is within this sensitivity's range.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is within range; otherwise, false.</returns>
    public bool Contains(double value) => _range.Contains(value);

    /// <summary>
    /// Clamps a value to this sensitivity's range.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <returns>The clamped value.</returns>
    public double Clamp(double value) => _range.Clamp(value);

    /// <summary>
    /// Creates a SensitivityValue from a raw value using the default range (-10 to +10).
    /// </summary>
    /// <param name="value">The sensitivity value.</param>
    /// <returns>A new SensitivityValue.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is outside the default range.</exception>
    public static SensitivityValue FromRaw(double value) => new(value, DefaultRange);

    /// <summary>
    /// Creates a SensitivityValue from a raw value with a custom range.
    /// </summary>
    /// <param name="value">The sensitivity value.</param>
    /// <param name="minValue">The minimum allowed value.</param>
    /// <param name="maxValue">The maximum allowed value.</param>
    /// <returns>A new SensitivityValue.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is outside the specified range.</exception>
    public static SensitivityValue FromRaw(double value, double minValue, double maxValue) =>
        new(value, new ValueRange(minValue, maxValue));

    /// <summary>
    /// Creates a neutral sensitivity (0.0).
    /// </summary>
    /// <returns>A new SensitivityValue with value 0.</returns>
    public static SensitivityValue Neutral()
    {
        return new SensitivityValue(0.0, DefaultRange);
    }

    /// <summary>
    /// Normalizes the sensitivity value to 0-1 range based on its min/max bounds.
    /// </summary>
    /// <returns>A normalized value between 0 and 1.</returns>
    public NormalizedValue Normalize()
    {
        var normalized = _range.Normalize(_value);
        return NormalizedValue.FromRatio(normalized);
    }

    /// <summary>
    /// Returns a string representation of the value.
    /// </summary>
    public override string ToString() => $"{_value:F2}";
}