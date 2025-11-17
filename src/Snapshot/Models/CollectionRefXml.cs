using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Reference to a PersonCollection.
/// </summary>
public class CollectionRefXml
{
    /// <summary>Gets or sets the person collection ID reference.</summary>
    [XmlAttribute("Id")]
    public string Id { get; set; } = string.Empty;
}