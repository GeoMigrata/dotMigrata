namespace dotMigrata.Core.Values;

/// <summary>
/// Represents a factor sensitivity value with a configurable range.
/// Default range is -10 to +10, representing negative to positive sensitivity.
/// Implements as a readonly record struct for zero-allocation overhead.
/// </summary>
public readonly record struct SensitivityValue : IRangedValue, INormalizable
{
    private readonly ValueRange _range;

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
    /// A neutral sensitivity value (0.0) using the default range.
    /// </summary>
    public static readonly SensitivityValue Zero = new(0.0, DefaultRange);

    /// <summary>
    /// Initializes a new instance of the SensitivityValue struct.
    /// </summary>
    /// <param name="value">The sensitivity value.</param>
    /// <param name="range">The valid range for this sensitivity value.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is outside the specified range or is NaN/Infinity.</exception>
    private SensitivityValue(double value, ValueRange range)
    {
        if (!range.IsValid)
            throw new ArgumentException("Range must be valid (Min < Max).");

        if (!IsValidValue(value, range))
            throw new ArgumentOutOfRangeException(nameof(value),
                $"Value must be a valid number between {range.Min} and {range.Max}.");

        Value = value;
        _range = range;
    }

    /// <summary>
    /// Checks if a value is valid for a SensitivityValue within the specified range.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="range">The valid range.</param>
    /// <returns>True if the value is valid; otherwise, false.</returns>
    public static bool IsValidValue(double value, ValueRange range) =>
        !double.IsNaN(value) && !double.IsInfinity(value) && range.Contains(value);

    /// <summary>
    /// Checks if a value is valid for a SensitivityValue using the default range.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is valid; otherwise, false.</returns>
    public static bool IsValidValue(double value) => IsValidValue(value, DefaultRange);

    /// <summary>
    /// Gets the raw sensitivity value.
    /// </summary>
    public double Value { get; }

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
    public bool IsValid() => IsValidValue(Value, _range);

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
    /// Tries to create a SensitivityValue from a raw value using the default range without throwing an exception.
    /// </summary>
    /// <param name="value">The sensitivity value.</param>
    /// <param name="result">When this method returns, contains the SensitivityValue if successful; otherwise, default.</param>
    /// <returns>True if the value was created successfully; otherwise, false.</returns>
    public static bool TryFromRaw(double value, out SensitivityValue result)
    {
        if (IsValidValue(value))
        {
            result = new SensitivityValue(value, DefaultRange);
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Creates a SensitivityValue from a raw value, clamping to the default range.
    /// </summary>
    /// <param name="value">The sensitivity value to clamp.</param>
    /// <returns>A new SensitivityValue clamped to the default range.</returns>
    public static SensitivityValue FromRawClamped(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            return Zero;

        return new SensitivityValue(DefaultRange.Clamp(value), DefaultRange);
    }

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
    /// Tries to create a SensitivityValue from a raw value with a custom range without throwing an exception.
    /// </summary>
    /// <param name="value">The sensitivity value.</param>
    /// <param name="minValue">The minimum allowed value.</param>
    /// <param name="maxValue">The maximum allowed value.</param>
    /// <param name="result">When this method returns, contains the SensitivityValue if successful; otherwise, default.</param>
    /// <returns>True if the value was created successfully; otherwise, false.</returns>
    public static bool TryFromRaw(double value, double minValue, double maxValue, out SensitivityValue result)
    {
        var range = new ValueRange(minValue, maxValue);
        if (range.IsValid && IsValidValue(value, range))
        {
            result = new SensitivityValue(value, range);
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Creates a neutral sensitivity (0.0).
    /// </summary>
    /// <returns>A new SensitivityValue with value 0.</returns>
    /// <remarks>
    /// Consider using <see cref="Zero"/> directly for better clarity.
    /// </remarks>
    [Obsolete("Use SensitivityValue.Zero instead for clearer intent.")]
    public static SensitivityValue Neutral() => Zero;

    /// <summary>
    /// Normalizes the sensitivity value to 0-1 range based on its min/max bounds.
    /// </summary>
    /// <returns>A normalized value between 0 and 1.</returns>
    public NormalizedValue Normalize()
    {
        var normalized = _range.Normalize(Value);
        return NormalizedValue.FromRatio(normalized);
    }

    /// <summary>
    /// Returns a string representation of the value.
    /// </summary>
    public override string ToString() => $"{Value:F2}";

    /// <summary>
    /// Implicitly converts a SensitivityValue to a double.
    /// </summary>
    /// <param name="value">The SensitivityValue to convert.</param>
    public static implicit operator double(SensitivityValue value) => value.Value;

    /// <summary>
    /// Explicitly converts a double to a SensitivityValue using the default range.
    /// </summary>
    /// <param name="value">The double value to convert (must be within default range).</param>
    public static explicit operator SensitivityValue(double value) => FromRaw(value);

    /// <summary>
    /// Adds two SensitivityValues, clamping the result to the first value's range.
    /// </summary>
    /// <param name="left">The first SensitivityValue (determines the result range).</param>
    /// <param name="right">The second SensitivityValue.</param>
    /// <returns>A new SensitivityValue with the clamped result using the left operand's range.</returns>
    /// <remarks>
    /// The result uses the left operand's range for clamping. When combining values with different ranges,
    /// ensure the left operand has the desired range.
    /// </remarks>
    public static SensitivityValue operator +(SensitivityValue left, SensitivityValue right)
    {
        var sum = left.Value + right.Value;
        return new SensitivityValue(left._range.Clamp(sum), left._range);
    }

    /// <summary>
    /// Subtracts one SensitivityValue from another, clamping the result to the first value's range.
    /// </summary>
    /// <param name="left">The first SensitivityValue (determines the result range).</param>
    /// <param name="right">The second SensitivityValue.</param>
    /// <returns>A new SensitivityValue with the clamped result using the left operand's range.</returns>
    /// <remarks>
    /// The result uses the left operand's range for clamping. When combining values with different ranges,
    /// ensure the left operand has the desired range.
    /// </remarks>
    public static SensitivityValue operator -(SensitivityValue left, SensitivityValue right)
    {
        var difference = left.Value - right.Value;
        return new SensitivityValue(left._range.Clamp(difference), left._range);
    }

    /// <summary>
    /// Multiplies a SensitivityValue by a scalar, clamping the result to the value's range.
    /// </summary>
    /// <param name="left">The SensitivityValue.</param>
    /// <param name="right">The scalar multiplier.</param>
    /// <returns>A new SensitivityValue with the clamped result.</returns>
    public static SensitivityValue operator *(SensitivityValue left, double right)
    {
        var product = left.Value * right;
        return new SensitivityValue(left._range.Clamp(product), left._range);
    }

    /// <summary>
    /// Multiplies a scalar by a SensitivityValue, clamping the result to the value's range.
    /// </summary>
    /// <param name="left">The scalar multiplier.</param>
    /// <param name="right">The SensitivityValue.</param>
    /// <returns>A new SensitivityValue with the clamped result.</returns>
    public static SensitivityValue operator *(double left, SensitivityValue right)
    {
        var product = left * right.Value;
        return new SensitivityValue(right._range.Clamp(product), right._range);
    }

    /// <summary>
    /// Negates a SensitivityValue, clamping the result to the value's range.
    /// </summary>
    /// <param name="value">The SensitivityValue to negate.</param>
    /// <returns>A new SensitivityValue with the negated value.</returns>
    public static SensitivityValue operator -(SensitivityValue value)
    {
        var negated = -value.Value;
        return new SensitivityValue(value._range.Clamp(negated), value._range);
    }
}