using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Interfaces;

/// <summary>
/// Base interface for all person model classes used in snapshot serialization.
/// Implement this interface and use XML attributes to define custom person models.
/// </summary>
/// <remarks>
/// This interface ensures all person models have the required base properties.
/// Custom person models can add additional properties decorated with XML attributes.
/// </remarks>
public interface IPersonModel
{
    /// <summary>
    /// Gets or sets the number of persons to create from this specification.
    /// </summary>
    [XmlAttribute("Count")]
    int Count { get; set; }

    /// <summary>
    /// Gets or sets the random seed for reproducible generation.
    /// When specified, acts as a generator. When not specified, acts as a template.
    /// </summary>
    [XmlAttribute("Seed")]
    int Seed { get; set; }

    /// <summary>
    /// Gets or sets whether Seed should be serialized.
    /// </summary>
    [XmlIgnore]
    bool SeedSpecified { get; set; }

    /// <summary>
    /// Gets or sets the person type name (e.g., "StandardPerson", "DemographicPerson").
    /// </summary>
    [XmlAttribute("Type")]
    string Type { get; set; }

    /// <summary>
    /// Gets or sets the semicolon-separated tags for categorization.
    /// </summary>
    [XmlElement("Tags")]
    string? Tags { get; set; }

    /// <summary>
    /// Determines whether this specification is a generator (has seed) or template (no seed).
    /// </summary>
    [XmlIgnore]
    bool IsGenerator { get; }
}