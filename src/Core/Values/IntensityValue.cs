namespace dotMigrata.Core.Values;

/// <summary>
/// Represents a raw intensity value for a factor with validation and normalization support.
/// </summary>
/// <remarks>
///     <para>Intensity values must be non-negative (≥ 0).</para>
///     <para>This type is implemented as a readonly record struct for zero-allocation overhead.</para>
///     <para>
///     This type does not implement <see cref="INormalizable" /> because normalization requires
///     a <see cref="FactorDefinition" />.
///     </para>
/// </remarks>
public readonly record struct IntensityValue : IValue<double>
{
    /// <summary>
    /// The minimum allowed intensity value (0.0). Intensities cannot be negative.
    /// </summary>
    public const double MinAllowedValue = 0.0;

    /// <summary>
    /// An intensity value representing zero (0.0).
    /// </summary>
    public static readonly IntensityValue Zero = new(0.0);

    /// <summary>
    /// An intensity value representing one (1.0).
    /// </summary>
    public static readonly IntensityValue One = new(1.0);

    /// <summary>
    /// Initializes a new instance of the <see cref="IntensityValue" /> struct.
    /// </summary>
    /// <param name="value">The intensity value. Must be non-negative.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value" /> is NaN, Infinity, or negative.
    /// </exception>
    private IntensityValue(double value)
    {
        if (!IsValidValue(value))
            throw new ArgumentOutOfRangeException(nameof(value),
                $"Intensity value must be a valid non-negative number (>= {MinAllowedValue}).");

        Value = value;
    }

    /// <summary>
    /// Gets the underlying intensity value.
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
    /// Determines whether the specified value is valid for an <see cref="IntensityValue" />.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>
    /// <see langword="true" /> if the value is non-negative and is not NaN or Infinity;
    /// otherwise, <see langword="false" />.
    /// </returns>
    public static bool IsValidValue(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value) && value >= MinAllowedValue;
    }

    /// <summary>
    /// Creates a new <see cref="IntensityValue" /> from the specified raw value.
    /// </summary>
    /// <param name="value">The raw intensity value. Must be non-negative.</param>
    /// <returns>A new <see cref="IntensityValue" /> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value" /> is NaN, Infinity, or negative.
    /// </exception>
    public static IntensityValue FromRaw(double value)
    {
        return new IntensityValue(value);
    }

    /// <summary>
    /// Attempts to create an <see cref="IntensityValue" /> from the specified raw value without throwing an exception.
    /// </summary>
    /// <param name="value">The raw intensity value.</param>
    /// <param name="result">
    /// When this method returns, contains the <see cref="IntensityValue" /> if the conversion succeeded,
    /// or the default value if the conversion failed.
    /// </param>
    /// <returns>
    /// <see langword="true" /> if the value was created successfully; otherwise, <see langword="false" />.
    /// </returns>
    public static bool TryFromRaw(double value, out IntensityValue result)
    {
        if (IsValidValue(value))
        {
            result = new IntensityValue(value);
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Creates a new <see cref="IntensityValue" /> from the specified raw value, clamping negative values to zero.
    /// </summary>
    /// <param name="value">The raw intensity value.</param>
    /// <returns>A new <see cref="IntensityValue" /> with a non-negative value.</returns>
    public static IntensityValue FromRawClamped(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            return Zero;

        return new IntensityValue(Math.Max(MinAllowedValue, value));
    }

    /// <summary>
    /// Creates a new <see cref="IntensityValue" /> with validation against a factor definition's range.
    /// </summary>
    /// <param name="value">The raw intensity value. Must be non-negative.</param>
    /// <param name="factorDefinition">The factor definition to validate against.</param>
    /// <returns>A new <see cref="IntensityValue" /> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factorDefinition" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value" /> is NaN, Infinity, or negative.
    /// </exception>
    /// <remarks>
    /// This method does not enforce the factor's min/max range, only validates that the value is non-negative.
    /// Values outside the range will be clamped during normalization.
    /// </remarks>
    public static IntensityValue Create(double value, FactorDefinition factorDefinition)
    {
        ArgumentNullException.ThrowIfNull(factorDefinition);
        return new IntensityValue(value);
    }

    /// <summary>
    /// Normalizes this intensity value using the specified factor definition.
    /// </summary>
    /// <param name="factorDefinition">The factor definition containing normalization rules.</param>
    /// <returns>A <see cref="NormalizedValue" /> between 0 and 1.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factorDefinition" /> is <see langword="null" />.
    /// </exception>
    public NormalizedValue Normalize(FactorDefinition factorDefinition)
    {
        ArgumentNullException.ThrowIfNull(factorDefinition);
        var normalized = factorDefinition.Normalize(Value);
        return NormalizedValue.FromRatio(normalized);
    }

    /// <summary>
    /// Clamps this intensity value to the specified factor definition's range.
    /// </summary>
    /// <param name="factorDefinition">The factor definition containing min/max bounds.</param>
    /// <returns>A new <see cref="IntensityValue" /> clamped to the factor's range.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factorDefinition" /> is <see langword="null" />.
    /// </exception>
    public IntensityValue Clamp(FactorDefinition factorDefinition)
    {
        ArgumentNullException.ThrowIfNull(factorDefinition);
        var clamped = Math.Clamp(Value, factorDefinition.MinValue, factorDefinition.MaxValue);
        return new IntensityValue(clamped);
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
    /// Defines an implicit conversion of an <see cref="IntensityValue" /> to a <see cref="double" />.
    /// </summary>
    /// <param name="value">The <see cref="IntensityValue" /> to convert.</param>
    /// <returns>The underlying double value.</returns>
    public static implicit operator double(IntensityValue value)
    {
        return value.Value;
    }

    /// <summary>
    /// Defines an explicit conversion of a <see cref="double" /> to an <see cref="IntensityValue" />.
    /// </summary>
    /// <param name="value">The double value to convert. Must be non-negative.</param>
    /// <returns>A new <see cref="IntensityValue" /> instance.</returns>
    public static explicit operator IntensityValue(double value)
    {
        return FromRaw(value);
    }

    /// <summary>
    /// Adds two <see cref="IntensityValue" /> instances.
    /// </summary>
    /// <param name="left">The first <see cref="IntensityValue" />.</param>
    /// <param name="right">The second <see cref="IntensityValue" />.</param>
    /// <returns>A new <see cref="IntensityValue" /> with the sum.</returns>
    /// <remarks>
    /// If the sum results in an invalid value (overflow), returns a clamped result.
    /// </remarks>
    public static IntensityValue operator +(IntensityValue left, IntensityValue right)
    {
        return FromRawClamped(left.Value + right.Value);
    }

    /// <summary>
    /// Subtracts one <see cref="IntensityValue" /> from another, clamping the result to zero.
    /// </summary>
    /// <param name="left">The <see cref="IntensityValue" /> to subtract from.</param>
    /// <param name="right">The <see cref="IntensityValue" /> to subtract.</param>
    /// <returns>A new <see cref="IntensityValue" /> with the clamped difference.</returns>
    public static IntensityValue operator -(IntensityValue left, IntensityValue right)
    {
        return FromRawClamped(left.Value - right.Value);
    }

    /// <summary>
    /// Multiplies an <see cref="IntensityValue" /> by a scalar.
    /// </summary>
    /// <param name="left">The <see cref="IntensityValue" /> operand.</param>
    /// <param name="right">The scalar multiplier.</param>
    /// <returns>A new <see cref="IntensityValue" /> with the clamped result.</returns>
    public static IntensityValue operator *(IntensityValue left, double right)
    {
        return FromRawClamped(left.Value * right);
    }

    /// <summary>
    /// Multiplies a scalar by an <see cref="IntensityValue" />.
    /// </summary>
    /// <param name="left">The scalar multiplier.</param>
    /// <param name="right">The <see cref="IntensityValue" /> operand.</param>
    /// <returns>A new <see cref="IntensityValue" /> with the clamped result.</returns>
    public static IntensityValue operator *(double left, IntensityValue right)
    {
        return FromRawClamped(left * right.Value);
    }

    /// <summary>
    /// Divides an <see cref="IntensityValue" /> by a scalar.
    /// </summary>
    /// <param name="left">The <see cref="IntensityValue" /> dividend.</param>
    /// <param name="right">The scalar divisor. Must be non-zero.</param>
    /// <returns>A new <see cref="IntensityValue" /> with the clamped result.</returns>
    /// <exception cref="DivideByZeroException">
    /// Thrown when <paramref name="right" /> is zero.
    /// </exception>
    public static IntensityValue operator /(IntensityValue left, double right)
    {
        return right == 0
            ? throw new DivideByZeroException("Cannot divide an IntensityValue by zero.")
            : FromRawClamped(left.Value / right);
    }
}