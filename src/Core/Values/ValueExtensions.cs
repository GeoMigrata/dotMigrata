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
        public NormalizedValue AsRatio() => NormalizedValue.FromRatio(value);

        /// <summary>
        /// Converts a double to a NormalizedValue, treating it as a percentage (0-100).
        /// </summary>
        /// <returns>A new NormalizedValue.</returns>
        public NormalizedValue AsPercentage() => NormalizedValue.FromPercentage(value);

        /// <summary>
        /// Converts a double to a SensitivityValue using the default range (-10 to +10).
        /// </summary>
        /// <returns>A new SensitivityValue.</returns>
        public SensitivityValue AsSensitivity() => SensitivityValue.FromRaw(value);

        /// <summary>
        /// Converts a double to a SensitivityValue with a custom range.
        /// </summary>
        /// <param name="minValue">The minimum allowed value.</param>
        /// <param name="maxValue">The maximum allowed value.</param>
        /// <returns>A new SensitivityValue.</returns>
        public SensitivityValue AsSensitivity(double minValue, double maxValue)
            => SensitivityValue.FromRaw(value, minValue, maxValue);

        /// <summary>
        /// Converts a double to an IntensityValue.
        /// </summary>
        /// <returns>A new IntensityValue.</returns>
        public IntensityValue AsIntensity() => IntensityValue.FromRaw(value);
    }

    /// <param name="value">The percentage value between 0 and 100.</param>
    extension(int value)
    {
        /// <summary>
        /// Converts an int to a NormalizedValue, treating it as a percentage (0-100).
        /// </summary>
        /// <returns>A new NormalizedValue.</returns>
        public NormalizedValue AsPercentage() => NormalizedValue.FromPercentage(value);

        /// <summary>
        /// Converts an int to a SensitivityValue using the default range (-10 to +10).
        /// </summary>
        /// <returns>A new SensitivityValue.</returns>
        public SensitivityValue AsSensitivity() => SensitivityValue.FromRaw(value);

        /// <summary>
        /// Converts an int to an IntensityValue.
        /// </summary>
        /// <returns>A new IntensityValue.</returns>
        public IntensityValue AsIntensity() => IntensityValue.FromRaw(value);
    }
}