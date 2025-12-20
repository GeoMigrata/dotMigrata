namespace dotMigrata.Core.Values.Interfaces;

/// <summary>
/// Defines a strategy for calculating distances between geographic coordinates.
/// </summary>
/// <remarks>
/// This interface allows for different distance calculation algorithms to be plugged in,
/// such as Haversine (great-circle), Vincenty (geodesic), or custom projection-based calculations.
/// Enables future extensibility for different coordinate systems and accuracy requirements.
/// </remarks>
public interface IDistanceCalculator
{
    /// <summary>
    /// Gets the name of this distance calculation method.
    /// </summary>
    /// <remarks>
    /// Used for logging, diagnostics, and snapshot metadata.
    /// Examples: "Haversine", "Vincenty", "Euclidean", "Custom"
    /// </remarks>
    string MethodName { get; }

    /// <summary>
    /// Gets whether this calculator is suitable for short distances (less than 100km).
    /// </summary>
    /// <remarks>
    /// Some algorithms are optimized for short distances while others are better for long distances.
    /// This helps with algorithm selection and validation.
    /// </remarks>
    bool SuitableForShortDistances { get; }

    /// <summary>
    /// Gets whether this calculator is suitable for long distances (greater than 1000km).
    /// </summary>
    /// <remarks>
    /// High-accuracy algorithms like Vincenty are better for long distances.
    /// Simple algorithms like Haversine are acceptable for most cases but less accurate for very long distances.
    /// </remarks>
    bool SuitableForLongDistances { get; }

    /// <summary>
    /// Calculates the distance between two geographic coordinates.
    /// </summary>
    /// <param name="from">The starting coordinate.</param>
    /// <param name="to">The destination coordinate.</param>
    /// <returns>The distance in kilometers.</returns>
    /// <exception cref="ArgumentException">Thrown when coordinates are invalid.</exception>
    double CalculateDistance(Coordinate from, Coordinate to);
}