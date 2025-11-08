# dotGeoMigrata

dotGeoMigrata is a C# .NET 9.0 simulation framework designed to model population migration and city evolution in a
multi-city, multi-population system. The framework captures how city characteristics influence population movement and
how migration feedback affects city factors over time.

## Core Idea

The main simulation loop follows:

```text
City Factors -> Population Preferences -> Attraction Differences -> Partial Migration -> City Feedback -> Iterative Evolution
```

Population migration is driven by city factors, and in turn, migration reshapes city factors in an ongoing iterative
process.

## Key Concepts

- **World:** Top-level entity containing cities, factor definitions, and population group definitions. Maintains global
  factor definitions, population group definitions, and current city states.
- **City:** Contains factor values (e.g., income, pollution, public services) and population group values for each
  defined population group. Each city has values for all factor definitions and all population group definitions defined
  in the world.
- **FactorDefinition & FactorValue:** Define metadata for each factor, including direction (pull or push),
  normalization, and value range. Factor values are normalized for internal calculations. Each city has a FactorValue
  for every FactorDefinition in the world.
- **PopulationGroupDefinition & PopulationGroupValue:** Define population groups with shared migration behavior.
  PopulationGroupDefinition specifies move willingness, retention rate, and sensitivities to various factors.
  PopulationGroupValue represents the actual population count of that group in a specific city. Each city has a
  PopulationGroupValue for every PopulationGroupDefinition in the world (count can be 0). This design allows reusing
  the same population group characteristics across multiple cities without duplication.
- **Attraction:** Computes net attractiveness of a city for a population group considering normalized factor values,
  group sensitivities, and factor directions.
- **Migration:** Migration decisions are based on attraction differences, thresholds, and migration cost. Migration
  is modeled via probabilistic sampling considering group size and city capacity. Instead of adding/removing population
  groups, migrations update the population counts in PopulationGroupValues.
- **City Feedback:** After migration, city factors are updated according to feedback mechanisms (per capita resources,
  housing costs, congestion/pollution, industrial/economic effects), typically with smoothing to avoid abrupt changes.

## Simulation Flow

1. Initialize the world with cities, factor definitions, population group definitions, and sensitivities.
    - Each PopulationGroupDefinition must have sensitivities for all FactorDefinitions
    - Each City must have FactorValues for all FactorDefinitions
    - Each City must have PopulationGroupValues for all PopulationGroupDefinitions
2. For each simulation step:
    - Normalize city factors.
    - For each population group definition, calculate city attraction across all cities.
    - Compute migration probability for each group toward possible destination cities.
    - Sample actual migration numbers considering capacity and retention.
    - Update city population composition by modifying PopulationGroupValue counts.
    - Update city factors based on migration feedback.
3. Repeat until simulation ends (max time steps or system stabilizes).

## Installation & Usage

Clone the repository and open in Visual Studio 2022+ or use the .NET 9.0 SDK. Build the project with `dotnet build`.

This is a library framework. To use it, reference the `dotGeoMigrata` library in your project and create a simulation
by:

1. Defining your factor definitions
2. Defining your population group definitions with sensitivities for all factors
3. Creating cities with factor values for all factor definitions
4. Creating population group values for each city for all population group definitions
5. Creating calculators (or using the built-in StandardModel)
6. Setting up simulation stages and configuration
7. Creating a simulation engine and running it

## Architecture

### Core Layer (`/src/Core`)

Contains fundamental domain models:

- `World`, `City` - Entity models
- `FactorDefinition`, `GroupDefinition` - Definition models
- `FactorValue`, `GroupValue` - Value models

### Logic Layer (`/src/Logic`)

Provides calculation interfaces and implementations:

- `IAttractionCalculator` - Calculates city attraction for groups
- `IMigrationCalculator` - Determines migration flows
- `StandardAttractionCalculator` / `StandardMigrationCalculator` - Default implementation based on scientific models

### Simulation Layer (`/src/Simulation`)

Implements a pipeline-based simulation engine:

- `ISimulationStage` - Extensible stage interface
- `SimulationEngine` - Tick-based orchestrator
- Built-in stages: `AttractionCalculationStage`, `MigrationDecisionStage`, `MigrationExecutionStage`
- `ISimulationObserver` - Observer pattern for monitoring (includes `ConsoleObserver`)

### Snapshot Layer (`/src/Snapshot`)

Git-like incremental snapshot system:

- Stores initial world state + simulation steps (deltas)
- Supports JSON and XML serialization
- Efficient storage with migration event records

## Contributing

Contributions, bug reports, and feature requests are welcome. Please submit via GitHub issues or pull requests.