namespace dotGeoMigrata.Interfaces;

/// <summary>
/// Common interface for identifiable simulation components.
/// </summary>
internal interface IIdentifiable
{
    public string DisplayName { get; init; }
}