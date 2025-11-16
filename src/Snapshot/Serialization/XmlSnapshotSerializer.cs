using System.Text;
using System.Xml;
using System.Xml.Serialization;
using dotMigrata.Snapshot.Models;

namespace dotMigrata.Snapshot.Serialization;

/// <summary>
/// XML snapshot serializer using C# XML serialization attributes.
/// <para>
/// Provides attribute-based XML serialization with namespace support:
/// <list type="bullet">
///     <item>
///         <description>Code concepts use 'c:' namespace (c:Person, c:City, c:FactorDefinition)</description>
///     </item>
///     <item>
///         <description>Snapshot containers use default namespace (PersonCollections, FactorDefinitions, Cities)</description>
///     </item>
///     <item>
///         <description>Automatic serialization/deserialization via System.Xml.Serialization</description>
///     </item>
///     <item>
///         <description>Deterministic XML output with proper formatting and namespace handling</description>
///     </item>
/// </list>
/// </para>
/// </summary>
public static class XmlSnapshotSerializer
{
    private static readonly XmlSerializer Serializer = new(typeof(WorldSnapshotXml));

    private static readonly XmlWriterSettings DefaultWriterSettings = new()
    {
        Indent = true,
        IndentChars = "  ",
        NewLineChars = "\n",
        NewLineHandling = NewLineHandling.Replace,
        OmitXmlDeclaration = false,
        Encoding = Encoding.UTF8,
        NamespaceHandling = NamespaceHandling.OmitDuplicates
    };

    private static readonly XmlSerializerNamespaces Namespaces = new();

    static XmlSnapshotSerializer()
    {
        // Configure XML namespaces
        Namespaces.Add("", "http://geomigrata.pages.dev/snapshot"); // Default namespace for snapshot structures
        Namespaces.Add("c", "http://geomigrata.pages.dev/code"); // Code namespace for framework classes
    }

    /// <summary>
    /// Serializes a snapshot to an XML string with proper namespace handling.
    /// </summary>
    /// <param name="snapshot">The snapshot to serialize.</param>
    /// <returns>XML string representation of the snapshot.</returns>
    public static string Serialize(WorldSnapshotXml snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, DefaultWriterSettings);
        Serializer.Serialize(xmlWriter, snapshot, Namespaces);
        return stringWriter.ToString();
    }

    /// <summary>
    /// Serializes a snapshot to an XML file with proper namespace handling.
    /// </summary>
    /// <param name="snapshot">The snapshot to serialize.</param>
    /// <param name="filePath">Path to the output XML file.</param>
    public static void SerializeToFile(WorldSnapshotXml snapshot, string filePath)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        using var xmlWriter = XmlWriter.Create(filePath, DefaultWriterSettings);
        Serializer.Serialize(xmlWriter, snapshot, Namespaces);
    }

    /// <summary>
    /// Deserializes a snapshot from an XML string.
    /// </summary>
    /// <param name="xml">XML string containing the snapshot.</param>
    /// <returns>Deserialized snapshot, or null if deserialization fails.</returns>
    public static WorldSnapshotXml? Deserialize(string xml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xml);

        using var stringReader = new StringReader(xml);
        return Serializer.Deserialize(stringReader) as WorldSnapshotXml;
    }

    /// <summary>
    /// Deserializes a snapshot from an XML file.
    /// </summary>
    /// <param name="filePath">Path to the XML file.</param>
    /// <returns>Deserialized snapshot, or null if deserialization fails.</returns>
    public static WorldSnapshotXml? DeserializeFromFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        using var fileStream = File.OpenRead(filePath);
        return Serializer.Deserialize(fileStream) as WorldSnapshotXml;
    }
}