using System.Xml;
using System.Xml.Serialization;
using dotGeoMigrata.Snapshot.Models;

namespace dotGeoMigrata.Snapshot.Serialization;

/// <summary>
/// Provides XML serialization and deserialization for world snapshots.
/// Uses System.Xml.Serialization with proper formatting.
/// </summary>
public static class XmlSnapshotSerializer
{
    private static readonly XmlSerializer Serializer = new(typeof(WorldSnapshot));

    private static readonly XmlWriterSettings DefaultWriterSettings = new()
    {
        Indent = true,
        IndentChars = "  ",
        OmitXmlDeclaration = false,
        Encoding = System.Text.Encoding.UTF8
    };

    private static readonly XmlReaderSettings DefaultReaderSettings = new()
    {
        IgnoreWhitespace = true,
        IgnoreComments = true
    };

    /// <summary>
    /// Serializes a world snapshot to XML.
    /// </summary>
    /// <param name="snapshot">The snapshot to serialize.</param>
    /// <param name="writerSettings">Optional XML writer settings. If null, uses default settings.</param>
    /// <returns>XML string representation of the snapshot.</returns>
    public static string Serialize(WorldSnapshot snapshot, XmlWriterSettings? writerSettings = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, writerSettings ?? DefaultWriterSettings);

        Serializer.Serialize(xmlWriter, snapshot);
        return stringWriter.ToString();
    }

    /// <summary>
    /// Serializes a world snapshot to an XML file.
    /// </summary>
    /// <param name="snapshot">The snapshot to serialize.</param>
    /// <param name="filePath">The file path where the XML will be saved.</param>
    /// <param name="writerSettings">Optional XML writer settings. If null, uses default settings.</param>
    public static void SerializeToFile(WorldSnapshot snapshot, string filePath,
        XmlWriterSettings? writerSettings = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        using var xmlWriter = XmlWriter.Create(filePath, writerSettings ?? DefaultWriterSettings);
        Serializer.Serialize(xmlWriter, snapshot);
    }

    /// <summary>
    /// Deserializes a world snapshot from XML.
    /// </summary>
    /// <param name="xml">The XML string to deserialize.</param>
    /// <param name="readerSettings">Optional XML reader settings. If null, uses default settings.</param>
    /// <returns>The deserialized world snapshot.</returns>
    public static WorldSnapshot? Deserialize(string xml, XmlReaderSettings? readerSettings = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xml);

        using var stringReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(stringReader, readerSettings ?? DefaultReaderSettings);

        return Serializer.Deserialize(xmlReader) as WorldSnapshot;
    }

    /// <summary>
    /// Deserializes a world snapshot from an XML file.
    /// </summary>
    /// <param name="filePath">The file path to read from.</param>
    /// <param name="readerSettings">Optional XML reader settings. If null, uses default settings.</param>
    /// <returns>The deserialized world snapshot.</returns>
    public static WorldSnapshot? DeserializeFromFile(string filePath, XmlReaderSettings? readerSettings = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Snapshot file not found: {filePath}");

        using var xmlReader = XmlReader.Create(filePath, readerSettings ?? DefaultReaderSettings);
        return Serializer.Deserialize(xmlReader) as WorldSnapshot;
    }
}