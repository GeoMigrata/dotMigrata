using dotGeoMigrata.Core.Domain.Values;

namespace dotGeoMigrata.Core.Domain.Entities;

public sealed record PopulationGroup
{
    public required int Count { get; init; }
    public required string DisplayName { get; init; }
    public double MovingWillingness { get; init; }
    public double RetentionRate { get; init; }

    private readonly List<FactorSensitivity> _sensitivities;
    public IReadOnlyList<FactorSensitivity> Sensitivities => _sensitivities;
    
    public PopulationGroup(List<FactorSensitivity> sensitivities)
    {
        _sensitivities = sensitivities;
    }
}