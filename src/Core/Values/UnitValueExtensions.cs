namespace dotMigrata.Core.Values;

/// <summary>
/// Extension methods for creating <see cref="UnitValue" /> values and specifications.
/// Provides a fluent API for intuitive value creation.
/// </summary>
public static class UnitValueExtensions
{
    /// <param name="value">The ratio value between 0 and 1.</param>
    extension(double value)
    {
        /// <summary>
        /// Converts a double to a <see cref="UnitValue" />, treating it as a ratio (0-1).
        /// Values outside the range will be clamped.
        /// </summary>
        /// <returns>A new <see cref="UnitValue" />.</returns>
        public UnitValue AsNormalized()
        {
            return UnitValue.FromRatio(value);
        }

        /// <summary>
        /// Converts a double to a <see cref="UnitValue" />, treating it as a percentage (0-100).
        /// Values outside the range will be clamped to [0, 1].
        /// </summary>
        /// <returns>A new <see cref="UnitValue" />.</returns>
        public UnitValue AsPercentage()
        {
            return UnitValue.FromPercentage(value);
        }

        /// <summary>
        /// Creates a fixed <see cref="UnitValuePromise" /> from this value.
        /// </summary>
        /// <returns>A new <see cref="UnitValuePromise" /> with a fixed value.</returns>
        public UnitValuePromise AsSpec()
        {
            return UnitValuePromise.Fixed(value);
        }
    }

    /// <param name="value">The percentage value between 0 and 100.</param>
    extension(int value)
    {
        /// <summary>
        /// Converts an int to a <see cref="UnitValue" />, treating it as a percentage (0-100).
        /// Values outside the range will be clamped to [0, 1].
        /// </summary>
        /// <returns>A new <see cref="UnitValue" />.</returns>
        public UnitValue AsPercentage()
        {
            return UnitValue.FromPercentage(value);
        }

        /// <summary>
        /// Creates a fixed <see cref="UnitValuePromise" /> from this value.
        /// </summary>
        /// <returns>A new <see cref="UnitValuePromise" /> with a fixed value.</returns>
        public UnitValuePromise AsSpec()
        {
            return UnitValuePromise.Fixed(value);
        }
    }
}