using dotGeoMigrata.Core.Enums;

namespace dotGeoMigrata.Core.Values;

/// <summary>
/// Defines a factor including its transformation type, direction, and normalization rule.
/// Factors represent measurable characteristics of cities that influence migration decisions.
/// </summary>
public record FactorDefinition
{
    private readonly ValueRange _range;

    /// <summary>
    /// Gets or initializes the display name of the factor.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets or initializes the factor type (Positive or Negative).
    /// Determines whether high values attract or repel population.
    /// </summary>
    public required FactorType Type { get; init; }

    /// <summary>
    /// Gets or initializes the minimum value for normalization.
    /// Values below this will be clamped to this minimum.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the value is NaN or Infinity.</exception>
    public required double MinValue
    {
        get => _range.Min;
        init
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("MinValue must be a valid number.", nameof(MinValue));
            _range = new ValueRange(value, _range.Max);
        }
    }

    /// <summary>
    /// Gets or initializes the maximum value for normalization.
    /// Values above this will be clamped to this maximum.
    /// Must be greater than MinValue.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the value is NaN, Infinity, or not greater than MinValue.</exception>
    public required double MaxValue
    {
        get => _range.Max;
        init
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("MaxValue must be a valid number.", nameof(MaxValue));
            if (value <= MinValue)
                throw new ArgumentException("MaxValue must be greater than MinValue.", nameof(MaxValue));
            _range = new ValueRange(_range.Min, value);
        }
    }

    /// <summary>
    /// Gets the value range for this factor.
    /// </summary>
    public ValueRange Range => _range;

    /// <summary>
    /// Gets or initializes the optional transformation type for normalization.
    /// When null, linear normalization is used.
    /// </summary>
    public required TransformType? Transform { get; init; }

    /// <summary>
    /// Normalizes a raw value to the 0-1 range using this factor's transformation.
    /// </summary>
    /// <param name="rawValue">The raw value to normalize.</param>
    /// <returns>A normalized value between 0 and 1.</returns>
    internal double Normalize(double rawValue)
    {
        return _range.Normalize(rawValue, Transform);
    }
}