namespace dotMigrata.Core.Values;

/// <summary>
/// Represents a normalized value in the 0-1 range. Used for ratios, percentages, and normalized intensities.
/// </summary>
/// <remarks>
/// This type is implemented as a readonly record struct for zero-allocation overhead.
/// </remarks>
public readonly record struct NormalizedValue : IValue<double>, IRangedValue, INormalizable
{
    /// <summary>
    /// The minimum allowed value (0.0).
    /// </summary>
    public const double MinValueConst = 0.0;

    /// <summary>
    /// The maximum allowed value (1.0).
    /// </summary>
    public const double MaxValueConst = 1.0;

    private static readonly ValueRange ValidRange = new(0.0, 1.0);

    /// <summary>
    /// A normalized value representing zero (0.0).
    /// </summary>
    public static readonly NormalizedValue Zero = new(0.0);

    /// <summary>
    /// A normalized value representing one (1.0).
    /// </summary>
    public static readonly NormalizedValue One = new(1.0);

    /// <summary>
    /// A normalized value representing half (0.5).
    /// </summary>
    public static readonly NormalizedValue Half = new(0.5);

    /// <summary>
    /// Initializes a new instance of the <see cref="NormalizedValue" /> struct.
    /// </summary>
    /// <param name="value">The value to store. Must be between 0 and 1.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value" /> is outside the 0-1 range, is NaN, or is Infinity.
    /// </exception>
    private NormalizedValue(double value)
    {
        if (!IsValidValue(value))
            throw new ArgumentOutOfRangeException(nameof(value),
                $"Value must be a valid number between {MinValueConst} and {MaxValueConst}.");

        Value = value;
    }

    /// <summary>
    /// Returns this instance, since <see cref="NormalizedValue" /> is already normalized.
    /// </summary>
    /// <returns>This instance.</returns>
    public NormalizedValue Normalize()
    {
        return this;
    }

    /// <summary>
    /// Gets the minimum allowed value for this type.
    /// </summary>
    public double MinValue => MinValueConst;

    /// <summary>
    /// Gets the maximum allowed value for this type.
    /// </summary>
    public double MaxValue => MaxValueConst;

    /// <summary>
    /// Gets the valid range for this type (0-1).
    /// </summary>
    public ValueRange Range => ValidRange;

    /// <summary>
    /// Determines whether the specified value is within the normalized range (0-1).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>
    /// <see langword="true" /> if the value is within range; otherwise, <see langword="false" />.
    /// </returns>
    public bool Contains(double value)
    {
        return ValidRange.Contains(value);
    }

    /// <summary>
    /// Clamps the specified value to the normalized range (0-1).
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <returns>The value clamped to the range 0-1.</returns>
    public double Clamp(double value)
    {
        return ValidRange.Clamp(value);
    }

    /// <summary>
    /// Gets the underlying double value in the range 0-1.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Determines whether this instance contains a valid value.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> if the value is valid; otherwise, <see langword="false" />.
    /// </returns>
    public bool IsValid()
    {
        return IsValidValue(Value);
    }

    /// <summary>
    /// Determines whether the specified value is valid for a <see cref="NormalizedValue" />.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>
    /// <see langword="true" /> if the value is between 0 and 1 and is not NaN or Infinity;
    /// otherwise, <see langword="false" />.
    /// </returns>
    public static bool IsValidValue(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value) && ValidRange.Contains(value);
    }

    /// <summary>
    /// Creates a new <see cref="NormalizedValue" /> from the specified ratio.
    /// </summary>
    /// <param name="ratio">The ratio value, which must be between 0 and 1.</param>
    /// <returns>A new <see cref="NormalizedValue" /> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="ratio" /> is outside the 0-1 range.
    /// </exception>
    public static NormalizedValue FromRatio(double ratio)
    {
        return new NormalizedValue(ratio);
    }

    /// <summary>
    /// Attempts to create a <see cref="NormalizedValue" /> from the specified ratio without throwing an exception.
    /// </summary>
    /// <param name="ratio">The ratio value, which should be between 0 and 1.</param>
    /// <param name="result">
    /// When this method returns, contains the <see cref="NormalizedValue" /> if the conversion succeeded,
    /// or the default value if the conversion failed.
    /// </param>
    /// <returns>
    /// <see langword="true" /> if the value was created successfully; otherwise, <see langword="false" />.
    /// </returns>
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
    /// Creates a new <see cref="NormalizedValue" /> from the specified ratio, clamping the value to the valid range.
    /// </summary>
    /// <param name="ratio">The ratio value to clamp.</param>
    /// <returns>A new <see cref="NormalizedValue" /> clamped to the range 0-1.</returns>
    public static NormalizedValue FromRatioClamped(double ratio)
    {
        if (double.IsNaN(ratio) || double.IsInfinity(ratio))
            return Zero;

        return new NormalizedValue(ValidRange.Clamp(ratio));
    }

    /// <summary>
    /// Creates a new <see cref="NormalizedValue" /> from the specified percentage.
    /// </summary>
    /// <param name="percentage">The percentage value, which must be between 0 and 100.</param>
    /// <returns>A new <see cref="NormalizedValue" /> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="percentage" /> is outside the 0-100 range.
    /// </exception>
    public static NormalizedValue FromPercentage(double percentage)
    {
        if (percentage is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(percentage),
                "Percentage must be between 0 and 100.");

        return new NormalizedValue(percentage / 100.0);
    }

    /// <summary>
    /// Attempts to create a <see cref="NormalizedValue" /> from the specified percentage without throwing an exception.
    /// </summary>
    /// <param name="percentage">The percentage value, which should be between 0 and 100.</param>
    /// <param name="result">
    /// When this method returns, contains the <see cref="NormalizedValue" /> if the conversion succeeded,
    /// or the default value if the conversion failed.
    /// </param>
    /// <returns>
    /// <see langword="true" /> if the value was created successfully; otherwise, <see langword="false" />.
    /// </returns>
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
    /// Converts this value to a percentage in the range 0-100.
    /// </summary>
    /// <returns>The value expressed as a percentage.</returns>
    public double ToPercentage()
    {
        return Value * 100.0;
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
    /// Defines an implicit conversion of a <see cref="NormalizedValue" /> to a <see cref="double" />.
    /// </summary>
    /// <param name="value">The <see cref="NormalizedValue" /> to convert.</param>
    /// <returns>The underlying double value.</returns>
    public static implicit operator double(NormalizedValue value)
    {
        return value.Value;
    }

    /// <summary>
    /// Defines an explicit conversion of a <see cref="double" /> to a <see cref="NormalizedValue" />.
    /// </summary>
    /// <param name="value">The double value to convert. Must be between 0 and 1.</param>
    /// <returns>A new <see cref="NormalizedValue" /> instance.</returns>
    public static explicit operator NormalizedValue(double value)
    {
        return FromRatio(value);
    }

    /// <summary>
    /// Multiplies a <see cref="NormalizedValue" /> by a scalar, clamping the result to 0-1.
    /// </summary>
    /// <param name="left">The <see cref="NormalizedValue" /> operand.</param>
    /// <param name="right">The scalar multiplier.</param>
    /// <returns>A new <see cref="NormalizedValue" /> with the clamped result.</returns>
    public static NormalizedValue operator *(NormalizedValue left, double right)
    {
        return FromRatioClamped(left.Value * right);
    }

    /// <summary>
    /// Multiplies a scalar by a <see cref="NormalizedValue" />, clamping the result to 0-1.
    /// </summary>
    /// <param name="left">The scalar multiplier.</param>
    /// <param name="right">The <see cref="NormalizedValue" /> operand.</param>
    /// <returns>A new <see cref="NormalizedValue" /> with the clamped result.</returns>
    public static NormalizedValue operator *(double left, NormalizedValue right)
    {
        return FromRatioClamped(left * right.Value);
    }

    /// <summary>
    /// Adds two <see cref="NormalizedValue" /> instances, clamping the result to 0-1.
    /// </summary>
    /// <param name="left">The first <see cref="NormalizedValue" />.</param>
    /// <param name="right">The second <see cref="NormalizedValue" />.</param>
    /// <returns>A new <see cref="NormalizedValue" /> with the clamped result.</returns>
    public static NormalizedValue operator +(NormalizedValue left, NormalizedValue right)
    {
        return FromRatioClamped(left.Value + right.Value);
    }

    /// <summary>
    /// Subtracts one <see cref="NormalizedValue" /> from another, clamping the result to 0-1.
    /// </summary>
    /// <param name="left">The <see cref="NormalizedValue" /> to subtract from.</param>
    /// <param name="right">The <see cref="NormalizedValue" /> to subtract.</param>
    /// <returns>A new <see cref="NormalizedValue" /> with the clamped result.</returns>
    public static NormalizedValue operator -(NormalizedValue left, NormalizedValue right)
    {
        return FromRatioClamped(left.Value - right.Value);
    }
}