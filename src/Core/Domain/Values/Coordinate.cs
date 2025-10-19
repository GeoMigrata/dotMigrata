namespace dotGeoMigrata.Core.Domain.Values;

internal readonly struct Coordinate
{
    public required double Longitude { get; init; }
    public required double Latitude { get; init; }
}