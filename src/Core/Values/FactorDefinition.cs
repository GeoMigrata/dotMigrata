using dotGeoMigrata.Core.Enums;

namespace dotGeoMigrata.Core.Values;

/// <summary>
/// Defines a factor including its transformation type, direction, and normalization rule.
/// </summary>
public record FactorDefinition
{
    private readonly double _maxValue;

    private readonly double _minValue;
    public required string DisplayName { get; init; }
    public required FactorType Type { get; init; }

    public required double MinValue
    {
        get => _minValue;
        init
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("MinValue must be a valid number.", nameof(MinValue));
            _minValue = value;
        }
    }

    public required double MaxValue
    {
        get => _maxValue;
        init
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("MaxValue must be a valid number.", nameof(MaxValue));
            if (value <= MinValue)
                throw new ArgumentException("MaxValue must be greater than MinValue.", nameof(MaxValue));
            _maxValue = value;
        }
    }

    public required TransformType? Transform { get; init; }

    internal double Normalize(double rawValue)
    {
        // Clamp raw value to valid range
        var clamped = Math.Clamp(rawValue, MinValue, MaxValue);
        var range = MaxValue - MinValue;

        if (range == 0)
            return 0;

        return Transform switch
        {
            TransformType.Linear => (clamped - MinValue) / range,
            TransformType.Log => NormalizeLogarithmic(clamped, range),
            TransformType.Sigmoid => NormalizeSigmoid(clamped, range),
            _ => (clamped - MinValue) / range
        };
    }

    private double NormalizeLogarithmic(double clamped, double range)
    {
        const double delta = 1e-6; // small offset to avoid log(0)
        var numerator = Math.Log(clamped - MinValue + delta);
        var denominator = Math.Log(range + delta);
        return denominator != 0 ? numerator / denominator : 0.0;
    }

    private double NormalizeSigmoid(double clamped, double range)
    {
        const double steepness = 10.0;
        var linear = (clamped - MinValue) / range;
        var centered = (linear - 0.5) * steepness;
        return 1.0 / (1.0 + Math.Exp(-centered));
    }
}