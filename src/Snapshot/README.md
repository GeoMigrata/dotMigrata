# Snapshot System Documentation

## Overview

The Snapshot system in dotGeoMigrata provides serialization and deserialization capabilities for the simulation world,
configuration, and state. It supports both JSON and XML formats with consistent naming conventions and ID strategies.

## Architecture

### Components

```
Snapshot/
├── Models/              # Data transfer objects for snapshots
│   ├── WorldSnapshot.cs
│   ├── InitializationSnapshot.cs
│   ├── CitySnapshot.cs
│   ├── FactorDefinitionSnapshot.cs
│   └── ...
├── Services/            # Snapshot service implementation
│   ├── ISnapshotService.cs
│   └── SnapshotService.cs
├── Serialization/       # Format-specific serializers
│   ├── JsonSnapshotSerializer.cs
│   └── XmlSnapshotSerializer.cs
└── Extensions/          # Helper extension methods
    └── SnapshotExtensions.cs
```

### Design Principles

1. **Separation of Concerns**: Domain objects remain pure, DTOs handle serialization
2. **ID-based References**: Objects are referenced by IDs in snapshots, converted to object references in domain
3. **Format-Specific Conventions**: Each format follows its own best practices
4. **Immutability**: Domain objects use object references; IDs only exist in serialization layer

## Naming Conventions

### JSON Format

#### Structure

- **Metadata fields**: Use underscore prefix to distinguish from data
    - `_version`: Format version
    - `_initialization`: World structure and definitions
    - `_simulationConfig`: Configuration parameters
    - `_simulation`: Current state

- **Data fields**: Use camelCase
    - `displayName`, `factorDefinitions`, `populationGroupDefinitions`, `cities`

#### ID Convention

Use meaningful names for readability:

```json
{
  "_initialization": {
    "factorDefinitions": {
      "pollution": {
        ...
      },
      "income": {
        ...
      },
      "education": {
        ...
      }
    },
    "populationGroupDefinitions": {
      "youngProfessionals": {
        ...
      },
      "families": {
        ...
      }
    },
    "cities": {
      "beijing": {
        ...
      },
      "shanghai": {
        ...
      }
    }
  }
}
```

### XML Format

#### Structure

- **XML structural attributes**: Lowercase
    - `id`: Element identifier
    - `ref`: Reference to another element
    - `version`: Format version

