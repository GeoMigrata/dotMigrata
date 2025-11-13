namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents the initial state of a world for person-based simulations.
/// </summary>
public sealed record InitialWorldState
{
    public required string DisplayName { get; init; }
    public required List<FactorSnapshot> Factors { get; init; }
    public required List<CitySnapshot> Cities { get; init; }
    public required List<PersonSnapshot> Persons { get; init; }
}