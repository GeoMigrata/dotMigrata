using dotGeoMigrata.Core.Domain.Values;

namespace dotGeoMigrata.Core.Domain.Entities;

public sealed record PopulationGroup
{
    private readonly int _count;

    public required int Count
    {
        get => _count;
        init => _count = value >= 0 ? value : throw new ArgumentException("Count cannot be negative.", nameof(value));
    }

    public required string DisplayName { get; init; }

    private readonly double _movingWillingness;

    public double MovingWillingness
    {
        get => _movingWillingness;
        init => _movingWillingness = value is >= 0 and <= 1
            ? value
            : throw new ArgumentException("MovingWillingness must be between 0 and 1.", nameof(value));
    }

    private readonly double _retentionRate;

    public double RetentionRate
    {
        get => _retentionRate;
        init => _retentionRate = value is >= 0 and <= 1
            ? value
            : throw new ArgumentException("RetentionRate must be between 0 and 1.", nameof(value));
    }

    private readonly List<FactorSensitivity> _sensitivities;
    public IReadOnlyList<FactorSensitivity> Sensitivities => _sensitivities;

    public PopulationGroup(List<FactorSensitivity> sensitivities)
    {
        ArgumentNullException.ThrowIfNull(sensitivities, nameof(sensitivities));
        _sensitivities = sensitivities.ToList();
    }
}