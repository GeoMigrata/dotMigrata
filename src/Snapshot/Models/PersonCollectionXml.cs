using System.Xml.Serialization;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Person collection definition (snapshot-only container).
/// </summary>
public class PersonCollectionXml
{
    [XmlAttribute("Id")] public string Id { get; set; } = string.Empty;

    [XmlElement("Person", Namespace = "http://geomigrata.org/code")]
    public List<PersonTemplateXml>? Persons { get; set; }

    [XmlElement("Generator")] public List<GeneratorXml>? Generators { get; set; }
}