namespace dotMigrata.Core.Values;

/// <summary>
/// Represents a factor sensitivity value with a configurable range.
/// </summary>
/// <remarks>
///     <para>The default range is -10 to +10, representing negative to positive sensitivity.</para>
///     <para>This type is implemented as a readonly record struct for zero-allocation overhead.</para>
/// </remarks>
public readonly record struct SensitivityValue : IRangedValue, INormalizable
{
    /// <summary>
    /// The default minimum sensitivity value (-10.0).
    /// </summary>
    public const double DefaultMinValue = -10.0;

    /// <summary>
    /// The default maximum sensitivity value (+10.0).
    /// </summary>
    public const double DefaultMaxValue = 10.0;

    private static readonly ValueRange DefaultRange = new(DefaultMinValue, DefaultMaxValue);

    /// <summary>
    /// A neutral sensitivity value (0.0) using the default range.
    /// </summary>
    public static readonly SensitivityValue Zero = new(0.0, DefaultRange);

    private readonly ValueRange _range;

    /// <summary>
    /// Initializes a new instance of the <see cref="SensitivityValue" /> struct.
    /// </summary>
    /// <param name="value">The sensitivity value.</param>
    /// <param name="range">The valid range for this sensitivity value.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="range" /> is invalid (Min ≥ Max).
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value" /> is outside the specified range, is NaN, or is Infinity.
    /// </exception>
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
    /// Normalizes this sensitivity value to the 0-1 range based on its min/max bounds.
    /// </summary>
    /// <returns>A <see cref="NormalizedValue" /> between 0 and 1.</returns>
    public NormalizedValue Normalize()
    {
        var normalized = _range.Normalize(Value);
        return NormalizedValue.FromRatio(normalized);
    }

    /// <summary>
    /// Gets the underlying sensitivity value.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Gets the minimum allowed value for this instance.
    /// </summary>
    public double MinValue => _range.Min;

    /// <summary>
    /// Gets the maximum allowed value for this instance.
    /// </summary>
    public double MaxValue => _range.Max;

    /// <summary>
    /// Gets the valid range for this instance.
    /// </summary>
    public ValueRange Range => _range;

    /// <summary>
    /// Determines whether this instance contains a valid value.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> if the value is valid; otherwise, <see langword="false" />.
    /// </returns>
    public bool IsValid()
    {
        return IsValidValue(Value, _range);
    }

    /// <summary>
    /// Determines whether the specified value is within this sensitivity's range.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>
    /// <see langword="true" /> if the value is within range; otherwise, <see langword="false" />.
    /// </returns>
    public bool Contains(double value)
    {
        return _range.Contains(value);
    }

    /// <summary>
    /// Clamps the specified value to this sensitivity's range.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <returns>The value clamped to the valid range.</returns>
    public double Clamp(double value)
    {
        return _range.Clamp(value);
    }

    /// <summary>
    /// Determines whether the specified value is valid for a <see cref="SensitivityValue" /> within the specified range.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="range">The valid range.</param>
    /// <returns>
    /// <see langword="true" /> if the value is within range and is not NaN or Infinity;
    /// otherwise, <see langword="false" />.
    /// </returns>
    public static bool IsValidValue(double value, ValueRange range)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value) && range.Contains(value);
    }

    /// <summary>
    /// Determines whether the specified value is valid for a <see cref="SensitivityValue" /> using the default range.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>
    /// <see langword="true" /> if the value is valid; otherwise, <see langword="false" />.
    /// </returns>
    public static bool IsValidValue(double value)
    {
        return IsValidValue(value, DefaultRange);
    }

    /// <summary>
    /// Creates a new <see cref="SensitivityValue" /> from the specified raw value using the default range (-10 to +10).
    /// </summary>
    /// <param name="value">The sensitivity value.</param>
    /// <returns>A new <see cref="SensitivityValue" /> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value" /> is outside the default range.
    /// </exception>
    public static SensitivityValue FromRaw(double value)
    {
        return new SensitivityValue(value, DefaultRange);
    }

    /// <summary>
    /// Attempts to create a <see cref="SensitivityValue" /> from the specified raw value using the default range without
    /// throwing an exception.
    /// </summary>
    /// <param name="value">The sensitivity value.</param>
    /// <param name="result">
    /// When this method returns, contains the <see cref="SensitivityValue" /> if the conversion succeeded,
    /// or the default value if the conversion failed.
    /// </param>
    /// <returns>
    /// <see langword="true" /> if the value was created successfully; otherwise, <see langword="false" />.
    /// </returns>
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
    /// Creates a new <see cref="SensitivityValue" /> from the specified raw value, clamping to the default range.
    /// </summary>
    /// <param name="value">The sensitivity value to clamp.</param>
    /// <returns>A new <see cref="SensitivityValue" /> clamped to the default range.</returns>
    public static SensitivityValue FromRawClamped(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            return Zero;

        return new SensitivityValue(DefaultRange.Clamp(value), DefaultRange);
    }

    /// <summary>
    /// Creates a new <see cref="SensitivityValue" /> from the specified raw value with a custom range.
    /// </summary>
    /// <param name="value">The sensitivity value.</param>
    /// <param name="minValue">The minimum allowed value.</param>
    /// <param name="maxValue">The maximum allowed value.</param>
    /// <returns>A new <see cref="SensitivityValue" /> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value" /> is outside the specified range.
    /// </exception>
    public static SensitivityValue FromRaw(double value, double minValue, double maxValue)
    {
        return new SensitivityValue(value, new ValueRange(minValue, maxValue));
    }

    /// <summary>
    /// Attempts to create a <see cref="SensitivityValue" /> from the specified raw value with a custom range without throwing
    /// an exception.
    /// </summary>
    /// <param name="value">The sensitivity value.</param>
    /// <param name="minValue">The minimum allowed value.</param>
    /// <param name="maxValue">The maximum allowed value.</param>
    /// <param name="result">
    /// When this method returns, contains the <see cref="SensitivityValue" /> if the conversion succeeded,
    /// or the default value if the conversion failed.
    /// </param>
    /// <returns>
    /// <see langword="true" /> if the value was created successfully; otherwise, <see langword="false" />.
    /// </returns>
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
    /// Returns a string representation of the value.
    /// </summary>
    /// <returns>A string that represents the current value.</returns>
    public override string ToString()
    {
        return $"{Value:F2}";
    }

    /// <summary>
    /// Defines an implicit conversion of a <see cref="SensitivityValue" /> to a <see cref="double" />.
    /// </summary>
    /// <param name="value">The <see cref="SensitivityValue" /> to convert.</param>
    /// <returns>The underlying double value.</returns>
    public static implicit operator double(SensitivityValue value)
    {
        return value.Value;
    }

    /// <summary>
    /// Defines an explicit conversion of a <see cref="double" /> to a <see cref="SensitivityValue" /> using the default range.
    /// </summary>
    /// <param name="value">The double value to convert. Must be within the default range.</param>
    /// <returns>A new <see cref="SensitivityValue" /> instance.</returns>
    public static explicit operator SensitivityValue(double value)
    {
        return FromRaw(value);
    }

    /// <summary>
    /// Adds two <see cref="SensitivityValue" /> instances, clamping the result to the first value's range.
    /// </summary>
    /// <param name="left">The first <see cref="SensitivityValue" /> (determines the result range).</param>
    /// <param name="right">The second <see cref="SensitivityValue" />.</param>
    /// <returns>A new <see cref="SensitivityValue" /> with the clamped result.</returns>
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
    /// Subtracts one <see cref="SensitivityValue" /> from another, clamping the result to the first value's range.
    /// </summary>
    /// <param name="left">The <see cref="SensitivityValue" /> to subtract from (determines the result range).</param>
    /// <param name="right">The <see cref="SensitivityValue" /> to subtract.</param>
    /// <returns>A new <see cref="SensitivityValue" /> with the clamped result.</returns>
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
    /// Multiplies a <see cref="SensitivityValue" /> by a scalar, clamping the result to the value's range.
    /// </summary>
    /// <param name="left">The <see cref="SensitivityValue" /> operand.</param>
    /// <param name="right">The scalar multiplier.</param>
    /// <returns>A new <see cref="SensitivityValue" /> with the clamped result.</returns>
    public static SensitivityValue operator *(SensitivityValue left, double right)
    {
        var product = left.Value * right;
        return new SensitivityValue(left._range.Clamp(product), left._range);
    }

    /// <summary>
    /// Multiplies a scalar by a <see cref="SensitivityValue" />, clamping the result to the value's range.
    /// </summary>
    /// <param name="left">The scalar multiplier.</param>
    /// <param name="right">The <see cref="SensitivityValue" /> operand.</param>
    /// <returns>A new <see cref="SensitivityValue" /> with the clamped result.</returns>
    public static SensitivityValue operator *(double left, SensitivityValue right)
    {
        var product = left * right.Value;
        return new SensitivityValue(right._range.Clamp(product), right._range);
    }

    /// <summary>
    /// Negates a <see cref="SensitivityValue" />, clamping the result to the value's range.
    /// </summary>
    /// <param name="value">The <see cref="SensitivityValue" /> to negate.</param>
    /// <returns>A new <see cref="SensitivityValue" /> with the negated value.</returns>
    public static SensitivityValue operator -(SensitivityValue value)
    {
        var negated = -value.Value;
        return new SensitivityValue(value._range.Clamp(negated), value._range);
    }
}