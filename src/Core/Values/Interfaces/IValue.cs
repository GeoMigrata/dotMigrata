namespace dotMigrata.Core.Values;

/// <summary>
/// Represents a typed value with validation and conversion capabilities.
/// Provides a common interface for all value types in the system.
/// </summary>
/// <typeparam name="T">The underlying value type.</typeparam>
public interface IValue<out T> where T : struct
{
    /// <summary>
    /// Gets the raw underlying value.
    /// </summary>
    T Value { get; }

    /// <summary>
    /// Validates that the value is within acceptable bounds.
    /// </summary>
    /// <returns>True if the value is valid; otherwise, false.</returns>
    bool IsValid();
}