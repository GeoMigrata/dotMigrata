# dotGeoMigrata

**README Version:** Oct 25, 2025

dotGeoMigrata is a C# .NET 9.0 simulation framework designed to model population migration and city evolution in a
multi-city, multi-population system. The framework captures how city characteristics influence population movement and
how migration feedback affects city factors over time.

## Core Idea

The main simulation loop follows:

```text
City Factors → Population Preferences → Attraction Differences → Partial Migration → City Feedback → Iterative Evolution
```

Population migration is driven by city factors, and in turn, migration reshapes city factors in an ongoing iterative
process.

## Key Concepts

- **World:** Top-level entity containing cities, factor definitions, and optionally events. Maintains global factor
  definitions and current city states.
- **City:** Contains factor values (e.g., income, pollution, public services) and multiple population groups (e.g.,
  youth, retirees, working-age adults). May have a capacity constraint.
- **FactorDefinition & FactorValue:** Define metadata for each factor, including direction (pull or push),
  normalization, and value range. Factor values are normalized for internal calculations.
- **PopulationGroup:** Represents a subset of residents with similar migration behavior. Key attributes include
  population count, move willingness, retention rate, migration threshold, and minimum acceptable attraction. Each group
  has sensitivities to various factors.
- **Attraction:** Computes net attractiveness of a city for a population group considering normalized factor values,
  group sensitivities, and factor directions.
- **Migration:** Migration decisions are based on attraction differences, thresholds, and migration cost. Partial
  migration is modeled via probabilistic sampling considering group size and city capacity.
- **City Feedback:** After migration, city factors are updated according to feedback mechanisms (per capita resources,
  housing costs, congestion/pollution, industrial/economic effects), typically with smoothing to avoid abrupt changes.

## Simulation Flow

1. Initialize the world with cities, factor definitions, population groups, and sensitivities.
2. For each simulation step:
    - Normalize city factors.
    - Calculate city attraction for each population group.
    - Compute migration probability for each group toward possible destination cities.
    - Sample actual migration numbers considering capacity and retention.
    - Update city population composition.
    - Update city factors based on migration feedback.
3. Repeat until simulation ends (max time steps or system stabilizes).

## Installation & Usage

Clone the repository and open in Visual Studio 2022+ or use the .NET 9.0 SDK. Build the project with `dotnet build`.

This is a library framework. To use it, reference the `dotGeoMigrata` library in your project and create a simulation by:
1. Defining your factor definitions
2. Creating cities with factor values
3. Adding population groups with sensitivities
4. Configuring the simulation engine
5. Running the simulation

For detailed algorithm documentation, see:
- `/src/Logic/README.md` - Logic layer algorithms
- `/src/Simulation/README.md` - Simulation engine design

## Contributing

Contributions, bug reports, and feature requests are welcome. Please submit via GitHub issues or pull requests.