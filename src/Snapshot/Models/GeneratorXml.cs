using System.Xml;
using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Person generator with randomized attribute specifications.
/// </summary>
/// <remarks>
/// Creates multiple persons with attributes drawn from specified distributions.
/// </remarks>
public class GeneratorXml
{
    /// <summary>
    /// Gets or sets the number of persons to generate.
    /// </summary>
    [XmlAttribute("Count")]
    public int Count { get; set; }

    /// <summary>
    /// Gets or sets the person type name (e.g., "StandardPerson", "DemographicPerson").
    /// </summary>
    /// <remarks>
    /// Defaults to "StandardPerson" if not specified. For custom person types, specify the type name
    /// and register a custom generator factory with <see cref="Conversion.PersonTypeRegistry" />.
    /// </remarks>
    [XmlAttribute("PersonType")]
    public string PersonType { get; set; } = "StandardPerson";

    /// <summary>
    /// Gets or sets the random seed for reproducibility.
    /// </summary>
    [XmlAttribute("Seed")]
    public int Seed { get; set; }

    /// <summary>
    /// Gets or sets whether seed is explicitly specified.
    /// </summary>
    [XmlIgnore]
    public bool SeedSpecified { get; set; } = true;

    /// <summary>
    /// Gets or sets the factor sensitivity specifications.
    /// </summary>
    [XmlArray("Sensitivities")]
    [XmlArrayItem("S")]
    public List<SensitivitySpecXml>? FactorSensitivities { get; set; }

    /// <summary>
    /// Gets or sets the moving willingness specification.
    /// </summary>
    [XmlElement("Willingness")]
    public ValueSpecXml? MovingWillingness { get; set; }

    /// <summary>
    /// Gets or sets the retention rate specification.
    /// </summary>
    [XmlElement("Retention")]
    public ValueSpecXml? RetentionRate { get; set; }

    /// <summary>
    /// Gets or sets the attraction threshold specification.
    /// </summary>
    [XmlElement("Threshold")]
    public ValueSpecXml? AttractionThreshold { get; set; }

    /// <summary>
    /// Gets or sets the sensitivity scaling specification.
    /// </summary>
    [XmlElement("Scaling")]
    public ValueSpecXml? SensitivityScaling { get; set; }

    /// <summary>
    /// Gets or sets the minimum acceptable attraction specification.
    /// </summary>
    [XmlElement("MinAttraction")]
    public ValueSpecXml? MinimumAcceptableAttraction { get; set; }

    /// <summary>
    /// Gets or sets the semicolon-separated tags for categorization.
    /// </summary>
    [XmlElement("Tags")]
    public string? Tags { get; set; }

    /// <summary>
    /// Gets or sets custom property specifications for extended person types.
    /// </summary>
    /// <remarks>
    /// This element contains arbitrary XML elements for custom person type property specifications.
    /// Custom person generator types should implement ICustomGeneratorSerializer to handle serialization.
    /// </remarks>
    [XmlAnyElement]
    public XmlElement? CustomProperties { get; set; }
}