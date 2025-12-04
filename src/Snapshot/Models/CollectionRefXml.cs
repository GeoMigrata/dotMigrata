using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Reference to a population group.
/// </summary>
public class CollectionRefXml
{
    /// <summary>
    /// Gets or sets the population group ID reference.
    /// </summary>
    [XmlAttribute("Id")]
    public string Id { get; set; } = string.Empty;
}