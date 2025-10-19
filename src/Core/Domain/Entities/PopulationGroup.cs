using dotGeoMigrata.Core.Domain.Values;
using dotGeoMigrata.Interfaces;

namespace dotGeoMigrata.Core.Domain.Entities;

internal sealed record PopulationGroup : IIdentifiable
{
    public int Count { get; init; }
    public string DisplayName { get; init; }
    public double MovingWillingness { get; init; }
    public double MigrationThreshold { get; init; }
    public double AcceptanceThreshold { get; init; }
    
    private readonly List<FactorSensitivity> _sensitivities;
    public IReadOnlyList<FactorSensitivity> Sensitivities => _sensitivities;

    public PopulationGroup(
        string displayName,
        int count,
        double movingWillingness,
        double maxMigrationThreshold,
        double minMigrationThreshold,
        IEnumerable<FactorSensitivity>? sensitivities = null)
    {
        (DisplayName, Count, MovingWillingness, MigrationThreshold, AcceptanceThreshold, _sensitivities) = (
            displayName,
            count > 0 ? count : throw new ArgumentException("Count must be larger than zero", nameof(count)),
            movingWillingness,
            maxMigrationThreshold,
            minMigrationThreshold,
            sensitivities == null ? [] : sensitivities.ToList());
    }
}