- **Data attributes**: PascalCase (matching C# DTOs)
    - `DisplayName`, `Intensity`, `Population`, `MovingWillingness`

#### ID Convention

Use prefixed numeric IDs for clarity:

```xml

<World version="1.0" DisplayName="Example World">
    <World.FactorDefinitions>
        <FactorDefinition id="fd_0" DisplayName="Pollution"
        .../>
        <FactorDefinition id="fd_1" DisplayName="Income"
        .../>
    </World.FactorDefinitions>

    <World.PopulationGroupDefinitions>
        <PopulationGroupDefinition id="pgd_0" DisplayName="Young Professionals"
        ...>
        <PopulationGroupDefinition.Sensitivities>
            <FactorSensitivity ref="fd_0" Sensitivity="8"/>
            <FactorSensitivity ref="fd_1" Sensitivity="10"/>
        </PopulationGroupDefinition.Sensitivities>
    </PopulationGroupDefinition>
</World.PopulationGroupDefinitions>

<World.Cities>
<City DisplayName="Beijing"
...>
<City.FactorValues>
    <FactorValue ref="fd_0" Intensity="45"/>
</City.FactorValues>
</City>
        </World.Cities>
        </World>
```

## ID Strategy

### Purpose of IDs

IDs are **only used for serialization** to:

1. Establish references between objects in the snapshot
2. Maintain order-independence during deserialization
3. Support human-readable snapshots

### ID Lifecycle

```
Serialization:
  Domain Object (object reference) 
    → ID assigned based on collection index
      → Serialized with ID
Deserialization:
  Snapshot with IDs
    → Build lookup dictionary (ID → Object)
      → Resolve references using lookup
        → Domain Object (object reference)
```

### ID Patterns

| Type              | JSON ID Example        | XML ID Example | Pattern                          |
|-------------------|------------------------|----------------|----------------------------------|
| Factor Definition | `"pollution"`          | `"fd_0"`       | Meaningful name / `fd_{index}`   |
| Population Group  | `"youngProfessionals"` | `"pgd_0"`      | Meaningful name / `pgd_{index}`  |
| City              | `"beijing"`            | `"city_0"`     | Meaningful name / `city_{index}` |

**Note**: JSON allows meaningful names for better readability, while XML uses numeric IDs for consistency. Both are
converted to object references in the domain model.

## Usage Examples

### Exporting a Snapshot

```csharp
using dotGeoMigrata.Snapshot.Services;

var snapshotService = new SnapshotService();

// Export world with optional config and state
var snapshot = snapshotService.ExportToSnapshot(
    world, 
    configuration, 
    simulationState);

// Save as JSON
await snapshotService.SaveJsonAsync(snapshot, "world.json");

// Save as XML
await snapshotService.SaveXmlAsync(snapshot, "world.xml");
```

### Importing a Snapshot

```csharp
// Load from JSON
var snapshot = await snapshotService.LoadJsonAsync("world.json");

// Import world (converts IDs to object references)
var world = snapshotService.ImportWorld(snapshot);

// Import configuration if present
var config = snapshotService.ImportSimulationConfiguration(snapshot);
```

### JSON Example

```json
{
  "displayName": "Example World",
  "_version": "1.0",
  "_initialization": {
    "factorDefinitions": {
      "pollution": {
        "displayName": "Pollution",
        "type": "Negative",
        "minValue": "0",
        "maxValue": "100",
        "transform": "Linear"
      }
    },
    "populationGroupDefinitions": {
      "youngProfessionals": {
        "displayName": "Young Professionals",
        "movingWillingness": 0.8,
        "retentionRate": 0.3,
        "factorSensitivities": {
          "pollution": {
            "sensitivity": 8
          }
        }
      }
    },
    "cities": {
      "beijing": {
        "displayName": "Beijing",
        "area": 16410,
        "location": {
          "latitude": 39.9042,
          "longitude": 116.4074
        },
        "factorValues": {
          "pollution": {
            "intensity": 75
          }
        },
        "populationGroupValues": {
          "youngProfessionals": {
            "population": 5000000
          }
        }
      }
    }
  },
  "_simulationConfig": {
    "maxSteps": 100,
    "stabilizationThreshold": 0.01,
    "checkStabilization": true,
    "feedbackSmoothingFactor": 0.3,
    "randomSeed": 42
  }
}
```

## Best Practices

### When Creating Snapshots

1. **Use Meaningful IDs in JSON**: Helps with readability and debugging
2. **Keep Consistent ID Patterns**: Use the documented patterns for clarity
3. **Include Version**: Always specify the format version for future compatibility
4. **Validate Before Saving**: Ensure world structure is valid

### When Modifying Snapshots

1. **Don't Break References**: Ensure all referenced IDs exist
2. **Maintain Structure**: Follow the established schema
3. **Preserve Type Information**: Keep type strings valid (e.g., "Positive", "Negative")
4. **Check Number Formats**: Use appropriate precision for numeric values

### When Extending the System

1. **Update Both Formats**: Keep JSON and XML in sync
2. **Document New Fields**: Update this documentation
3. **Maintain Backward Compatibility**: Consider version migration if breaking changes
4. **Test Both Serializers**: Verify round-trip (save → load) works

## Schema Validation

### JSON Schema

To be added: A JSON schema file for validation.

### XML Schema

See `example/SnapshotSchema.xsd` for the XML schema definition.

## Error Handling

The snapshot system provides detailed error messages:

```csharp
try {
    var world = snapshotService.ImportWorld(snapshot);
} 
catch (InvalidOperationException ex) {
    // Common errors:
    // - "Factor 'fd_5' not found in factor definitions."
    // - "Population group 'pgd_3' not found in population group definitions."
    // - "Factor definition not found in collection."
    Console.WriteLine($"Import error: {ex.Message}");
}
```

## Performance Considerations

### Memory

- Snapshots hold all data in memory during serialization/deserialization
- Large worlds (many cities/factors) may require significant memory

### Speed

- JSON is generally faster for small to medium snapshots
- XML provides better structure for very large, hierarchical data
- Consider async operations for I/O-bound operations

### Optimization Tips

1. Use streaming for very large snapshots (future enhancement)
2. Cache deserialized lookups if loading multiple times
3. Consider compression for large snapshot files

## Version Management

Current version: **1.0**

### Future Versions

When introducing breaking changes:

1. Increment version number
2. Implement migration logic
3. Support reading older versions
4. Document migration guide

## Troubleshooting

### Common Issues

**Issue**: "Factor not found in factor definitions"

- **Cause**: ID reference doesn't exist in definitions section
- **Solution**: Verify all referenced IDs are defined

**Issue**: "Cannot implicitly convert type"

- **Cause**: Type mismatch in snapshot data
- **Solution**: Check numeric types and enum strings

**Issue**: "Invalid factor type: XYZ"

- **Cause**: Invalid enum value in snapshot
- **Solution**: Use valid values: "Positive", "Negative"

**Issue**: XML deserialization fails silently

- **Cause**: Attribute name mismatch
- **Solution**: Verify attribute names match schema (case-sensitive)

## See Also

- [Main README](../../../README.md) - Project overview
- [Logic Layer Documentation](../../Logic/README.md) - Core algorithms
- [Simulation Engine Documentation](../../Simulation/README.md) - Simulation design
- [Example Snapshots](../../../example/) - Sample files