namespace dotGeoMigrata.Generator;

/// <summary>
/// Fluent builder for specifying person attribute values with semantic API.
/// Provides natural language-style API for clearer and more readable specifications.
/// </summary>
public sealed class AttributeValueBuilder
{
    private double? _fixedValue;
    private (double Min, double Max)? _range;
    private double _scale = 1.0;

    private AttributeValueBuilder()
    {
    }

    /// <summary>
    /// Starts building a value specification.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public static AttributeValueBuilder Value()
    {
        return new AttributeValueBuilder();
    }

    /// <summary>
    /// Specifies a fixed value.
    /// </summary>
    /// <param name="value">The exact value to use.</param>
    /// <returns>The built value specification.</returns>
    public ValueSpecification Of(double value)
    {
        _fixedValue = value;
        return Build();
    }

    /// <summary>
    /// Specifies a value within a range.
    /// </summary>
    /// <param name="min">Minimum value (inclusive).</param>
    /// <param name="max">Maximum value (inclusive).</param>
    /// <returns>The built value specification.</returns>
    public ValueSpecification InRange(double min, double max)
    {
        if (min >= max)
            throw new ArgumentException("Min must be less than max.");
        _range = (min, max);
        return Build();
    }

    /// <summary>
    /// Specifies an approximate value with scaling/bias.
    /// </summary>
    /// <param name="target">The approximate target value.</param>
    /// <param name="scale">Scale factor for variation. Default is 1.0.</param>
    /// <returns>The built value specification.</returns>
    public ValueSpecification Approximately(double target, double scale = 1.0)
    {
        _fixedValue = target;
        _scale = scale;
        return Build();
    }

    private ValueSpecification Build()
    {
        if (_fixedValue.HasValue && !_range.HasValue)
            // Fixed value (possibly with scale for BeApproximately)
            return Math.Abs(_scale - 1.0) < double.Epsilon
                ? ValueSpecification.Fixed(_fixedValue.Value)
                : ValueSpecification.Fixed(_fixedValue.Value).WithScale(_scale);

        if (!_range.HasValue) return ValueSpecification.Random().WithScale(_scale);
        // Range specification
        var spec = ValueSpecification.InRange(_range.Value.Min, _range.Value.Max);
        if (Math.Abs(_scale - 1.0) > double.Epsilon)
            spec = spec.WithScale(_scale);
        return spec;

        // Random with scale
    }
}