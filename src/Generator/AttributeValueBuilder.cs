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
    /// <exception cref="ArgumentException">Thrown when <paramref name="name" /> is null or whitespace.</exception>
    public static AttributeSpec Attribute(string name)
    {
        Guard.ThrowIfNullOrWhiteSpace(name);
        return new AttributeSpec(name);
    }

    /// <summary>
    /// Creates a builder for an Age attribute.
    /// </summary>
    /// <returns>An attribute value builder for Age.</returns>
    public static AttributeSpec Age()
    {
        return new AttributeSpec("Age");
    }

    /// <summary>
    /// Creates a builder for an Income attribute.
    /// </summary>
    /// <returns>An attribute value builder for Income.</returns>
    public static AttributeSpec Income()
    {
        return new AttributeSpec("Income");
    }

    /// <summary>
    /// Creates a builder for an Education attribute.
    /// </summary>
    /// <returns>An attribute value builder for Education.</returns>
    public static AttributeSpec Education()
    {
        return new AttributeSpec("Education");
    }

    /// <summary>
    /// Creates a builder for a RiskPreference attribute.
    /// </summary>
    /// <returns>An attribute value builder for RiskPreference.</returns>
    public static AttributeSpec RiskPreference()
    {
        return new AttributeSpec("RiskPreference");
    }

    /// <summary>
    /// Creates a builder for a MovingWillingness attribute.
    /// </summary>
    /// <returns>An attribute value builder for MovingWillingness.</returns>
    public static AttributeSpec MovingWillingness()
    {
        return new AttributeSpec("MovingWillingness");
    }

    /// <summary>
    /// Creates a builder for a RetentionRate attribute.
    /// </summary>
    /// <returns>An attribute value builder for RetentionRate.</returns>
    public static AttributeSpec RetentionRate()
    {
        return new AttributeSpec("RetentionRate");
    }

    /// <summary>
    /// Creates a builder for a SensitivityScaling attribute.
    /// </summary>
    /// <returns>An attribute value builder for SensitivityScaling.</returns>
    public static AttributeSpec SensitivityScaling()
    {
        return new AttributeSpec("SensitivityScaling");
    }

    /// <summary>
    /// Creates a builder for an AttractionThreshold attribute.
    /// </summary>
    /// <returns>An attribute value builder for AttractionThreshold.</returns>
    public static AttributeSpec AttractionThreshold()
    {
        return new AttributeSpec("AttractionThreshold");
    }

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
        public static ValueSpecification Fixed(double value)
        {
            return ValueSpecification.Fixed(value);
        }

        /// <summary>
        /// Creates a ranged value specification for this attribute.
        /// Values will be uniformly distributed between min and max.
        /// </summary>
        /// <param name="min">The inclusive minimum value.</param>
        /// <param name="max">The inclusive maximum value. Must be greater than or equal to <paramref name="min" />.</param>
        /// <returns>A value specification with the specified range.</returns>
        /// <exception cref="Core.Exceptions.GeneratorSpecificationException">
        /// Thrown when <paramref name="min" /> is greater than
        /// <paramref name="max" />.
        /// </exception>
        public static ValueSpecification InRange(double min, double max)
        {
            return ValueSpecification.InRange(min, max);
        }

        /// <summary>
        /// Creates an approximate value specification for this attribute using a normal distribution.
        /// Values will be sampled from a normal distribution centered at the mean with the specified standard deviation.
        /// </summary>
        /// <param name="mean">The mean value (center of the distribution).</param>
        /// <param name="standardDeviation">The standard deviation. Must be positive.</param>
        /// <returns>A value specification representing an approximate value.</returns>
        /// <exception cref="Core.Exceptions.GeneratorSpecificationException">
        /// Thrown when <paramref name="standardDeviation" /> is
        /// not positive.
        /// </exception>
        public static ValueSpecification Approximately(double mean, double standardDeviation)
        {
            return ValueSpecification.Approximately(mean, standardDeviation);
        }

        /// <summary>
        /// Creates a random value specification for this attribute using the default range and an optional scale.
        /// The scale affects the distribution: values greater than 1.0 bias toward higher values,
        /// less than 1.0 bias toward lower values.
        /// </summary>
        /// <param name="scale">The scale factor to apply to the random value. Must be non-negative. Defaults to 1.0.</param>
        /// <returns>A value specification representing a random value.</returns>
        /// <exception cref="Core.Exceptions.GeneratorSpecificationException">Thrown when <paramref name="scale" /> is negative.</exception>
        public static ValueSpecification Random(double scale = 1.0)
        {
            return ValueSpecification.RandomWithScale(scale);
        }
    }
}