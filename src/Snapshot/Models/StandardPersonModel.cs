using System.Xml.Serialization;
using dotMigrata.Snapshot.Interfaces;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Standard person model for snapshot serialization using XML attributes.
/// </summary>
/// <remarks>
/// <para>
/// This is the default person model and works with <see cref="Core.Entities.StandardPerson"/>.
/// Implements <see cref="IPersonModel"/> and uses XML attributes for declarative serialization.
/// </para>
/// <para>
/// Use <see cref="Conversion.StandardPersonModelConverter"/> for conversion between this model
/// and runtime <see cref="Core.Entities.StandardPerson"/> instances.
/// </para>
/// </remarks>
[XmlRoot("Person")]
public sealed class StandardPersonModel : IPersonModel
{
    /// <inheritdoc />
    [XmlAttribute("Count")]
    public int Count { get; set; } = 1;

    /// <inheritdoc />
    [XmlAttribute("Seed")]
    public int Seed { get; set; }

    /// <inheritdoc />
    [XmlIgnore]
    public bool SeedSpecified { get; set; }

    /// <inheritdoc />
    [XmlAttribute("Type")]
    public string Type { get; set; } = "StandardPerson";

    /// <inheritdoc />
    [XmlElement("Tags")]
    public string? Tags { get; set; }

    /// <inheritdoc />
    [XmlIgnore]
    public bool IsGenerator => SeedSpecified;

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
}