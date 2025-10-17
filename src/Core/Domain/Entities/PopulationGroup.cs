using dotGeoMigrata.Core.Domain.Values;

namespace dotGeoMigrata.Core.Domain.Entities;

internal sealed record PopulationGroup
{
    public required string Id { get; init; }
    public required int Count { get; init; }
    public required string? DisplayName { get; init; }
    public required double MovingWillingness { get; init; }
    public required double MaxMigrationThreshold { get; init; }
    public required double MinMigrationThreshold { get; init; }
    public required List<FactorSensitivity> Sensitivities { get; init; }

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