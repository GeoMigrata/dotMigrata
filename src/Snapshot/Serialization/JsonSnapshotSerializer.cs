using System.Text.Json;
using System.Text.Json.Serialization;
using dotGeoMigrata.Snapshot.Models;

namespace dotGeoMigrata.Snapshot.Serialization;

/// <summary>
/// Provides JSON serialization and deserialization for snapshots.
/// </summary>
public static class JsonSnapshotSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a WorldSnapshot to JSON format.
    /// </summary>
    /// <param name="snapshot">The snapshot to serialize.</param>
    /// <returns>JSON string representation.</returns>
    public static string Serialize(WorldSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return JsonSerializer.Serialize(snapshot, Options);
    }

    /// <summary>
    /// Deserializes a WorldSnapshot from JSON format.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized WorldSnapshot.</returns>
    public static WorldSnapshot Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var snapshot = JsonSerializer.Deserialize<WorldSnapshot>(json, Options);
        return snapshot ?? throw new InvalidOperationException("Failed to deserialize snapshot from JSON.");
    }

    /// <summary>
    /// Asynchronously saves a WorldSnapshot to a JSON file.
    /// </summary>
    /// <param name="snapshot">The snapshot to save.</param>
    /// <param name="filePath">The file path to save to.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    public static async Task SaveToFileAsync(WorldSnapshot snapshot, string filePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var json = Serialize(snapshot);
        await File.WriteAllTextAsync(filePath, json, cancellationToken);
    }

    /// <summary>
    /// Asynchronously loads a WorldSnapshot from a JSON file.
    /// </summary>
    /// <param name="filePath">The file path to load from.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The loaded WorldSnapshot.</returns>
    public static async Task<WorldSnapshot> LoadFromFileAsync(string filePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Snapshot file not found: {filePath}");

        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        return Deserialize(json);
    }
}