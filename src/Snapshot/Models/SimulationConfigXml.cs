using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// XML model for simulation configuration.
/// </summary>
public sealed class SimulationConfigXml
{
    /// <summary>
    /// Gets or sets the maximum number of steps.
    /// </summary>
    [XmlAttribute("MaxSteps")]
    public int MaxSteps { get; set; } = 1000;

    /// <summary>
    /// Gets or sets whether to check for stability.
    /// </summary>
    [XmlAttribute("CheckStability")]
    public bool CheckStability { get; set; } = true;

    /// <summary>
    /// Gets or sets the stability threshold.
    /// </summary>
    [XmlAttribute("StabilityThreshold")]
    public int StabilityThreshold { get; set; } = 10;

    /// <summary>
    /// Gets or sets the stability check interval.
    /// </summary>
    [XmlAttribute("StabilityCheckInterval")]
    public int StabilityCheckInterval { get; set; } = 1;

    /// <summary>
    /// Gets or sets the minimum steps before stability check.
    /// </summary>
    [XmlAttribute("MinStepsBeforeStabilityCheck")]
    public int MinStepsBeforeStabilityCheck { get; set; } = 10;
}