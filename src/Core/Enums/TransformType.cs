namespace dotMigrata.Core.Enums;

/// <summary>
/// Defines the transformation function used to normalize factor values.
/// </summary>
public enum TransformType
{
    /// <summary>
    /// Linear normalization: proportional scaling between min and max.
    /// </summary>
    Linear,

    /// <summary>
    /// Logarithmic normalization: emphasizes differences at lower values.
    /// </summary>
    Log,

    /// <summary>
    /// Sigmoid normalization: smooth S-curve transformation.
    /// </summary>
    Sigmoid
}