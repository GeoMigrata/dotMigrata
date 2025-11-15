using System.Text.Json;
using System.Text.Json.Serialization;
using dotGeoMigrata.Snapshot.Models;

namespace dotGeoMigrata.Snapshot.Serialization;

/// <summary>
/// JSON serializer for world snapshots with enhanced formatting and configuration options.
/// </summary>
public static class JsonSnapshotSerializer
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private static readonly JsonSerializerOptions CompactOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes a world snapshot to JSON.
    /// </summary>
    /// <param name="snapshot">The snapshot to serialize.</param>
    /// <param name="options">Optional JSON serializer options. Uses default formatted options if null.</param>
    /// <returns>JSON string representation.</returns>
    public static string Serialize(WorldSnapshot snapshot, JsonSerializerOptions? options = null) =>
        JsonSerializer.Serialize(snapshot, options ?? DefaultOptions);

    /// <summary>
    /// Serializes a snapshot to a JSON file.
    /// </summary>
    /// <param name="snapshot">The snapshot to serialize.</param>
    /// <param name="filePath">The file path to write to.</param>
    /// <param name="options">Optional JSON serializer options. Uses default formatted options if null.</param>
    public static void SerializeToFile(WorldSnapshot snapshot, string filePath,
        JsonSerializerOptions? options = null) =>
        File.WriteAllText(filePath, Serialize(snapshot, options));

    /// <summary>
    /// Serializes a snapshot to a JSON file asynchronously.
    /// </summary>
    /// <param name="snapshot">The snapshot to serialize.</param>
    /// <param name="filePath">The file path to write to.</param>
    /// <param name="options">Optional JSON serializer options. Uses default formatted options if null.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    public static async Task SerializeToFileAsync(
        WorldSnapshot snapshot,
        string filePath,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, snapshot, options ?? DefaultOptions, cancellationToken);
    }

    /// <summary>
    /// Deserializes a world snapshot from JSON.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="options">Optional JSON serializer options. Uses default options if null.</param>
    /// <returns>The deserialized world snapshot, or null if deserialization fails.</returns>
    public static WorldSnapshot? Deserialize(string json, JsonSerializerOptions? options = null) =>
        JsonSerializer.Deserialize<WorldSnapshot>(json, options ?? DefaultOptions);

    /// <summary>
    /// Deserializes a snapshot from a JSON file.
    /// </summary>
    /// <param name="filePath">The file path to read from.</param>
    /// <param name="options">Optional JSON serializer options. Uses default options if null.</param>
    /// <returns>The deserialized world snapshot, or null if deserialization fails.</returns>
    public static WorldSnapshot? DeserializeFromFile(string filePath, JsonSerializerOptions? options = null) =>
        Deserialize(File.ReadAllText(filePath), options);

    /// <summary>
    /// Deserializes a snapshot from a JSON file asynchronously.
    /// </summary>
    /// <param name="filePath">The file path to read from.</param>
    /// <param name="options">Optional JSON serializer options. Uses default options if null.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The deserialized world snapshot, or null if deserialization fails.</returns>
    public static async Task<WorldSnapshot?> DeserializeFromFileAsync(
        string filePath,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<WorldSnapshot>(stream, options ?? DefaultOptions,
            cancellationToken);
    }

    /// <summary>
    /// Gets the default JSON serializer options (formatted with camelCase naming).
    /// </summary>
    public static JsonSerializerOptions GetDefaultOptions() => new(DefaultOptions);

    /// <summary>
    /// Gets compact JSON serializer options (no indentation, camelCase naming).
    /// </summary>
    public static JsonSerializerOptions GetCompactOptions() => new(CompactOptions);
}