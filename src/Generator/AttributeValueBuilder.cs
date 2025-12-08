using dotMigrata.Core.Validation;

namespace dotMigrata.Generator;

/// <summary>
/// Provides a fluent, strongly-typed way to define value specifications for person attributes.
/// Use <c>using static dotMigrata.Generator.AttributeValueBuilder;</c> to enable named attribute methods.
/// </summary>
public static class AttributeValueBuilder
{
    /// <summary>
    /// Creates a builder for a named attribute.
    /// </summary>
    /// <param name="name">The attribute name. Must not be null or empty.</param>
    /// <returns>An attribute value builder for the specified attribute.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or whitespace.</exception>
    public static AttributeSpec Attribute(string name)
    {
        Guard.ThrowIfNullOrWhiteSpace(name);
        return new AttributeSpec(name);
    }

    /// <summary>
    /// Creates a builder for an Age attribute.
    /// </summary>
    /// <returns>An attribute value builder for Age.</returns>
    public static AttributeSpec Age() => new("Age");

    /// <summary>
    /// Creates a builder for an Income attribute.
    /// </summary>
    /// <returns>An attribute value builder for Income.</returns>
    public static AttributeSpec Income() => new("Income");

    /// <summary>
    /// Creates a builder for an Education attribute.
    /// </summary>
    /// <returns>An attribute value builder for Education.</returns>
    public static AttributeSpec Education() => new("Education");

    /// <summary>
    /// Creates a builder for a RiskPreference attribute.
    /// </summary>
    /// <returns>An attribute value builder for RiskPreference.</returns>
    public static AttributeSpec RiskPreference() => new("RiskPreference");

    /// <summary>
    /// Creates a builder for a MovingWillingness attribute.
    /// </summary>
    /// <returns>An attribute value builder for MovingWillingness.</returns>
    public static AttributeSpec MovingWillingness() => new("MovingWillingness");

    /// <summary>
    /// Creates a builder for a RetentionRate attribute.
    /// </summary>
    /// <returns>An attribute value builder for RetentionRate.</returns>
    public static AttributeSpec RetentionRate() => new("RetentionRate");

    /// <summary>
    /// Creates a builder for a SensitivityScaling attribute.
    /// </summary>
    /// <returns>An attribute value builder for SensitivityScaling.</returns>
    public static AttributeSpec SensitivityScaling() => new("SensitivityScaling");

    /// <summary>
    /// Creates a builder for an AttractionThreshold attribute.
    /// </summary>
    /// <returns>An attribute value builder for AttractionThreshold.</returns>
    public static AttributeSpec AttractionThreshold() => new("AttractionThreshold");

    /// <summary>
    /// Represents a named attribute for which a value specification can be created.
    /// </summary>
    /// <param name="Name">The attribute name.</param>
    public readonly record struct AttributeSpec(string Name)
    {
        /// <summary>
        /// Creates a fixed value specification for this attribute.
        /// </summary>
        /// <param name="value">The fixed value.</param>
        /// <returns>A value specification with the fixed value.</returns>
        public static ValueSpecification Fixed(double value) => ValueSpecification.Fixed(value);

        /// <summary>
        /// Creates a ranged value specification for this attribute.
        /// Values will be uniformly distributed between min and max.
        /// </summary>
        /// <param name="min">The inclusive minimum value.</param>
        /// <param name="max">The inclusive maximum value. Must be greater than or equal to <paramref name="min"/>.</param>
        /// <returns>A value specification with the specified range.</returns>
        /// <exception cref="Core.Exceptions.GeneratorSpecificationException">Thrown when <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
        public static ValueSpecification InRange(double min, double max) => ValueSpecification.InRange(min, max);

        /// <summary>
        /// Creates an approximate value specification for this attribute using a normal distribution.
        /// Values will be sampled from a normal distribution centered at the mean with the specified standard deviation.
        /// </summary>
        /// <param name="mean">The mean value (center of the distribution).</param>
        /// <param name="standardDeviation">The standard deviation. Must be positive.</param>
        /// <returns>A value specification representing an approximate value.</returns>
        /// <exception cref="Core.Exceptions.GeneratorSpecificationException">Thrown when <paramref name="standardDeviation"/> is not positive.</exception>
        public static ValueSpecification Approximately(double mean, double standardDeviation) =>
            ValueSpecification.Approximately(mean, standardDeviation);

        /// <summary>
        /// Creates a random value specification for this attribute using the default range and an optional scale.
        /// The scale affects the distribution: values greater than 1.0 bias toward higher values,
        /// less than 1.0 bias toward lower values.
        /// </summary>
        /// <param name="scale">The scale factor to apply to the random value. Must be non-negative. Defaults to 1.0.</param>
        /// <returns>A value specification representing a random value.</returns>
        /// <exception cref="Core.Exceptions.GeneratorSpecificationException">Thrown when <paramref name="scale"/> is negative.</exception>
        public static ValueSpecification Random(double scale = 1.0) => ValueSpecification.RandomWithScale(scale);
    }
}