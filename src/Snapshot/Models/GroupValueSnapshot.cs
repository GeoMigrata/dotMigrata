using System.Xml.Serialization;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Snapshot representation of a population group value.
/// </summary>
[XmlType("Group")]
public sealed class GroupValueSnapshot
{
    /// <summary>
    /// Gets or sets the group reference (ID).
    /// </summary>
    [XmlAttribute("GroupId")]
    public string GroupRef { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the population count.
    /// </summary>
    [XmlAttribute("Population")]
    public int Population { get; set; }
}