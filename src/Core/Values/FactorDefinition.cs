using dotMigrata.Core.Enums;
using dotMigrata.Core.Values.Interfaces;

namespace dotMigrata.Core.Values;

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
    /// Must be non-negative (>= 0).
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the value is NaN, Infinity, or negative.</exception>
    public required double MinValue
    {
        get => _range.Min;
        init
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("MinValue must be a valid number.", nameof(MinValue));
            if (value < 0)
                throw new ArgumentException("MinValue must be non-negative (>= 0).", nameof(MinValue));
            _range = new ValueRange(value, _range.Max);
        }
    }

    /// <summary>
    /// Gets or initializes the maximum value for normalization.
    /// Values above this will be clamped to this maximum.
    /// Must be greater than MinValue and non-negative.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the value is NaN, Infinity, not greater than MinValue, or negative.</exception>
    public required double MaxValue
    {
        get => _range.Max;
        init
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("MaxValue must be a valid number.", nameof(MaxValue));
            if (value < 0)
                throw new ArgumentException("MaxValue must be non-negative (>= 0).", nameof(MaxValue));
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
    /// <remarks>
    /// For backward compatibility with enum-based transforms.
    /// Prefer using <see cref="TransformFunction"/> for extensibility.
    /// </remarks>
    public TransformType? Transform { get; init; }

    /// <summary>
    /// Gets or initializes the custom transformation function for normalization.
    /// When set, this takes precedence over <see cref="Transform" />.
    /// </summary>
    /// <remarks>
    /// Allows custom normalization functions beyond the built-in enum types.
    /// Use <see cref="TransformFunctions" /> for built-in implementations or provide your own.
    /// </remarks>
    public ITransformFunction? TransformFunction { get; init; }

    /// <summary>
    /// Normalizes a raw value to the 0-1 range using this factor's transformation.
    /// </summary>
    /// <param name="rawValue">The raw value to normalize.</param>
    /// <returns>A normalized value between 0 and 1.</returns>
    internal double Normalize(double rawValue)
    {
        // Custom transform function takes precedence
        if (TransformFunction == null)
            return _range.Normalize(rawValue, Transform);

        var clamped = Math.Clamp(rawValue, _range.Min, _range.Max);
        return TransformFunction.Transform(clamped, _range.Min, _range.Max);

        // Fall back to enum-based transform for backward compatibility
    }
}