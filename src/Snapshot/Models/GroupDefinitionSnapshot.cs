using System.Xml.Serialization;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Snapshot representation of a population group definition.
/// </summary>
[XmlType("GroupDefinition")]
public sealed class GroupDefinitionSnapshot
{
    /// <summary>
    /// Gets or sets the unique identifier for this group.
    /// Used for references within the snapshot.
    /// </summary>
    [XmlAttribute("Id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    [XmlAttribute("Name")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the moving willingness (0-1).
    /// </summary>
    [XmlAttribute("MovingWillingness")]
    public double MovingWillingness { get; set; }

    /// <summary>
    /// Gets or sets the retention rate (0-1).
    /// </summary>
    [XmlAttribute("RetentionRate")]
    public double RetentionRate { get; set; }

    /// <summary>
    /// Gets or sets the sensitivity scaling coefficient.
    /// </summary>
    [XmlAttribute("SensitivityScaling")]
    public double SensitivityScaling { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the attraction threshold.
    /// </summary>
    [XmlAttribute("AttractionThreshold")]
    public double AttractionThreshold { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the minimum acceptable attraction score.
    /// </summary>
    [XmlAttribute("MinAcceptableAttraction")]
    public double MinimumAcceptableAttraction { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the factor sensitivities.
    /// References factors by ID.
    /// </summary>
    [XmlArray("Sensitivities")]
    public List<FactorSensitivitySnapshot> Sensitivities { get; set; } = new();
}