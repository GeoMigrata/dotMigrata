using dotMigrata.Core.Exceptions;
using dotMigrata.Core.Values;

namespace dotMigrata.Generator;

/// <summary>
/// Represents a value specification for a person attribute or factor sensitivity.
/// Can be a fixed value, a random range, or an approximate value using normal distribution.
/// </summary>
/// <remarks>
/// This class is deprecated. Use <see cref="ValueSpec"/> instead.
/// This type is maintained for backward compatibility and will be removed in a future version.
/// </remarks>
[Obsolete("Use ValueSpec from dotMigrata.Core.Values instead. This type will be removed in a future version.")]
public sealed record ValueSpecification
{
    private readonly double? _fixedValue;
    private readonly double? _mean;
    private readonly (double Min, double Max)? _range;
    private readonly double? _standardDeviation;

    private ValueSpecification(
        double? fixedValue,
        (double, double)? range,
        double scale,
        double? mean,
        double? standardDeviation)
    {
        _fixedValue = fixedValue;
        _range = range;
        Scale = scale;
        _mean = mean;
        _standardDeviation = standardDeviation;
    }

    /// <summary>
    /// Gets whether this is a fixed value.
    /// </summary>
    public bool IsFixed => _fixedValue.HasValue;

    /// <summary>
    /// Gets whether this has a custom range.
    /// </summary>
    public bool HasRange => _range.HasValue;

    /// <summary>
    /// Gets whether this is an approximate value using normal distribution.
    /// </summary>
    public bool IsApproximate => _mean.HasValue && _standardDeviation.HasValue;

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
    /// Values greater than 1.0 tend toward higher values, less than 1.0 toward lower values.
    /// </summary>
    public double Scale { get; init; }

    /// <summary>
    /// Gets the mean value for approximate specifications.
    /// </summary>
    public double? Mean => _mean;

    /// <summary>
    /// Gets the standard deviation for approximate specifications.
    /// </summary>
    public double? StandardDeviation => _standardDeviation;

    /// <summary>
    /// Creates a fixed value specification.
    /// </summary>
    /// <param name="value">The fixed value.</param>
    /// <returns>A value specification with a fixed value.</returns>
    public static ValueSpecification Fixed(double value)
    {
        return new ValueSpecification(value, null, 1.0, null, null);
    }

    /// <summary>
    /// Creates a random value specification with a range.
    /// </summary>
    /// <param name="min">Minimum value (inclusive).</param>
    /// <param name="max">Maximum value (inclusive). Must be greater than or equal to <paramref name="min" />.</param>
    /// <returns>A value specification with a random range.</returns>
    /// <exception cref="GeneratorSpecificationException">
    /// Thrown when <paramref name="min" /> is greater than
    /// <paramref name="max" />.
    /// </exception>
    public static ValueSpecification InRange(double min, double max)
    {
        return min > max
            ? throw new GeneratorSpecificationException(
                $"Minimum value ({min}) must be less than or equal to maximum value ({max}).")
            : new ValueSpecification(null, (min, max), 1.0, null, null);
    }

    /// <summary>
    /// Creates a value specification that represents an approximate value using a normal distribution.
    /// Values will be sampled from a normal distribution with the specified mean and standard deviation.
    /// </summary>
    /// <param name="mean">The mean value (center of the distribution).</param>
    /// <param name="standardDeviation">The standard deviation. Must be positive.</param>
    /// <returns>A value specification representing an approximate value.</returns>
    /// <exception cref="GeneratorSpecificationException">Thrown when <paramref name="standardDeviation" /> is not positive.</exception>
    public static ValueSpecification Approximately(double mean, double standardDeviation)
    {
        return standardDeviation <= 0
            ? throw new GeneratorSpecificationException(
                $"Standard deviation must be positive. Got: {standardDeviation}")
            : new ValueSpecification(null, null, 1.0, mean, standardDeviation);
    }

    /// <summary>
    /// Creates a random value specification with a scale.
    /// The scale affects the distribution: values greater than 1.0 bias toward higher values,
    /// less than 1.0 bias toward lower values.
    /// </summary>
    /// <param name="scale">Scale factor to apply to the random value. Must be non-negative.</param>
    /// <returns>A value specification with default range and scale.</returns>
    /// <exception cref="GeneratorSpecificationException">Thrown when <paramref name="scale" /> is negative.</exception>
    public static ValueSpecification RandomWithScale(double scale)
    {
        return scale < 0
            ? throw new GeneratorSpecificationException($"Scale must be non-negative. Got: {scale}")
            : new ValueSpecification(null, null, scale, null, null);
    }

    /// <summary>
    /// Creates a value specification for default random generation (no range, no scale).
    /// </summary>
    /// <returns>A value specification for default random generation.</returns>
    public static ValueSpecification Random()
    {
        return new ValueSpecification(null, null, 1.0, null, null);
    }

    /// <summary>
    /// Applies a scale to this specification, creating a biased random value.
    /// The scale affects the distribution: values greater than 1.0 bias toward higher values,
    /// less than 1.0 bias toward lower values.
    /// </summary>
    /// <param name="scale">Scale factor to multiply the generated value by. Must be non-negative.</param>
    /// <returns>A new value specification with the scale applied.</returns>
    /// <exception cref="GeneratorSpecificationException">Thrown when <paramref name="scale" /> is negative.</exception>
    public ValueSpecification WithScale(double scale)
    {
        return scale < 0
            ? throw new GeneratorSpecificationException($"Scale must be non-negative. Got: {scale}")
            : this with { Scale = scale };
    }
}