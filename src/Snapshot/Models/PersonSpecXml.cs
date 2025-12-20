using System.Xml;
using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Unified person specification for both templates (fixed values) and generators (randomized values).
/// </summary>
/// <remarks>
///     <para>
///     Behavior is determined by the <see cref="Seed" /> attribute:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             <strong>Template mode</strong>: No seed specified - creates persons with fixed attribute
///             values
///             </description>
///         </item>
///         <item>
///             <description>
///             <strong>Generator mode</strong>: Seed specified - creates persons with randomized attributes
///             from specifications
///             </description>
///         </item>
///     </list>
///     <para>
///     All numeric attributes support both fixed values (V="0.5") and ranges (Min="0.3" Max="0.8") for generator mode.
///     Template mode uses only fixed values.
///     </para>
/// </remarks>
public class PersonSpecXml
{
    /// <summary>
    /// Gets or sets the number of persons to create from this specification.
    /// </summary>
    [XmlAttribute("Count")]
    public int Count { get; set; } = 1;

    /// <summary>
    /// Gets or sets the random seed for reproducible generation.
    /// </summary>
    /// <remarks>
    /// When specified, this PersonSpecXml acts as a generator with randomized attributes.
    /// When not specified, acts as a template with fixed attributes.
    /// </remarks>
    [XmlAttribute("Seed")]
    public int? Seed { get; set; }

    /// <summary>
    /// Gets or sets the person type name (e.g., "StandardPerson", "DemographicPerson").
    /// </summary>
    /// <remarks>
    /// Defaults to "StandardPerson" if not specified. For custom person types, specify the type name
    /// and register a custom factory with <see cref="Conversion.PersonTypeRegistry" />.
    /// </remarks>
    [XmlAttribute("Type")]
    public string Type { get; set; } = "StandardPerson";

    /// <summary>
    /// Gets or sets the factor sensitivity specifications.
    /// </summary>
    [XmlArray("Sensitivities")]
    [XmlArrayItem("S")]
    public List<ValueSpecXml>? Sensitivities { get; set; }

    /// <summary>
    /// Gets or sets the moving willingness specification (0-1 range).
    /// </summary>
    [XmlElement("Willingness")]
    public ValueSpecXml? Willingness { get; set; }

    /// <summary>
    /// Gets or sets the retention rate specification (0-1 range).
    /// </summary>
    [XmlElement("Retention")]
    public ValueSpecXml? Retention { get; set; }

    /// <summary>
    /// Gets or sets the attraction threshold specification (0-1 range).
    /// </summary>
    [XmlElement("Threshold")]
    public ValueSpecXml? Threshold { get; set; }

    /// <summary>
    /// Gets or sets the sensitivity scaling coefficient specification (0-1 range).
    /// </summary>
    [XmlElement("Scaling")]
    public ValueSpecXml? Scaling { get; set; }

    /// <summary>
    /// Gets or sets the minimum acceptable attraction score specification (0-1 range).
    /// </summary>
    [XmlElement("MinAttraction")]
    public ValueSpecXml? MinAttraction { get; set; }

    /// <summary>
    /// Gets or sets the semicolon-separated tags for categorization.
    /// </summary>
    [XmlElement("Tags")]
    public string? Tags { get; set; }

    /// <summary>
    /// Gets or sets custom properties for extended person types.
    /// </summary>
    /// <remarks>
    /// This element contains arbitrary XML elements for custom person type properties.
    /// Custom person types should implement ICustomPersonSerializer to handle serialization.
    /// </remarks>
    [XmlAnyElement]
    public XmlElement? CustomProperties { get; set; }

    /// <summary>
    /// Determines whether this specification is a generator (has seed) or template (no seed).
    /// </summary>
    [XmlIgnore]
    public bool IsGenerator => Seed.HasValue;
}