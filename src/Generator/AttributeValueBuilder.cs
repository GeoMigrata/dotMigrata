namespace dotMigrata.Generator;

/// <summary>
/// Provides a fluent, strongly-typed way to define value specifications for person attributes.
/// </summary>
public static class AttributeValueBuilder
{
    /// <summary>
    /// Creates a builder for a named attribute.
    /// </summary>
    /// <param name="name">The attribute name. Must not be null or empty.</param>
    /// <returns>An attribute value builder for the specified attribute.</returns>
    public static AttributeSpec Attribute(string name) =>
        string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Attribute name must not be null or empty. ", nameof(name))
            : new AttributeSpec(name);

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
        public ValueSpecification Fixed(double value) => ValueSpecification.Fixed(value);

        /// <summary>
        /// Creates a ranged value specification for this attribute.
        /// </summary>
        /// <param name="min">The inclusive minimum value.</param>
        /// <param name="max">The inclusive maximum value. Must be greater than or equal to <paramref name="min"/>.</param>
        /// <returns>A value specification with the specified range.</returns>
        public ValueSpecification InRange(double min, double max) => ValueSpecification.InRange(min, max);

        /// <summary>
        /// Creates an approximate value specification for this attribute using a normal distribution.
        /// </summary>
        /// <param name="mean">The mean value.</param>
        /// <param name="standardDeviation">The standard deviation. Must be positive.</param>
        /// <returns>A value specification representing an approximate value.</returns>
        public ValueSpecification Approximately(double mean, double standardDeviation) =>
            ValueSpecification.Approximately(mean, standardDeviation);

        /// <summary>
        /// Creates a random value specification for this attribute using the default range and an optional scale.
        /// </summary>
        /// <param name="scale">The scale factor to apply to the random value. Must be non-negative. Defaults to 1.0.</param>
        /// <returns>A value specification representing a random value.</returns>
        public ValueSpecification Random(double scale = 1.0) => ValueSpecification.RandomWithScale(scale);
    }
}