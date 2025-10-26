namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents a geographic location in a snapshot.
/// </summary>
public sealed class LocationSnapshot
{
    /// <summary>
    /// Gets or sets the latitude.
    /// </summary>
    public required double Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude.
    /// </summary>
    public required double Longitude { get; set; }
}