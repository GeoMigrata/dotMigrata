using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// XML model for standard model configuration.
/// </summary>
public sealed class StandardModelConfigXml
{
    /// <summary>
    /// Gets or sets the capacity steepness parameter.
    /// </summary>
    [XmlAttribute("CapacitySteepness")]
    public double CapacitySteepness { get; set; } = 5.0;

    /// <summary>
    /// Gets or sets the distance decay lambda.
    /// </summary>
    [XmlAttribute("DistanceDecayLambda")]
    public double DistanceDecayLambda { get; set; } = 0.001;

    /// <summary>
    /// Gets or sets the migration probability steepness.
    /// </summary>
    [XmlAttribute("MigrationProbabilitySteepness")]
    public double MigrationProbabilitySteepness { get; set; } = 10.0;

    /// <summary>
    /// Gets or sets the migration probability threshold.
    /// </summary>
    [XmlAttribute("MigrationProbabilityThreshold")]
    public double MigrationProbabilityThreshold { get; set; }

    /// <summary>
    /// Gets or sets the factor smoothing alpha.
    /// </summary>
    [XmlAttribute("FactorSmoothingAlpha")]
    public double FactorSmoothingAlpha { get; set; } = 0.2;

    /// <summary>
    /// Gets or sets whether to use parallel processing.
    /// </summary>
    [XmlAttribute("UseParallelProcessing")]
    public bool UseParallelProcessing { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum degree of parallelism.
    /// </summary>
    [XmlAttribute("MaxDegreeOfParallelism")]
    public int? MaxDegreeOfParallelism { get; set; }
}