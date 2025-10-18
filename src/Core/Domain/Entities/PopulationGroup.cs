using dotGeoMigrata.Core.Domain.Values;
using dotGeoMigrata.Interfaces;

namespace dotGeoMigrata.Core.Domain.Entities;

internal sealed record PopulationGroup : IIdentifiable
{
    public string Id { get; init; }
    public int Count { get; init; }
    public string? DisplayName { get; init; }
    public double MovingWillingness { get; init; }
    public double MaxMigrationThreshold { get; init; }
    public double MinMigrationThreshold { get; init; }
    public List<FactorSensitivity> Sensitivities { get; init; }

    public PopulationGroup(
        string id,
        int count,
        double movingWillingness,
        double maxMigrationThreshold,
        double minMigrationThreshold,
        string? displayName = null,
        List<FactorSensitivity>? sensitivities = null) =>
        (Id, Count, MovingWillingness, MaxMigrationThreshold, MinMigrationThreshold, DisplayName, Sensitivities) = (
            !string.IsNullOrWhiteSpace(id)
                ? id
                : throw new ArgumentException("Id of PopulationGroup must be non-empty", nameof(id)),
            count > 0 ? count : throw new ArgumentException("Count must be larger than zero", nameof(count)),
            movingWillingness,
            maxMigrationThreshold,
            minMigrationThreshold,
            displayName,
            sensitivities ?? []);
}