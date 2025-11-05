﻿# Source Code Organization

This directory contains the source code for the dotGeoMigrata simulation framework.

## Directory Structure

```
src/
├── Core/           # Core domain models and entities
├── Logic/          # Business logic for calculations
├── Simulation/     # Simulation orchestration and state
└── Snapshot/       # Serialization and deserialization
```

## Layer Descriptions

### Core
**Path:** `src/Core/`

Contains the fundamental domain models representing the world, cities, populations, and factors.

**Sub-directories:**
- **Entities**: Main domain entities (World, City)
- **Values**: Value objects (FactorDefinition, FactorValue, FactorSensitivity, PopulationGroupDefinition, Coordinate)
- **Enums**: Enumeration types (FactorType, TransformType)

**Key Classes:**
- `World` - Top-level container for the entire simulation
- `City` - Represents a city with location, factors, and populations
- `PopulationGroupDefinition` - Defines a population group with migration behavior
- `PopulationGroupValue` - Represents population count for a group in a city
- `FactorDefinition` - Defines a factor's metadata and normalization
- `FactorValue` - A factor's current value in a city
- `Coordinate` - Geographic position with distance calculation

**Characteristics:**
- Pure domain models
- No dependencies on other layers
- Immutable where possible
- Rich validation in constructors/initializers
- Uses object references (not IDs) for type safety

---

### Logic
**Path:** `src/Logic/`

Contains the algorithms and calculators that implement the simulation's core logic.

**Sub-directories:**
- **Attraction**: City attractiveness calculation
- **Migration**: Migration flow determination
- **Feedback**: Post-migration factor updates

**Key Classes:**
- `AttractionCalculator` - Computes city attractiveness for population groups
- `MigrationCalculator` - Determines migration flows based on attraction differences
- `FeedbackCalculator` - Updates city factors after migration

**See:** [Logic README](Logic/README.md) for detailed algorithm documentation

---

### Simulation
**Path:** `src/Simulation/`

Contains the simulation engine that orchestrates the step-by-step execution.

**Sub-directories:**
- **Engine**: Main simulation orchestrator and observers
- **Configuration**: Simulation parameters
- **State**: Runtime state tracking

**Key Classes:**
- `SimulationEngine` - Main orchestrator coordinating all calculations
- `SimulationConfiguration` - Configuration parameters (max steps, thresholds, etc.)
- `SimulationState` - Tracks current step, migrations, and status
- `ISimulationObserver` - Interface for monitoring simulation progress
- `ConsoleSimulationObserver` - Built-in console logger

**See:** [Simulation README](Simulation/README.md) for detailed engine design documentation

---

### Snapshot
**Path:** `src/Snapshot/`

Contains serialization and deserialization for persisting and loading simulation worlds.

**Sub-directories:**
- **Models**: Data transfer objects for snapshots
- **Services**: Snapshot service interface and implementation
- **Serialization**: Format-specific serializers (JSON, XML)
- **Extensions**: Helper extension methods

**Key Classes:**
- `ISnapshotService` - Interface for snapshot operations
- `SnapshotService` - Main service for exporting/importing snapshots
- `JsonSnapshotSerializer` - JSON format serializer
- `XmlSnapshotSerializer` - XML format serializer
- `WorldSnapshot` - Complete snapshot structure

**Features:**
- Support for JSON and XML formats
- ID-based references in snapshots, converted to object references in domain
- Standardized naming conventions (underscore prefix for metadata in JSON)
- Comprehensive error handling and validation

**See:** [Snapshot README](Snapshot/README.md) for detailed snapshot system documentation

---

## Dependency Flow

```
Snapshot ←→ Simulation
              ↓ (depends on)
            Logic
              ↓ (depends on)
            Core
```

- **Core** has no dependencies (pure domain)
- **Logic** depends on Core (uses domain models)
- **Simulation** depends on Logic and Core (orchestrates everything)
- **Snapshot** depends on Core and Simulation (serializes domain objects and state)

---

## Namespace Convention

All namespaces follow the pattern: `dotGeoMigrata.{Layer}.{SubFolder}`

Examples:
- `dotGeoMigrata.Core.Entities`
- `dotGeoMigrata.Core.Values`
- `dotGeoMigrata.Logic.Attraction`
- `dotGeoMigrata.Simulation.Engine`
- `dotGeoMigrata.Snapshot.Services`

---

## Design Principles

1. **Separation of Concerns**: Each layer has a distinct responsibility
2. **Domain-Driven Design**: Core models represent the business domain
3. **Single Responsibility**: Each class has one clear purpose
4. **Dependency Inversion**: Higher layers depend on abstractions
5. **Immutability**: Prefer immutable objects where appropriate
6. **Validation**: Validate at construction time
7. **Type Safety**: Use object references instead of IDs in domain layer

---

## Extension Points

### Adding New Calculators
1. Create a new class in `Logic/`
2. Follow the pattern of existing calculators
3. Add comprehensive XML documentation
4. Document algorithms in `Logic/README.md`

### Adding New Observers
1. Implement `ISimulationObserver` interface
2. Add to simulation via `engine.AddObserver()`
3. React to simulation events as needed

### Adding New Domain Entities
1. Add to `Core/Entities/` or `Core/Values/`
2. Follow existing patterns (records for values, classes for entities)
3. Add validation in initializers
4. Document with XML comments

### Extending Snapshot Support
1. Update snapshot models in `Snapshot/Models/`
2. Update both JSON and XML serializers
3. Maintain backward compatibility when possible
4. Document changes in `Snapshot/README.md`

---

## Code Quality Standards

- **XML Documentation**: All public types and members must have `/// <summary>` comments
- **Validation**: Validate inputs in constructors and property initializers
- **Naming**: Use clear, descriptive names following C# conventions
- **Null Safety**: Use C#'s nullable reference types (`enable` in project)
- **Exception Messages**: Provide clear, actionable error messages with parameter names
- **Readonly**: Use `readonly` for fields that shouldn't change
- **Modern C#**: Leverage C# 12.0 and .NET 9.0 features where appropriate

---

## Getting Started

To use this framework:

1. **Create or Load a World**:
   ```csharp
   // Option 1: Create programmatically
   var world = new World(cities, factorDefinitions, populationGroupDefinitions) 
   {
       DisplayName = "My World"
   };
   
   // Option 2: Load from snapshot
   var snapshotService = new SnapshotService();
   var snapshot = await snapshotService.LoadJsonAsync("world.json");
   var world = snapshotService.ImportWorld(snapshot);
   ```

2. **Configure Simulation**:
   ```csharp
   var config = new SimulationConfiguration 
   { 
       MaxSteps = 100,
       StabilizationThreshold = 0.01,
       CheckStabilization = true,
       FeedbackSmoothingFactor = 0.3,
       RandomSeed = 42  // For reproducibility
   };
   ```

3. **Create Engine and Add Observers**:
   ```csharp
   var engine = new SimulationEngine(world, config);
   engine.AddObserver(new ConsoleSimulationObserver(verbose: true));
   ```

4. **Run Simulation**:
   ```csharp
   engine.Run();
   ```

5. **Save Results** (Optional):
   ```csharp
   var resultSnapshot = snapshotService.ExportToSnapshot(
       world, config, engine.State);
   await snapshotService.SaveJsonAsync(resultSnapshot, "results.json");
   ```

See the README files in each subdirectory for detailed documentation.