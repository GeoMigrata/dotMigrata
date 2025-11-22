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
    /// Initializes a new instance of the IntensityValue struct.
    /// </summary>
    /// <param name="value">The intensity value. Must be non-negative.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is NaN, Infinity, or negative.</exception>
    private IntensityValue(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be a valid number.");

        if (value < MinAllowedValue)
            throw new ArgumentOutOfRangeException(nameof(value),
                $"Intensity value must be non-negative (>= {MinAllowedValue}).");

        Value = value;
    }

    /// <summary>
    /// Gets the raw intensity value.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Validates that this intensity value is valid (non-negative, not NaN, not Infinity).
    /// </summary>
    /// <returns>True if the value is valid; otherwise, false.</returns>
    public bool IsValid() => Value >= MinAllowedValue &&
                             !double.IsNaN(Value) &&
                             !double.IsInfinity(Value);

    /// <summary>
    /// Creates an IntensityValue from a raw value.
    /// </summary>
    /// <param name="value">The raw intensity value. Must be non-negative.</param>
    /// <returns>A new IntensityValue.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is NaN, Infinity, or negative.</exception>
    public static IntensityValue FromRaw(double value) => new(value);

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
}