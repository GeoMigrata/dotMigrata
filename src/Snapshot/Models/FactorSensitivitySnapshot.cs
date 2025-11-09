using System.Xml.Serialization;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Snapshot representation of a factor sensitivity.
/// </summary>
[XmlType("Sensitivity")]
public sealed class FactorSensitivitySnapshot
{
    /// <summary>
    /// Gets or sets the factor reference (ID).
    /// </summary>
    [XmlAttribute("FactorId")]
    public string FactorRef { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sensitivity value.
    /// </summary>
    [XmlAttribute("Value")]
    public int Sensitivity { get; set; }

    /// <summary>
    /// Gets or sets an optional factor type override ("Positive" or "Negative").
    /// </summary>
    [XmlAttribute("OverrideType")]
    public string? OverriddenFactorType { get; set; }
}