namespace dotMigrata.Core.Values.Attributes;

/// <summary>
/// Specifies the valid range for a value specification property.
/// Used for validation and constraint enforcement.
/// </summary>
/// <remarks>
/// This attribute can be applied to properties that store <see cref="ValueSpec" /> instances
/// to define their acceptable value range. The validation is performed when the value is evaluated.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ValueRangeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueRangeAttribute" /> class.
    /// </summary>
    /// <param name="min">The minimum allowed value (inclusive).</param>
    /// <param name="max">The maximum allowed value (inclusive).</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="min" /> is greater than <paramref name="max" />.</exception>
    public ValueRangeAttribute(double min, double max)
    {
        if (min > max)
            throw new ArgumentException($"Minimum value ({min}) must be less than or equal to maximum value ({max}).");

        Min = min;
        Max = max;
    }

    /// <summary>
    /// Gets the minimum allowed value (inclusive).
    /// </summary>
    public double Min { get; }

    /// <summary>
    /// Gets the maximum allowed value (inclusive).
    /// </summary>
    public double Max { get; }

    /// <summary>
    /// Gets or sets a value indicating whether negative values are allowed.
    /// Default is true (negative values allowed if within Min/Max range).
    /// </summary>
    public bool AllowNegative { get; init; } = true;
}