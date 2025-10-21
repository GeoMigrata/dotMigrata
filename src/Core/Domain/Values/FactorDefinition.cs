using dotGeoMigrata.Core.Domain.Enums;

namespace dotGeoMigrata.Core.Domain.Values;

/// <summary>
/// Defines a factor including its transformation type, direction, and normalization rule.
/// </summary>
public record FactorDefinition
{
    public required string DisplayName { get; init; }
    public required FactorType Type { get; init; }
    public required double MinValue { get; init; }
    public required double MaxValue { get; init; }
    public required TransformType? Transform { get; init; }

    internal double Normalize(double rawValue)
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

        return t;
    }
}