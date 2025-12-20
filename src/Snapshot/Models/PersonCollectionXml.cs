using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Defines a collection of person specifications (both templates and generators).
/// </summary>
/// <remarks>
/// Person specifications can be either templates (fixed values) or generators (randomized values).
/// The presence of a Seed attribute determines the mode.
/// </remarks>
public class PersonCollectionXml
{
    /// <summary>
    /// Gets or sets the unique identifier for this collection.
    /// </summary>
    [XmlAttribute("Id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the person specifications (both templates and generators).
    /// </summary>
    /// <remarks>
    /// Specifications without a Seed attribute act as templates (fixed values).
    /// Specifications with a Seed attribute act as generators (randomized values).
    /// </remarks>
    [XmlElement("Person")]
    public List<PersonSpecXml>? PersonSpecs { get; set; }
}