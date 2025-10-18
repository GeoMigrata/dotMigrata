using dotGeoMigrata.Core.Domain.Enums;

namespace dotGeoMigrata.Core.Domain.Values;

/// <summary>
/// Defines a factor including its transformation type, direction, and normalization rule.
/// </summary>
internal readonly record struct FactorDefinition(
    string Id,
    FactorType Type,
    double MinValue = 0,
    double MaxValue = 1,
    string? DisplayName = null,
    string? Description = null,
    TransformType? Transform = null)
{
    public string Id { get; init; } = !string.IsNullOrWhiteSpace(Id)
        ? Id
        : throw new ArgumentException("Id of Factor must be non-empty", nameof(Id));

    public string? DisplayName { get; init; } = DisplayName;
    public string? Description { get; init; } = Description;
    public FactorType Type { get; init; } = Type;
    public TransformType? Transform { get; init; } = Transform;

    public double MinValue { get; init; }
    public double MaxValue { get; init; }

    public double Normalize(double rawValue)
    {
        // Clamp raw value to valid range
        var clamped = Math.Clamp(rawValue, MinValue, MaxValue);
        double t;
        switch (Transform)
        {
            case TransformType.Linear:
                t = (clamped - MinValue) / (MaxValue - MinValue);
                break;
            case TransformType.Log:
                // Apply logarithmic normalization
                const double delta = 1e-6; // small offset to avoid log(0)
                var num = Math.Log(clamped + delta) - Math.Log(MinValue + delta);
                var denom = Math.Log(MaxValue + delta) - Math.Log(MinValue + delta);
                t = denom != 0 ? num / denom : .0;
                break;
            case TransformType.Sigmoid:
                // Apply sigmoid normalization
                var linear = (clamped - MinValue) / (MaxValue - MinValue);
                var centered = (linear - .5) * 10.0; // 10.0 = steepness parameter
                var s = 1.0 / (1.0 + Math.Exp(-centered));
                t = s;
                break;
            default:
                t = (clamped - MinValue) / (MaxValue - MinValue);
                break;
        }

        // Reverse value if the factor type is Negative
        return Type == FactorType.Positive ? t : 1.0 - t;
    }
}