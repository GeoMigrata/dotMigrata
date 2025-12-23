using System.Xml.Serialization;
using dotMigrata.Snapshot.Interfaces;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Defines a collection of person specifications (both templates and generators).
/// </summary>
/// <remarks>
/// Person specifications use the DI-based model system with XML attributes.
/// Supports custom person models implementing <see cref="IPersonModel" />.
/// </remarks>
public class PersonCollectionXml
{
    /// <summary>
    /// Gets or sets the unique identifier for this collection.
    /// </summary>
    [XmlAttribute("Id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the person specifications using StandardPersonModel.
    /// </summary>
    /// <remarks>
    /// Models without SeedSpecified=true act as templates (fixed values).
    /// Models with SeedSpecified=true act as generators (randomized values).
    /// </remarks>
    [XmlElement("Person")]
    public List<StandardPersonModel>? PersonModels { get; set; }
}