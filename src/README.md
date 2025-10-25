# Source Code Organization

This directory contains the source code for the dotGeoMigrata simulation framework.

## Directory Structure

```
src/
├── Core/           # Core domain models and entities
├── Logic/          # Business logic for calculations
└── Simulation/     # Simulation orchestration and state
```

## Layer Descriptions

### Core
**Path:** `src/Core/Domain/`

Contains the fundamental domain models representing the world, cities, populations, and factors.

**Sub-directories:**
- **Entities**: Main domain entities (World, City, PopulationGroup)
- **Values**: Value objects (FactorDefinition, FactorValue, FactorSensitivity, Coordinate)
- **Enums**: Enumeration types (FactorType, TransformType)

**Key Classes:**
- `World` - Top-level container for the entire simulation
- `City` - Represents a city with location, factors, and populations
- `PopulationGroup` - A subset of residents with shared migration behavior
- `FactorDefinition` - Defines a factor's metadata and normalization
- `FactorValue` - A factor's current value in a city
- `Coordinate` - Geographic position with distance calculation

**Characteristics:**
- Pure domain models
- No dependencies on other layers
- Immutable where possible
- Rich validation in constructors/initializers

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

## Dependency Flow

```
Simulation
    ↓ (depends on)
Logic
    ↓ (depends on)
Core
```

- **Core** has no dependencies (pure domain)
- **Logic** depends on Core (uses domain models)
- **Simulation** depends on Logic and Core (orchestrates everything)

---

## Namespace Convention

All namespaces follow the pattern: `dotGeoMigrata.{Layer}.{SubFolder}`

Examples:
- `dotGeoMigrata.Core.Domain.Entities`
- `dotGeoMigrata.Logic.Attraction`
- `dotGeoMigrata.Simulation.Engine`

---

## Design Principles

1. **Separation of Concerns**: Each layer has a distinct responsibility
2. **Domain-Driven Design**: Core models represent the business domain
3. **Single Responsibility**: Each class has one clear purpose
4. **Dependency Inversion**: Higher layers depend on abstractions
5. **Immutability**: Prefer immutable objects where appropriate
6. **Validation**: Validate at construction time

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
1. Add to `Core/Domain/Entities/` or `Core/Domain/Values/`
2. Follow existing patterns (records for values, classes for entities)
3. Add validation in initializers
4. Document with XML comments

---

## Code Quality Standards

- **XML Documentation**: All public types and members must have `/// <summary>` comments
- **Validation**: Validate inputs in constructors and property initializers
- **Naming**: Use clear, descriptive names following C# conventions
- **Null Safety**: Use C#'s nullable reference types (`enable` in project)
- **Exception Messages**: Provide clear, actionable error messages
- **Readonly**: Use `readonly` for fields that shouldn't change

---

## Getting Started

To use this framework:

1. **Create a World**:
   ```csharp
   var world = new World(cities, factorDefinitions);
   ```

2. **Configure Simulation**:
   ```csharp
   var config = new SimulationConfiguration { MaxSteps = 100 };
   ```

3. **Create Engine**:
   ```csharp
   var engine = new SimulationEngine(world, config);
   ```

4. **Run**:
   ```csharp
   engine.Run();
   ```

See the README files in each subdirectory for detailed documentation.