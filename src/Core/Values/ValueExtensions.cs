namespace dotMigrata.Core.Values;

/// <summary>
/// Extension methods for creating and working with typed values.
/// Provides a fluent API for intuitive value creation.
/// </summary>
public static class ValueExtensions
{
    /// <param name="value">The ratio value between 0 and 1.</param>
    extension(double value)
    {
        /// <summary>
        /// Converts a double to a NormalizedValue, treating it as a ratio (0-1).
        /// </summary>
        /// <returns>A new NormalizedValue.</returns>
        public NormalizedValue AsRatio()
        {
            return NormalizedValue.FromRatio(value);
        }

        /// <summary>
        /// Converts a double to a NormalizedValue, treating it as a percentage (0-100).
        /// </summary>
        /// <returns>A new NormalizedValue.</returns>
        public NormalizedValue AsPercentage()
        {
            return NormalizedValue.FromPercentage(value);
        }

        /// <summary>
        /// Creates a fixed ValueSpec from this value.
        /// </summary>
        /// <returns>A new ValueSpec with a fixed value.</returns>
        public ValueSpec AsValueSpec()
        {
            return ValueSpec.Fixed(value);
        }
    }

    /// <param name="value">The percentage value between 0 and 100.</param>
    extension(int value)
    {
        /// <summary>
        /// Converts an int to a NormalizedValue, treating it as a percentage (0-100).
        /// </summary>
        /// <returns>A new NormalizedValue.</returns>
        public NormalizedValue AsPercentage()
        {
            return NormalizedValue.FromPercentage(value);
        }

        /// <summary>
        /// Creates a fixed ValueSpec from this value.
        /// </summary>
        /// <returns>A new IntensityValue.</returns>
        /// <returns>A new ValueSpec with a fixed value.</returns>
        public ValueSpec AsValueSpec()
        {
            return ValueSpec.Fixed(value);
        }
    }
}