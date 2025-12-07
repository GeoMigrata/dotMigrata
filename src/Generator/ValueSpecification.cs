namespace dotMigrata.Generator;

/// <summary>
/// Represents a value specification for a person attribute or factor sensitivity.
/// Can be a fixed value, a random range, a normal distribution, or a random value with scale.
/// </summary>
public sealed class ValueSpecification
{
    private readonly double? _fixedValue;
    private readonly (double Min, double Max)? _range;

    private ValueSpecification(double? fixedValue, (double, double)? range, double scale, double? mean,
        double? standardDeviation) =>
        (_fixedValue, _range, Scale, Mean, StandardDeviation) = (fixedValue, range, scale, mean, standardDeviation);

    /// <summary>
    /// Gets whether this is a fixed value.
    /// </summary>
    public bool IsFixed => _fixedValue.HasValue;

    /// <summary>
    /// Gets whether this has a custom range.
    /// </summary>
    public bool HasRange => _range.HasValue;

    /// <summary>
    /// Gets the fixed value if this is a fixed specification.
    /// </summary>
    public double? FixedValue => _fixedValue;

    /// <summary>
    /// Gets the range if this has a custom range.
    /// </summary>
    public (double Min, double Max)? Range => _range;

    /// <summary>
    /// Gets the scale factor to apply to generated values.
    /// </summary>
    public double Scale { get; }

    /// <summary>
    /// Gets the mean value for approximate specifications.
    /// </summary>
    public double? Mean { get; }

    /// <summary>
    /// Gets the standard deviation for approximate specifications.
    /// </summary>
    public double? StandardDeviation { get; }

    /// <summary>
    /// Creates a fixed value specification.
    /// </summary>
    /// <param name="value">The fixed value.</param>
    /// <returns>A value specification with a fixed value.</returns>
    public static ValueSpecification Fixed(double value) =>
        new(value, null, 1.0, null, null);

    /// <summary>
    /// Creates a random value specification with a range.
    /// </summary>
    /// <param name="min">Minimum value (inclusive).</param>
    /// <param name="max">Maximum value (inclusive).</param>
    /// <returns>A value specification with a random range.</returns>
    public static ValueSpecification InRange(double min, double max) =>
        min > max
            ? throw new ArgumentOutOfRangeException(nameof(min),
                "Minimum value must be less than or equal to maximum value.")
            : new ValueSpecification(null, (min, max), 1.0, null, null);

    /// <summary>
    /// Creates a value specification that represents an approximate value using a normal distribution.
    /// </summary>
    /// <param name="mean">The mean value.</param>
    /// <param name="standardDeviation">The standard deviation. Must be positive.</param>
    /// <returns>A value specification representing an approximate value.</returns>
    public static ValueSpecification Approximately(double mean, double standardDeviation) =>
        standardDeviation <= 0
            ? throw new ArgumentOutOfRangeException(nameof(standardDeviation), "Standard deviation must be positive.")
            : new ValueSpecification(null, null, 1.0, mean, standardDeviation);

    /// <summary>
    /// Creates a random value specification with a scale.
    /// </summary>
    /// <param name="scale">Scale factor to apply to the random value. Must be non-negative.</param>
    /// <returns>A value specification with default range and scale.</returns>
    public static ValueSpecification RandomWithScale(double scale) =>
        scale < 0
            ? throw new ArgumentOutOfRangeException(nameof(scale), "Scale must be non-negative.")
            : new ValueSpecification(null, null, scale, null, null);

    /// <summary>
    /// Creates a value specification for default random generation (no range, no scale).
    /// </summary>
    /// <returns>A value specification for default random generation.</returns>
    public static ValueSpecification Random() => new(null, null, 1.0, null, null);

    /// <summary>
    /// Applies a scale to this specification, creating a biased random value.
    /// </summary>
    /// <param name="scale">Scale factor to multiply the generated value by. Must be non-negative.</param>
    /// <returns>A new value specification with the scale applied.</returns>
    public ValueSpecification WithScale(double scale) =>
        scale < 0
            ? throw new ArgumentOutOfRangeException(nameof(scale), "Scale must be non-negative.")
            : new ValueSpecification(_fixedValue, _range, scale, Mean, StandardDeviation);
}