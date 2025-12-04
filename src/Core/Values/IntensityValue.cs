namespace dotMigrata.Core.Values;

/// <summary>
/// Represents a raw intensity value for a factor with validation and normalization support.
/// Wraps the raw value and provides type-safe operations.
/// Intensity values must be non-negative (>= 0).
/// Implements as a readonly record struct for zero-allocation overhead.
/// Note: IntensityValue does not implement INormalizable because normalization requires a FactorDefinition.
/// </summary>
public readonly record struct IntensityValue : IValue<double>
{
    /// <summary>
    /// Minimum allowed intensity value (0.0 - intensities cannot be negative).
    /// </summary>
    public const double MinAllowedValue = 0.0;

    /// <summary>
    /// An intensity value of zero (0.0).
    /// </summary>
    public static readonly IntensityValue Zero = new(0.0);

    /// <summary>
    /// An intensity value of one (1.0).
    /// </summary>
    public static readonly IntensityValue One = new(1.0);

    /// <summary>
    /// Initializes a new instance of the IntensityValue struct.
    /// </summary>
    /// <param name="value">The intensity value. Must be non-negative.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is NaN, Infinity, or negative.</exception>
    private IntensityValue(double value)
    {
        if (!IsValidValue(value))
            throw new ArgumentOutOfRangeException(nameof(value),
                $"Intensity value must be a valid non-negative number (>= {MinAllowedValue}).");

        Value = value;
    }

    /// <summary>
    /// Checks if a value is valid for an IntensityValue (non-negative, not NaN, not Infinity).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is valid; otherwise, false.</returns>
    public static bool IsValidValue(double value) =>
        !double.IsNaN(value) && !double.IsInfinity(value) && value >= MinAllowedValue;

    /// <summary>
    /// Gets the raw intensity value.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Validates that this intensity value is valid (non-negative, not NaN, not Infinity).
    /// </summary>
    /// <returns>True if the value is valid; otherwise, false.</returns>
    public bool IsValid() => IsValidValue(Value);

    /// <summary>
    /// Creates an IntensityValue from a raw value.
    /// </summary>
    /// <param name="value">The raw intensity value. Must be non-negative.</param>
    /// <returns>A new IntensityValue.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is NaN, Infinity, or negative.</exception>
    public static IntensityValue FromRaw(double value) => new(value);

    /// <summary>
    /// Tries to create an IntensityValue from a raw value without throwing an exception.
    /// </summary>
    /// <param name="value">The raw intensity value.</param>
    /// <param name="result">When this method returns, contains the IntensityValue if successful; otherwise, default.</param>
    /// <returns>True if the value was created successfully; otherwise, false.</returns>
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
    /// Creates an IntensityValue from a raw value, clamping negative values to zero.
    /// </summary>
    /// <param name="value">The raw intensity value.</param>
    /// <returns>A new IntensityValue with non-negative value.</returns>
    public static IntensityValue FromRawClamped(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            return Zero;

        return new IntensityValue(Math.Max(MinAllowedValue, value));
    }

    /// <summary>
    /// Creates an IntensityValue with validation against a factor definition's range.
    /// </summary>
    /// <param name="value">The raw intensity value. Must be non-negative.</param>
    /// <param name="factorDefinition">The factor definition to validate against.</param>
    /// <returns>A new IntensityValue.</returns>
    /// <exception cref="ArgumentNullException">Thrown when factorDefinition is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is NaN, Infinity, or negative.</exception>
    /// <remarks>
    /// Note: This does not enforce the factor's min/max range, only validates that the value is non-negative.
    /// Values outside the range will be clamped during normalization.
    /// </remarks>
    public static IntensityValue Create(double value, FactorDefinition factorDefinition)
    {
        ArgumentNullException.ThrowIfNull(factorDefinition);
        return new IntensityValue(value);
    }

    /// <summary>
    /// Normalizes the intensity value using the provided factor definition.
    /// </summary>
    /// <param name="factorDefinition">The factor definition containing normalization rules.</param>
    /// <returns>A normalized value between 0 and 1.</returns>
    /// <exception cref="ArgumentNullException">Thrown when factorDefinition is null.</exception>
    public NormalizedValue Normalize(FactorDefinition factorDefinition)
    {
        ArgumentNullException.ThrowIfNull(factorDefinition);
        var normalized = factorDefinition.Normalize(Value);
        return NormalizedValue.FromRatio(normalized);
    }

    /// <summary>
    /// Clamps the intensity value to the factor definition's range.
    /// </summary>
    /// <param name="factorDefinition">The factor definition containing min/max bounds.</param>
    /// <returns>A new IntensityValue clamped to the factor's range.</returns>
    /// <exception cref="ArgumentNullException">Thrown when factorDefinition is null.</exception>
    public IntensityValue Clamp(FactorDefinition factorDefinition)
    {
        ArgumentNullException.ThrowIfNull(factorDefinition);
        var clamped = Math.Clamp(Value, factorDefinition.MinValue, factorDefinition.MaxValue);
        return new IntensityValue(clamped);
    }

    /// <summary>
    /// Returns a string representation of the value.
    /// </summary>
    public override string ToString() => $"{Value:F2}";

    /// <summary>
    /// Implicitly converts an IntensityValue to a double.
    /// </summary>
    /// <param name="value">The IntensityValue to convert.</param>
    public static implicit operator double(IntensityValue value) => value.Value;

    /// <summary>
    /// Explicitly converts a double to an IntensityValue.
    /// </summary>
    /// <param name="value">The double value to convert (must be non-negative).</param>
    public static explicit operator IntensityValue(double value) => FromRaw(value);

    /// <summary>
    /// Adds two IntensityValues.
    /// </summary>
    /// <param name="left">The first IntensityValue.</param>
    /// <param name="right">The second IntensityValue.</param>
    /// <returns>A new IntensityValue with the sum.</returns>
    /// <remarks>
    /// If the sum results in an invalid value (overflow), returns a clamped result.
    /// </remarks>
    public static IntensityValue operator +(IntensityValue left, IntensityValue right) =>
        FromRawClamped(left.Value + right.Value);

    /// <summary>
    /// Subtracts one IntensityValue from another, clamping the result to zero.
    /// </summary>
    /// <param name="left">The first IntensityValue.</param>
    /// <param name="right">The second IntensityValue.</param>
    /// <returns>A new IntensityValue with the clamped difference.</returns>
    public static IntensityValue operator -(IntensityValue left, IntensityValue right) =>
        FromRawClamped(left.Value - right.Value);

    /// <summary>
    /// Multiplies an IntensityValue by a scalar.
    /// </summary>
    /// <param name="left">The IntensityValue.</param>
    /// <param name="right">The scalar multiplier (must result in non-negative value).</param>
    /// <returns>A new IntensityValue with the clamped result.</returns>
    public static IntensityValue operator *(IntensityValue left, double right) =>
        FromRawClamped(left.Value * right);

    /// <summary>
    /// Multiplies a scalar by an IntensityValue.
    /// </summary>
    /// <param name="left">The scalar multiplier.</param>
    /// <param name="right">The IntensityValue.</param>
    /// <returns>A new IntensityValue with the clamped result.</returns>
    public static IntensityValue operator *(double left, IntensityValue right) =>
        FromRawClamped(left * right.Value);

    /// <summary>
    /// Divides an IntensityValue by a scalar.
    /// </summary>
    /// <param name="left">The IntensityValue.</param>
    /// <param name="right">The scalar divisor (must be non-zero).</param>
    /// <returns>A new IntensityValue with the clamped result.</returns>
    /// <exception cref="DivideByZeroException">Thrown when the divisor is zero.</exception>
    public static IntensityValue operator /(IntensityValue left, double right)
    {
        return right == 0
            ? throw new DivideByZeroException("Cannot divide an IntensityValue by zero.")
            : FromRawClamped(left.Value / right);
    }
}