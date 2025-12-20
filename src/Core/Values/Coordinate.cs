using dotMigrata.Core.Values.Interfaces;

namespace dotMigrata.Core.Values;

/// <summary>
/// Represents a geographic coordinate using WGS84 datum (latitude and longitude).
/// </summary>
/// <remarks>
/// Provides distance calculation using pluggable <see cref="Interfaces.IDistanceCalculator" /> strategies.
/// Defaults to Haversine formula for backward compatibility and general use.
/// </remarks>
public readonly record struct Coordinate
{
    /// <summary>
    /// Gets or sets the default distance calculator (Haversine by default).
    /// </summary>
    /// <remarks>
    /// This property uses thread-safe operations for updates.
    /// For better design, prefer passing IDistanceCalculator via dependency injection or method parameters.
    /// </remarks>
    public static IDistanceCalculator DefaultDistanceCalculator
    {
        get => Volatile.Read(ref field);
        set => Volatile.Write(ref field, value ?? throw new ArgumentNullException(nameof(value)));
    } = HaversineDistanceCalculator.Instance;

    /// <summary>
    /// Gets or initializes the longitude in degrees.
    /// Valid range: -180 to 180.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when longitude is outside the valid range.</exception>
    public required double Longitude
    {
        get;
        init => field = value is >= -180 and <= 180
            ? value
            : throw new ArgumentException("Longitude must be between -180 and 180 degrees.", nameof(value));
    }

    /// <summary>
    /// Gets or initializes the latitude in degrees.
    /// Valid range: -90 to 90.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when latitude is outside the valid range.</exception>
    public required double Latitude
    {
        get;
        init => field = value is >= -90 and <= 90
            ? value
            : throw new ArgumentException("Latitude must be between -90 and 90 degrees.", nameof(value));
    }

    /// <summary>
    /// Computes the distance between this coordinate and another using the default distance calculator.
    /// </summary>
    /// <param name="other">Other coordinate to calculate distance to.</param>
    /// <returns>Distance in kilometers.</returns>
    public double DistanceTo(Coordinate other)
    {
        return DefaultDistanceCalculator.CalculateDistance(this, other);
    }

    /// <summary>
    /// Computes the distance between this coordinate and another using a specific distance calculator.
    /// </summary>
    /// <param name="other">Other coordinate to calculate distance to.</param>
    /// <param name="calculator">The distance calculator to use.</param>
    /// <returns>Distance in kilometers.</returns>
    public double DistanceTo(Coordinate other, IDistanceCalculator calculator)
    {
        ArgumentNullException.ThrowIfNull(calculator);
        return calculator.CalculateDistance(this, other);
    }

    /// <summary>
    /// Computes the distance between two coordinates using the default distance calculator.
    /// </summary>
    /// <param name="c1">First coordinate.</param>
    /// <param name="c2">Second coordinate.</param>
    /// <returns>Distance in kilometers.</returns>
    public static double DistanceBetween(Coordinate c1, Coordinate c2)
    {
        return DefaultDistanceCalculator.CalculateDistance(c1, c2);
    }

    /// <summary>
    /// Computes the distance between two coordinates using a specific distance calculator.
    /// </summary>
    /// <param name="c1">First coordinate.</param>
    /// <param name="c2">Second coordinate.</param>
    /// <param name="calculator">The distance calculator to use.</param>
    /// <returns>Distance in kilometers.</returns>
    public static double DistanceBetween(Coordinate c1, Coordinate c2, IDistanceCalculator calculator)
    {
        ArgumentNullException.ThrowIfNull(calculator);
        return calculator.CalculateDistance(c1, c2);
    }
}