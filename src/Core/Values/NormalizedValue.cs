namespace dotMigrata.Core.Values;

/// <summary>
/// Represents a normalized value in the 0-1 range.
/// Used for ratios, percentages, and normalized intensities.
/// Implements as a readonly record struct for zero-allocation overhead.
/// </summary>
public readonly record struct NormalizedValue : IValue<double>, IRangedValue, INormalizable
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
    /// A normalized value of zero (0.0).
    /// </summary>
    public static readonly NormalizedValue Zero = new(0.0);

    /// <summary>
    /// A normalized value of one (1.0).
    /// </summary>
    public static readonly NormalizedValue One = new(1.0);

    /// <summary>
    /// A normalized value of half (0.5).
    /// </summary>
    public static readonly NormalizedValue Half = new(0.5);

    /// <summary>
    /// Initializes a new instance of the NormalizedValue struct.
    /// </summary>
    /// <param name="value">The value to normalize. Must be between 0 and 1.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is outside the 0-1 range or is NaN/Infinity.</exception>
    private NormalizedValue(double value)
    {
        if (!IsValidValue(value))
            throw new ArgumentOutOfRangeException(nameof(value),
                $"Value must be a valid number between {MinValueConst} and {MaxValueConst}.");

        Value = value;
    }

    /// <summary>
    /// Checks if a value is valid for a NormalizedValue (0-1, not NaN, not Infinity).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is valid; otherwise, false.</returns>
    public static bool IsValidValue(double value) =>
        !double.IsNaN(value) && !double.IsInfinity(value) && ValidRange.Contains(value);

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
    public bool IsValid() => IsValidValue(Value);

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
    /// Tries to create a NormalizedValue from a ratio (0-1) without throwing an exception.
    /// </summary>
    /// <param name="ratio">The ratio value between 0 and 1.</param>
    /// <param name="result">When this method returns, contains the NormalizedValue if successful; otherwise, default.</param>
    /// <returns>True if the value was created successfully; otherwise, false.</returns>
    public static bool TryFromRatio(double ratio, out NormalizedValue result)
    {
        if (IsValidValue(ratio))
        {
            result = new NormalizedValue(ratio);
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Creates a NormalizedValue from a ratio, clamping to the valid range.
    /// </summary>
    /// <param name="ratio">The ratio value to clamp.</param>
    /// <returns>A new NormalizedValue clamped to 0-1.</returns>
    public static NormalizedValue FromRatioClamped(double ratio)
    {
        if (double.IsNaN(ratio) || double.IsInfinity(ratio))
            return Zero;

        return new NormalizedValue(ValidRange.Clamp(ratio));
    }

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
    /// Tries to create a NormalizedValue from a percentage (0-100) without throwing an exception.
    /// </summary>
    /// <param name="percentage">The percentage value between 0 and 100.</param>
    /// <param name="result">When this method returns, contains the NormalizedValue if successful; otherwise, default.</param>
    /// <returns>True if the value was created successfully; otherwise, false.</returns>
    public static bool TryFromPercentage(double percentage, out NormalizedValue result)
    {
        if (percentage is >= 0 and <= 100 && !double.IsNaN(percentage) && !double.IsInfinity(percentage))
        {
            result = new NormalizedValue(percentage / 100.0);
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Returns itself since NormalizedValue is already normalized.
    /// </summary>
    /// <returns>This value.</returns>
    public NormalizedValue Normalize() => this;

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

    /// <summary>
    /// Multiplies a NormalizedValue by a scalar, clamping the result to 0-1.
    /// </summary>
    /// <param name="left">The NormalizedValue.</param>
    /// <param name="right">The scalar multiplier.</param>
    /// <returns>A new NormalizedValue with the clamped result.</returns>
    public static NormalizedValue operator *(NormalizedValue left, double right) =>
        FromRatioClamped(left.Value * right);

    /// <summary>
    /// Multiplies a scalar by a NormalizedValue, clamping the result to 0-1.
    /// </summary>
    /// <param name="left">The scalar multiplier.</param>
    /// <param name="right">The NormalizedValue.</param>
    /// <returns>A new NormalizedValue with the clamped result.</returns>
    public static NormalizedValue operator *(double left, NormalizedValue right) =>
        FromRatioClamped(left * right.Value);

    /// <summary>
    /// Adds two NormalizedValues, clamping the result to 0-1.
    /// </summary>
    /// <param name="left">The first NormalizedValue.</param>
    /// <param name="right">The second NormalizedValue.</param>
    /// <returns>A new NormalizedValue with the clamped result.</returns>
    public static NormalizedValue operator +(NormalizedValue left, NormalizedValue right) =>
        FromRatioClamped(left.Value + right.Value);

    /// <summary>
    /// Subtracts one NormalizedValue from another, clamping the result to 0-1.
    /// </summary>
    /// <param name="left">The first NormalizedValue.</param>
    /// <param name="right">The second NormalizedValue.</param>
    /// <returns>A new NormalizedValue with the clamped result.</returns>
    public static NormalizedValue operator -(NormalizedValue left, NormalizedValue right) =>
        FromRatioClamped(left.Value - right.Value);
}