using System.Xml.Serialization;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Reference to a PersonCollection.
/// </summary>
public class CollectionRefXml
{
    [XmlAttribute("Id")] public string Id { get; set; } = string.Empty;
}