using System.Xml;
using dotMigrata.Core.Entities;
using dotMigrata.Core.Values;
using dotMigrata.Generator;
using dotMigrata.Snapshot.Models;

namespace dotMigrata.Snapshot.Conversion;

/// <summary>
/// Defines serialization methods for custom person generators in snapshots.
/// </summary>
/// <typeparam name="TPerson">The custom person type derived from <see cref="PersonBase" />.</typeparam>
/// <typeparam name="TGenerator">The generator type implementing <see cref="IPersonGenerator{TPerson}" />.</typeparam>
/// <remarks>
/// Implement this interface to enable snapshot support for custom person generators.
/// Register implementations with <see cref="PersonTypeRegistry.RegisterGeneratorType{TPerson,TGenerator}" />.
/// </remarks>
public interface ICustomGeneratorSerializer<TPerson, TGenerator>
    where TPerson : PersonBase
    where TGenerator : IPersonGenerator<TPerson>
{
    /// <summary>
    /// Creates a generator instance from PersonSpecXml XML data.
    /// </summary>
    /// <param name="spec">The person specification containing base configuration.</param>
    /// <param name="factorSpecs">Factor sensitivity specifications.</param>
    /// <param name="tags">Tags to apply to generated persons.</param>
    /// <returns>A new generator instance.</returns>
    /// <remarks>
    /// Base specifications (Count, Seed, Willingness, Retention, etc.) are provided.
    /// Extract custom specifications from <paramref name="spec" />.CustomProperties if needed.
    /// </remarks>
    TGenerator CreateFromXml(
        PersonSpecXml spec,
        Dictionary<FactorDefinition, UnitValuePromise> factorSpecs,
        List<string> tags);

    /// <summary>
    /// Serializes custom generator properties to XML.
    /// </summary>
    /// <param name="generator">The generator instance to serialize.</param>
    /// <param name="doc">XML document for creating elements.</param>
    /// <returns>An XML element containing custom properties, or null if no custom properties.</returns>
    /// <remarks>
    /// Only serialize properties beyond those in the base generator specification.
    /// Return null if the generator type has no additional properties to serialize.
    /// </remarks>
    XmlElement? SerializeCustomProperties(TGenerator generator, XmlDocument doc);
}