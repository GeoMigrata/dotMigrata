using dotMigrata.Core.Values.Interfaces;

namespace dotMigrata.Core.Values;

/// <summary>
/// Provides the default Haversine formula-based distance calculation for geographic coordinates.
/// </summary>
/// <remarks>
/// The Haversine formula calculates great-circle distances between points on a sphere.
/// It's suitable for most migration simulation scenarios with acceptable accuracy (±0.5%).
/// For higher accuracy over very long distances, consider Vincenty's formula.
/// </remarks>
public sealed class HaversineDistanceCalculator : IDistanceCalculator
{
    /// <summary>
    /// Earth's mean radius in kilometers (WGS84).
    /// </summary>
    private const double EarthRadiusKm = 6371.0;

    /// <summary>
    /// Gets the singleton instance of the Haversine distance calculator.
    /// </summary>
    public static HaversineDistanceCalculator Instance { get; } = new();

    /// <inheritdoc />
    public string MethodName => "Haversine";

    /// <inheritdoc />
    public bool SuitableForShortDistances => true;

    /// <inheritdoc />
    public bool SuitableForLongDistances => true;

    /// <inheritdoc />
    public double CalculateDistance(Coordinate from, Coordinate to)
    {
        var lat1Rad = DegreesToRadians(from.Latitude);
        var lat2Rad = DegreesToRadians(to.Latitude);
        var deltaLat = DegreesToRadians(to.Latitude - from.Latitude);
        var deltaLon = DegreesToRadians(to.Longitude - from.Longitude);

        var sinDeltaLatHalf = Math.Sin(deltaLat * 0.5);
        var sinDeltaLonHalf = Math.Sin(deltaLon * 0.5);

        var a = sinDeltaLatHalf * sinDeltaLatHalf +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                sinDeltaLonHalf * sinDeltaLonHalf;

        var c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}