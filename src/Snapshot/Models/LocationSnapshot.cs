namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Snapshot representation of geographic coordinates.
/// </summary>
public sealed record LocationSnapshot
{
    /// <summary>
    /// Gets or initializes the longitude (-180 to 180).
    /// </summary>
    public required double Longitude { get; init; }

    /// <summary>
    /// Gets or initializes the latitude (-90 to 90).
    /// </summary>
    public required double Latitude { get; init; }
}