namespace dotMigrata.Core.Values;

/// <summary>
/// Represents a normalized value in the 0-1 range.
/// Used for ratios, percentages, and normalized intensities.
/// Implements as a readonly record struct for zero-allocation overhead.
/// </summary>
public readonly record struct NormalizedValue : IValue<double>, IRangedValue
{
    private static readonly ValueRange ValidRange = new(0.0, 1.0);

    /// <summary>
    /// Minimum allowed value (0.0).
    /// </summary>
    public const double MinValueConst = 0.0;

    /// <summary>
    /// Maximum allowed value (1.0).
    /// </summary>
    public const double MaxValueConst = 1.0;

    /// <summary>
    /// Initializes a new instance of the NormalizedValue struct.
    /// </summary>
    /// <param name="value">The value to normalize. Must be between 0 and 1.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is outside the 0-1 range or is NaN/Infinity.</exception>
    private NormalizedValue(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be a valid number.");

        if (!ValidRange.Contains(value))
            throw new ArgumentOutOfRangeException(nameof(value),
                $"Value must be between {MinValueConst} and {MaxValueConst}.");

        Value = value;
    }

    /// <summary>
    /// Gets the raw double value (0-1).
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Gets the minimum allowed value for normalized values.
    /// </summary>
    public double MinValue => MinValueConst;

    /// <summary>
    /// Gets the maximum allowed value for normalized values.
    /// </summary>
    public double MaxValue => MaxValueConst;

    /// <summary>
    /// Gets the valid range for normalized values (0-1).
    /// </summary>
    public ValueRange Range => ValidRange;

    /// <summary>
    /// Validates that this value is within the 0-1 range.
    /// </summary>
    /// <returns>True if the value is valid; otherwise, false.</returns>
    public bool IsValid() => ValidRange.Contains(Value) &&
                             !double.IsNaN(Value) &&
                             !double.IsInfinity(Value);

    /// <summary>
    /// Checks if a value is within the valid normalized range (0-1).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is within range; otherwise, false.</returns>
    public bool Contains(double value) => ValidRange.Contains(value);

    /// <summary>
    /// Clamps a value to the normalized range (0-1).
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <returns>The clamped value.</returns>
    public double Clamp(double value) => ValidRange.Clamp(value);

    /// <summary>
    /// Creates a NormalizedValue from a ratio (0-1).
    /// </summary>
    /// <param name="ratio">The ratio value between 0 and 1.</param>
    /// <returns>A new NormalizedValue.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when ratio is outside the 0-1 range.</exception>
    public static NormalizedValue FromRatio(double ratio) => new NormalizedValue(ratio);

    /// <summary>
    /// Creates a NormalizedValue from a percentage (0-100).
    /// </summary>
    /// <param name="percentage">The percentage value between 0 and 100.</param>
    /// <returns>A new NormalizedValue.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when percentage is outside the 0-100 range.</exception>
    public static NormalizedValue FromPercentage(double percentage)
    {
        if (percentage is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(percentage),
                "Percentage must be between 0 and 100.");

        return new NormalizedValue(percentage / 100.0);
    }

    /// <summary>
    /// Gets the value as a percentage (0-100).
    /// </summary>
    /// <returns>The value as a percentage.</returns>
    public double ToPercentage() => Value * 100.0;

    /// <summary>
    /// Returns a string representation of the value.
    /// </summary>
    public override string ToString() => $"{Value:F2}";
    
    /// <summary>
    /// Implicitly converts a NormalizedValue to a double.
    /// </summary>
    /// <param name="value">The NormalizedValue to convert.</param>
    public static implicit operator double(NormalizedValue value) => value.Value;

    /// <summary>
    /// Explicitly converts a double to a NormalizedValue.
    /// </summary>
    /// <param name="value">The double value to convert (must be between 0 and 1).</param>
    public static explicit operator NormalizedValue(double value) => FromRatio(value);
}