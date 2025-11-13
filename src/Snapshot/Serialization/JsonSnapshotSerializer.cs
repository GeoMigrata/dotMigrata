using dotGeoMigrata.Snapshot.Models;

namespace dotGeoMigrata.Snapshot.Serialization;

/// <summary>
/// JSON serializer for world snapshots.
/// NOTE: This is a stub implementation. Full person-based snapshot serialization is pending.
/// </summary>
public static class JsonSnapshotSerializer
{
    /// <summary>
    /// Serializes a world snapshot to JSON.
    /// </summary>
    /// <param name="snapshot">The snapshot to serialize.</param>
    /// <param name="options">Optional JSON serializer options.</param>
    /// <returns>JSON string representation.</returns>
    public static string Serialize(WorldSnapshot snapshot, System.Text.Json.JsonSerializerOptions? options = null)
    {
        return System.Text.Json.JsonSerializer.Serialize(snapshot, options);
    }

    /// <summary>
    /// Serializes a snapshot to a file.
    /// </summary>
    public static void SerializeToFile(WorldSnapshot snapshot, string filePath,
        System.Text.Json.JsonSerializerOptions? options = null)
    {
        var json = Serialize(snapshot, options);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Deserializes a world snapshot from JSON.
    /// </summary>
    public static WorldSnapshot? Deserialize(string json, System.Text.Json.JsonSerializerOptions? options = null)
    {
        return System.Text.Json.JsonSerializer.Deserialize<WorldSnapshot>(json, options);
    }

    /// <summary>
    /// Deserializes a snapshot from a file.
    /// </summary>
    public static WorldSnapshot? DeserializeFromFile(string filePath,
        System.Text.Json.JsonSerializerOptions? options = null)
    {
        var json = File.ReadAllText(filePath);
        return Deserialize(json, options);
    }
}