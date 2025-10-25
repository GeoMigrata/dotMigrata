namespace dotGeoMigrata.Core.Values;

public readonly record struct Coordinate
{
    private readonly double _longitude;

    public required double Longitude
    {
        get => _longitude;
        init => _longitude = value is >= -180 and <= 180
            ? value
            : throw new ArgumentException("Longitude must be between -180 and 180 degrees.", nameof(value));
    }

    private readonly double _latitude;

    public required double Latitude
    {
        get => _latitude;
        init => _latitude = value is >= -90 and <= 90
            ? value
            : throw new ArgumentException("Latitude must be between -90 and 90 degrees.", nameof(value));
    }

    /// <summary>
    /// Computes the great-circle distance between this coordinate and another using the Haversine formula.
    /// </summary>
    /// <param name="other">Other coordinate to calculate distance to.</param>
    /// <returns>Distance in kilometers.</returns>
    public double DistanceTo(Coordinate other) => DistanceBetween(this, other);

    /// <summary>
    /// Computes the great-circle distance between two coordinates using the Haversine formula.
    /// </summary>
    /// <param name="c1">First coordinate.</param>
    /// <param name="c2">Second coordinate.</param>
    /// <returns>Distance in kilometers.</returns>
    public static double DistanceBetween(Coordinate c1, Coordinate c2)
    {
        const double earthRadiusKm = 6371.0;

        var lat1Rad = DegreesToRadians(c1.Latitude);
        var lat2Rad = DegreesToRadians(c2.Latitude);
        var deltaLat = DegreesToRadians(c2.Latitude - c1.Latitude);
        var deltaLon = DegreesToRadians(c2.Longitude - c1.Longitude);

        var sinDeltaLatHalf = Math.Sin(deltaLat * 0.5);
        var sinDeltaLonHalf = Math.Sin(deltaLon * 0.5);

        var a = sinDeltaLatHalf * sinDeltaLatHalf +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                sinDeltaLonHalf * sinDeltaLonHalf;

        var c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
}