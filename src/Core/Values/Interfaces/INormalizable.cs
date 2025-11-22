namespace dotMigrata.Core.Values;

/// <summary>
/// Represents a value that can be normalized to the 0-1 range.
/// </summary>
public interface INormalizable
{
    /// <summary>
    /// Normalizes this value to a 0-1 range.
    /// </summary>
    /// <returns>A normalized value between 0 and 1.</returns>
    NormalizedValue Normalize();
}