using System.Text.Json;
using System.Text.Json.Serialization;
using dotGeoMigrata.Snapshot.Models;

namespace dotGeoMigrata.Snapshot.Serialization;

/// <summary>
/// Provides JSON serialization and deserialization for world snapshots.
/// Uses System.Text.Json with modern C# features.
/// </summary>
public static class JsonSnapshotSerializer
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Serializes a world snapshot to JSON.
    /// </summary>
    /// <param name="snapshot">The snapshot to serialize.</param>
    /// <param name="options">Optional JSON serializer options. If null, uses default options.</param>
    /// <returns>JSON string representation of the snapshot.</returns>
    public static string Serialize(WorldSnapshot snapshot, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return JsonSerializer.Serialize(snapshot, options ?? DefaultOptions);
    }

    /// <summary>
    /// Serializes a world snapshot to a JSON file.
    /// </summary>
    /// <param name="snapshot">The snapshot to serialize.</param>
    /// <param name="filePath">The file path where the JSON will be saved.</param>
    /// <param name="options">Optional JSON serializer options. If null, uses default options.</param>
    public static void SerializeToFile(WorldSnapshot snapshot, string filePath, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var json = Serialize(snapshot, options);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Deserializes a world snapshot from JSON.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="options">Optional JSON serializer options. If null, uses default options.</param>
    /// <returns>The deserialized world snapshot.</returns>
    public static WorldSnapshot? Deserialize(string json, JsonSerializerOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        return JsonSerializer.Deserialize<WorldSnapshot>(json, options ?? DefaultOptions);
    }

    /// <summary>
    /// Deserializes a world snapshot from a JSON file.
    /// </summary>
    /// <param name="filePath">The file path to read from.</param>
    /// <param name="options">Optional JSON serializer options. If null, uses default options.</param>
    /// <returns>The deserialized world snapshot.</returns>
    public static WorldSnapshot? DeserializeFromFile(string filePath, JsonSerializerOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Snapshot file not found: {filePath}");

        var json = File.ReadAllText(filePath);
        return Deserialize(json, options);
    }
}