using System.Text.Json;
using System.Text.Json.Serialization;
using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Interfaces;

namespace dotGeoMigrata.Simulation.Engine;

/// <summary>
/// Manages simulation snapshots (serialized World states).
/// </summary>
internal sealed class SnapshotManager : IIdentifiable
{
    public string Id { get; init; }
    public string? DisplayName { get; init; }

    private readonly World _world;
    private readonly Dictionary<int, string> _snapshots = [];

    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public SnapshotManager(World world, string id, string? displayName = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id of SnapshotManager must be non-empty", nameof(id));
        Id = id;
        DisplayName = displayName;
        _world = world ?? throw new ArgumentNullException(nameof(world));
    }

    public void Record(int step)
    {
        var json = JsonSerializer.Serialize(_world, _jsonOptions);
        _snapshots[step] = json;
    }

    public bool TryRestore(int step, out World? restored)
    {
        restored = null;
        if (!_snapshots.TryGetValue(step, out var json)) return false;
        restored = JsonSerializer.Deserialize<World>(json, _jsonOptions);
        return restored != null;
    }

    public void SaveToFile(int step, string path)
    {
        if (!_snapshots.TryGetValue(step, out var json))
            throw new InvalidOperationException($"No snapshot found for step {step}");
        File.WriteAllText(path, json);
    }

    public void LoadFromFile(int step, string path)
    {
        var json = File.ReadAllText(path);
        _snapshots[step] = json;
    }

    public void Clear() => _snapshots.Clear();
}