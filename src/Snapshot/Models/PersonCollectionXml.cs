using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Population group containing person templates and generators.
/// </summary>
/// <remarks>
/// Groups can be referenced by cities to assign population.
/// </remarks>
public class PersonCollectionXml
{
    /// <summary>
    /// Gets or sets the unique identifier for this group.
    /// </summary>
    [XmlAttribute("Id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the person templates (fixed attribute persons).
    /// </summary>
    [XmlElement("Person")]
    public List<PersonTemplateXml>? Persons { get; set; }

    /// <summary>
    /// Gets or sets the generators (random attribute persons).
    /// </summary>
    [XmlElement("Generator")]
    public List<GeneratorXml>? Generators { get; set; }
}