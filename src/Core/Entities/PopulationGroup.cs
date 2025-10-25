using dotGeoMigrata.Core.Values;

namespace dotGeoMigrata.Core.Entities;

public sealed record PopulationGroup
{
    private readonly int _count;

    private readonly double _movingWillingness;

    private readonly double _retentionRate;

    private readonly List<FactorSensitivity> _sensitivities;

    public PopulationGroup(List<FactorSensitivity> sensitivities)
    {
        ArgumentNullException.ThrowIfNull(sensitivities, nameof(sensitivities));
        _sensitivities = sensitivities.ToList();
    }

    public required int Count
    {
        get => _count;
        init => _count = value >= 0 ? value : throw new ArgumentException("Count cannot be negative.", nameof(value));
    }

    public required string DisplayName { get; init; }

    public double MovingWillingness
    {
        get => _movingWillingness;
        init => _movingWillingness = value is >= 0 and <= 1
            ? value
            : throw new ArgumentException("MovingWillingness must be between 0 and 1.", nameof(value));
    }

    public double RetentionRate
    {
        get => _retentionRate;
        init => _retentionRate = value is >= 0 and <= 1
            ? value
            : throw new ArgumentException("RetentionRate must be between 0 and 1.", nameof(value));
    }

    public IReadOnlyList<FactorSensitivity> Sensitivities => _sensitivities;
}