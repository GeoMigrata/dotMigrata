using dotMigrata.Core.Values.Interfaces;

namespace dotMigrata.Core.Values;

/// <summary>
/// Built-in transform functions for factor normalization.
/// </summary>
public static class TransformFunctions
{
    /// <summary>
    /// Linear (proportional) transformation.
    /// </summary>
    public static ITransformFunction Linear { get; } = new LinearTransform();

    /// <summary>
    /// Logarithmic transformation - emphasizes differences at lower values.
    /// </summary>
    public static ITransformFunction Logarithmic { get; } = new LogarithmicTransform();

    /// <summary>
    /// Sigmoid (S-curve) transformation - smooth transition with emphasis on middle range.
    /// </summary>
    public static ITransformFunction Sigmoid { get; } = new SigmoidTransform();

    private sealed class LinearTransform : ITransformFunction
    {
        public string Name => "Linear";

        public double Transform(double value, double min, double max)
        {
            var range = max - min;
            return range == 0 ? 0 : (value - min) / range;
        }
    }

    private sealed class LogarithmicTransform : ITransformFunction
    {
        public string Name => "Logarithmic";

        public double Transform(double value, double min, double max)
        {
            const double delta = 1e-6; // small offset to avoid log(0)
            var range = max - min;
            if (range == 0) return 0;

            var numerator = Math.Log(value - min + delta);
            var denominator = Math.Log(range + delta);
            return denominator != 0 ? numerator / denominator : 0.0;
        }
    }

    private sealed class SigmoidTransform : ITransformFunction
    {
        public string Name => "Sigmoid";

        public double Transform(double value, double min, double max)
        {
            const double steepness = 10.0;
            var range = max - min;
            if (range == 0) return 0;

            var linear = (value - min) / range;
            var centered = (linear - 0.5) * steepness;
            return 1.0 / (1.0 + Math.Exp(-centered));
        }
    }
}