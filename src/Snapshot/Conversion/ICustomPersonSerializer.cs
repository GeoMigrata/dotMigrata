using System.Xml;
using dotMigrata.Core.Entities;
using dotMigrata.Core.Values;
using dotMigrata.Snapshot.Models;

namespace dotMigrata.Snapshot.Conversion;

/// <summary>
/// Defines serialization methods for custom person types in snapshots.
/// </summary>
/// <typeparam name="TPerson">The custom person type derived from <see cref="PersonBase" />.</typeparam>
/// <remarks>
/// Implement this interface to enable snapshot support for custom person types.
/// Register implementations with <see cref="PersonTypeRegistry.RegisterPersonType{TPerson}" />.
/// </remarks>
public interface ICustomPersonSerializer<TPerson> where TPerson : PersonBase
{
    /// <summary>
    /// Creates a person instance from template XML data.
    /// </summary>
    /// <param name="template">The person template containing base properties.</param>
    /// <param name="sensitivities">Factor sensitivities for the person.</param>
    /// <param name="tags">Tags for categorization.</param>
    /// <returns>A new person instance.</returns>
    /// <remarks>
    /// Base properties (MovingWillingness, RetentionRate, Tags, FactorSensitivities) are provided.
    /// Extract custom properties from <paramref name="template" />.CustomProperties if needed.
    /// </remarks>
    TPerson CreateFromTemplate(
        PersonTemplateXml template,
        Dictionary<FactorDefinition, double> sensitivities,
        List<string> tags);

    /// <summary>
    /// Serializes custom properties to XML.
    /// </summary>
    /// <param name="person">The person instance to serialize.</param>
    /// <param name="doc">XML document for creating elements.</param>
    /// <returns>An XML element containing custom properties, or null if no custom properties.</returns>
    /// <remarks>
    /// Only serialize properties beyond those in PersonBase and StandardPerson.
    /// Return null if the person type has no additional properties to serialize.
    /// </remarks>
    XmlElement? SerializeCustomProperties(TPerson person, XmlDocument doc);
}