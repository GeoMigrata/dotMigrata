namespace dotGeoMigrata.Core.Domain.Values;

public readonly struct Coordinate
{
    public required double Longitude { get; init; }
    public required double Latitude { get; init; }

    /// <summary>
    /// Computes the Euclidean distance (approximate) between two coordinates using Haversine formula.
    /// </summary>
    public static double CalculateDistance(Coordinate c1, Coordinate c2)
    {
        const double r = 6371; // Earth radius in km
        var lat1Rad = DegreesToRadians(c1.Latitude);
        var lat2Rad = DegreesToRadians(c2.Latitude);
        var deltaLat = DegreesToRadians(c2.Latitude - c1.Latitude);
        var deltaLon = DegreesToRadians(c2.Longitude - c1.Longitude);

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return r * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
}