namespace dotGeoMigrata.Generator;

/// <summary>
/// Represents a value specification for a person attribute or factor sensitivity.
/// Can be a fixed value, a random range, or a random range with bias (scale).
/// </summary>
public sealed class ValueSpecification
{
    private readonly double? _fixedValue;
    private readonly (double Min, double Max)? _range;

    private ValueSpecification(double? fixedValue, (double, double)? range, double scale)
    {
        _fixedValue = fixedValue;
        _range = range;
        Scale = scale;
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
    /// Gets the fixed value if this is a fixed specification.
    /// </summary>
    public double? FixedValue => _fixedValue;

    /// <summary>
    /// Gets the range if this has a custom range.
    /// </summary>
    public (double Min, double Max)? Range => _range;

    /// <summary>
    /// Gets the scale factor to apply to random values.
    /// </summary>
    public double Scale { get; }

    /// <summary>
    /// Creates a fixed value specification.
    /// </summary>
    /// <param name="value">The fixed value.</param>
    /// <returns>A value specification with a fixed value.</returns>
    public static ValueSpecification Fixed(double value)
    {
        return new ValueSpecification(value, null, 1.0);
    }

    /// <summary>
    /// Creates a random value specification with a range.
    /// </summary>
    /// <param name="min">Minimum value.</param>
    /// <param name="max">Maximum value.</param>
    /// <returns>A value specification with a random range.</returns>
    public static ValueSpecification InRange(double min, double max)
    {
        return new ValueSpecification(null, (min, max), 1.0);
    }

    /// <summary>
    /// Creates a random value specification that uses default ranges and applies a scale/bias.
    /// </summary>
    /// <param name="scale">Scale factor to apply to the random value. Must be non-negative.</param>
    /// <returns>A value specification with default range and scale.</returns>
    public static ValueSpecification RandomWithScale(double scale)
    {
        if (scale < 0)
            throw new ArgumentException("Scale must be non-negative.", nameof(scale));
        return new ValueSpecification(null, null, scale);
    }

    /// <summary>
    /// Creates a value specification for default random generation (no range, no scale).
    /// </summary>
    /// <returns>A value specification for default random generation.</returns>
    public static ValueSpecification Random()
    {
        return new ValueSpecification(null, null, 1.0);
    }

    /// <summary>
    /// Applies a scale to this specification, creating a biased random value.
    /// </summary>
    /// <param name="scale">Scale factor to multiply the generated value by.</param>
    /// <returns>A new value specification with the scale applied.</returns>
    public ValueSpecification WithScale(double scale)
    {
        if (scale < 0)
            throw new ArgumentException("Scale must be non-negative.", nameof(scale));
        return new ValueSpecification(_fixedValue, _range, scale);
    }
}