namespace dotMigrata.Core.Values.Interfaces;

/// <summary>
/// Defines a transformation function for normalizing factor values.
/// </summary>
/// <remarks>
///     <para>
///     Transform functions convert raw factor values to the 0-1 normalized range.
///     Different transforms emphasize different parts of the value range.
///     </para>
///     <para>
///     Implementations must be thread-safe and deterministic (same input always produces same output).
///     </para>
/// </remarks>
public interface ITransformFunction
{
    /// <summary>
    /// Gets the name of this transform function for identification and display.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Transforms a value from the specified range to the 0-1 normalized range.
    /// </summary>
    /// <param name="value">The value to transform (already clamped to the range).</param>
    /// <param name="min">The minimum value of the range.</param>
    /// <param name="max">The maximum value of the range.</param>
    /// <returns>A normalized value between 0 and 1.</returns>
    /// <remarks>
    /// The <paramref name="value" /> is guaranteed to be within [<paramref name="min" />, <paramref name="max" />].
    /// Implementations should not clamp again but can assume the value is pre-clamped.
    /// </remarks>
    double Transform(double value, double min, double max);
}