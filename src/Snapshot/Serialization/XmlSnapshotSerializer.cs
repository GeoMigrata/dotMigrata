using System.Text;
using System.Xml;
using System.Xml.Serialization;
using dotMigrata.Core.Exceptions;
using dotMigrata.Snapshot.Models;

namespace dotMigrata.Snapshot.Serialization;

/// <summary>
/// XML snapshot serializer using C# XML serialization attributes.
/// </summary>
/// <remarks>
///     Provides attribute-based XML serialization with features including:
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
///         <item>
///             <description>Version tracking for backwards compatibility</description>
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

    static XmlSnapshotSerializer() =>
        Namespaces.Add("", "https://geomigrata.pages.dev/snapshot"); // Single namespace for all elements

    /// <summary>
    /// Serializes a snapshot to an XML string with proper namespace handling.
    /// </summary>
    /// <param name="snapshot">The snapshot to serialize.</param>
    /// <returns>XML string representation of the snapshot.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="snapshot" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="SnapshotException">
    /// Thrown when serialization fails.
    /// </exception>
    public static string Serialize(WorldSnapshotXml snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        try
        {
            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, DefaultWriterSettings);
            Serializer.Serialize(xmlWriter, snapshot, Namespaces);
            return stringWriter.ToString();
        }
        catch (Exception ex) when (ex is not SnapshotException)
        {
            throw new SnapshotException("Failed to serialize snapshot to XML.", ex);
        }
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
    /// <exception cref="SnapshotException">
    /// Thrown when serialization fails.
    /// </exception>
    public static void SerializeToFile(WorldSnapshotXml snapshot, string filePath)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        try
        {
            using var xmlWriter = XmlWriter.Create(filePath, DefaultWriterSettings);
            Serializer.Serialize(xmlWriter, snapshot, Namespaces);
        }
        catch (Exception ex) when (ex is not SnapshotException)
        {
            throw new SnapshotException($"Failed to serialize snapshot to file: {filePath}", ex)
            {
                FilePath = filePath
            };
        }
    }

    /// <summary>
    /// Deserializes a snapshot from an XML string.
    /// </summary>
    /// <param name="xml">XML string containing the snapshot.</param>
    /// <returns>Deserialized <see cref="WorldSnapshotXml" />.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="xml" /> is <see langword="null" /> or whitespace.
    /// </exception>
    /// <exception cref="SnapshotException">
    /// Thrown when deserialization fails or the snapshot is invalid.
    /// </exception>
    public static WorldSnapshotXml Deserialize(string xml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xml);

        try
        {
            using var stringReader = new StringReader(xml);
            var snapshot = Serializer.Deserialize(stringReader) as WorldSnapshotXml;

            return snapshot ?? throw new SnapshotException("Deserialization resulted in null snapshot.");
        }
        catch (Exception ex) when (ex is not SnapshotException)
        {
            throw new SnapshotException("Failed to deserialize snapshot from XML string.", ex);
        }
    }

    /// <summary>
    /// Deserializes a snapshot from an XML file.
    /// </summary>
    /// <param name="filePath">Path to the XML file.</param>
    /// <returns>Deserialized <see cref="WorldSnapshotXml" />.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="filePath" /> is <see langword="null" /> or whitespace.
    /// </exception>
    /// <exception cref="SnapshotException">
    /// Thrown when the file cannot be read or deserialization fails.
    /// </exception>
    public static WorldSnapshotXml DeserializeFromFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        try
        {
            using var fileStream = File.OpenRead(filePath);
            var snapshot = Serializer.Deserialize(fileStream) as WorldSnapshotXml;

            if (snapshot == null)
                throw new SnapshotException($"Deserialization of file '{filePath}' resulted in null snapshot.")
                {
                    FilePath = filePath
                };

            return snapshot;
        }
        catch (Exception ex) when (ex is not SnapshotException)
        {
            throw new SnapshotException($"Failed to deserialize snapshot from file: {filePath}", ex)
            {
                FilePath = filePath
            };
        }
    }

    /// <summary>
    /// Attempts to deserialize a snapshot from an XML file without throwing exceptions.
    /// </summary>
    /// <param name="filePath">Path to the XML file.</param>
    /// <param name="snapshot">When this method returns, contains the deserialized snapshot if successful; otherwise, null.</param>
    /// <param name="error">When this method returns, contains the error message if deserialization failed; otherwise, null.</param>
    /// <returns>
    /// <see langword="true" /> if deserialization was successful; otherwise, <see langword="false" />.
    /// </returns>
    public static bool TryDeserializeFromFile(string filePath, out WorldSnapshotXml? snapshot, out string? error)
    {
        snapshot = null;
        error = null;

        try
        {
            snapshot = DeserializeFromFile(filePath);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Validates a snapshot file without fully deserializing it.
    /// </summary>
    /// <param name="filePath">Path to the XML file to validate.</param>
    /// <returns>
    /// <see langword="true" /> if the file is a valid snapshot; otherwise, <see langword="false" />.
    /// </returns>
    public static bool ValidateSnapshot(string filePath)
    {
        return TryDeserializeFromFile(filePath, out _, out _);
    }
}