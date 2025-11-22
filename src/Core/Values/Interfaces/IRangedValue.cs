namespace dotMigrata.Core.Values;

/// <summary>
/// Represents a value that has a defined valid range.
/// </summary>
public interface IRangedValue : IValue<double>
{
    /// <summary>
    /// Gets the minimum allowed value.
    /// </summary>
    double MinValue { get; }

    /// <summary>
    /// Gets the maximum allowed value.
    /// </summary>
    double MaxValue { get; }

    /// <summary>
    /// Gets the range of valid values.
    /// </summary>
    ValueRange Range { get; }

    /// <summary>
    /// Checks if a value is within the valid range.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is within range; otherwise, false.</returns>
    bool Contains(double value);

    /// <summary>
    /// Clamps a value to the valid range.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <returns>The clamped value.</returns>
    double Clamp(double value);
}