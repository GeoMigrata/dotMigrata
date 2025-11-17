using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Person collection definition (snapshot-only container).
/// </summary>
public class PersonCollectionXml
{
    /// <summary>Gets or sets the unique identifier for the collection.</summary>
    [XmlAttribute("Id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the person templates in this collection.</summary>
    [XmlElement("Person", Namespace = "http://geomigrata.pages.dev/code")]
    public List<PersonTemplateXml>? Persons { get; set; }

    /// <summary>Gets or sets the generators in this collection.</summary>
    [XmlElement("Generator")]
    public List<GeneratorXml>? Generators { get; set; }
}