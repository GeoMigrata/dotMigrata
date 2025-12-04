using System.Text;
using System.Xml;
using System.Xml.Serialization;
using dotMigrata.Snapshot.Models;

namespace dotMigrata.Snapshot.Serialization;

/// <summary>
/// XML snapshot serializer using C# XML serialization attributes.
/// </summary>
/// <remarks>
///     <para>Provides attribute-based XML serialization with v2.0 format:</para>
///     <list type="bullet">
///         <item>
///             <description>Single namespace for all elements</description>
///         </item>
///         <item>
///             <description>Attribute-based configuration for scalar values</description>
///         </item>
///         <item>
///             <description>Shorter element names for reduced file size</description>
///         </item>
///     </list>
/// </remarks>
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
        // Single namespace for all elements (v2.0 simplified format)
        Namespaces.Add("", "http://geomigrata.pages.dev/snapshot");
    }

    /// <summary>
    /// Serializes a snapshot to an XML string with proper namespace handling.
    /// </summary>
    /// <param name="snapshot">The snapshot to serialize.</param>
    /// <returns>XML string representation of the snapshot.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="snapshot" /> is <see langword="null" />.
    /// </exception>
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
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="snapshot" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="filePath" /> is <see langword="null" /> or whitespace.
    /// </exception>
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
    /// <returns>
    /// Deserialized <see cref="WorldSnapshotXml" />, or <see langword="null" /> if deserialization fails.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="xml" /> is <see langword="null" /> or whitespace.
    /// </exception>
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
    /// <returns>
    /// Deserialized <see cref="WorldSnapshotXml" />, or <see langword="null" /> if deserialization fails.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="filePath" /> is <see langword="null" /> or whitespace.
    /// </exception>
    public static WorldSnapshotXml? DeserializeFromFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        using var fileStream = File.OpenRead(filePath);
        return Serializer.Deserialize(fileStream) as WorldSnapshotXml;
    }
}