using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Person template with fixed attributes.
/// </summary>
/// <remarks>
/// Creates one or more persons with identical attributes.
/// Use <see cref="GeneratorXml"/> for randomized attribute generation.
/// </remarks>
public class PersonTemplateXml
{
    /// <summary>
    /// Gets or sets the number of persons to create from this template.
    /// </summary>
    [XmlAttribute("Count")]
    public int Count { get; set; } = 1;

    /// <summary>
    /// Gets or sets the willingness to migrate (0-1).
    /// </summary>
    [XmlAttribute("Willingness")]
    public double MovingWillingness { get; set; }

    /// <summary>
    /// Gets or sets the retention rate (0-1).
    /// </summary>
    [XmlAttribute("Retention")]
    public double RetentionRate { get; set; }

    /// <summary>
    /// Gets or sets the attraction threshold.
    /// </summary>
    [XmlAttribute("Threshold")]
    public double AttractionThreshold { get; set; }

    /// <summary>
    /// Gets or sets the sensitivity scaling coefficient.
    /// </summary>
    [XmlAttribute("Scaling")]
    public double SensitivityScaling { get; set; }

    /// <summary>
    /// Gets or sets the minimum acceptable attraction score.
    /// </summary>
    [XmlAttribute("MinAttraction")]
    public double MinimumAcceptableAttraction { get; set; }

    /// <summary>
    /// Gets or sets the semicolon-separated tags for categorization.
    /// </summary>
    [XmlAttribute("Tags")]
    public string? Tags { get; set; }

    /// <summary>
    /// Gets or sets the factor sensitivities.
    /// </summary>
    [XmlArray("Sensitivities")]
    [XmlArrayItem("S")]
    public List<FactorSensitivityXml>? FactorSensitivities { get; set; }
}