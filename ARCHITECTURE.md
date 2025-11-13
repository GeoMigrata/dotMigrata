# dotGeoMigrata Architecture

## Core Design Principles

### 1. Reference-Based Architecture with Guid Identity

**Design Decision**: Use both object references AND Guid IDs strategically.

**Object References** (for internal logic):

- `Person.CurrentCity: City?` - Direct reference for O(1) access
- `FactorDefinition` - Used as dictionary keys throughout
- `FactorValue.Definition: FactorDefinition` - Direct reference

**Guid IDs** (for serialization and identity):

- `Person.Id: Guid` - Stable identity for snapshots, logging
- `City` persons stored as `ConcurrentDictionary<Guid, Person>` - Thread-safe lookup

**Performance Analysis**:

- Object reference dereference: ~5 nanoseconds
- Dictionary lookup by Guid: ~50-100 nanoseconds (O(1))
- **Conclusion**: Both approaches are highly efficient, negligible performance difference

**Benefits of Hybrid Approach**:

- ✅ Direct references where possible for maximum performance
- ✅ Guid IDs for serialization, debugging, distributed scenarios
- ✅ Type safety with FactorDefinition references (no string typos)
- ✅ Thread-safe concurrent operations via ConcurrentDictionary

### 2. Circular Reference Resolution

**Problem**: Person needs City reference, City needs Person collection.

**Solution**: Nullable CurrentCity with lazy initialization.

```csharp
// Step 1: Create person (CurrentCity = null)
var person = new Person(sensitivities)
{
    MovingWillingness = 0.7,
    RetentionRate = 0.3
};

// Step 2: Add to city (establishes bidirectional reference)
city.AddPerson(person);  // Sets person.CurrentCity = this

// Result: No initialization order problem
```

**Key Design**:

- `Person.CurrentCity` is nullable (`City?`)
- Person constructor doesn't require CurrentCity
- `City.AddPerson()` establishes both relationships atomically
- `City.RemovePerson()` safely breaks relationships

**GC Safety**: C#'s mark-and-sweep garbage collector handles circular references correctly.

### 3. Immutability and Controlled Mutation

**Immutable by Default**:

- `FactorDefinition` - record type, init-only properties
- `ValueRange` - readonly struct
- `Person.FactorSensitivities` - readonly dictionary (internal mutable)

**Controlled Mutation**:

- `Person.CurrentCity` - set by City.AddPerson/RemovePerson only
- `Person.MovingWillingness/RetentionRate` - validated setters
- `City.Persons` - modified only via AddPerson/RemovePerson
- `FactorValue.Intensity` - validated setter with NaN/Infinity checks

**Benefits**:

- Thread-safe reads
- Clear mutation points
- Easier to reason about state changes
- Validation at mutation boundaries

### 4. Value Specification Pattern

**Semantic Fluent API**:

```csharp
MovingWillingness = Value().InRange(0.6, 0.9)        // Custom range
RetentionRate = Value().Of(0.3)                       // Fixed value
SensitivityScaling = Value().Approximately(1.0, 1.2)  // ~1.0 with 1.2x scale
```

**Design**:

- `AttributeValueBuilder` - provides fluent entry point
- `ValueSpecification` - stores generation rules
- `GeneratorConfig` - uses specifications to generate persons

**Benefits**:

- Self-documenting code
- Type-safe specification
- Flexible (fixed, ranged, scaled values)
- No hidden defaults (required attributes must be specified)

### 5. Type Safety with FactorDefinition References

**Old Approach** (string-based, error-prone):

```csharp
FactorSensitivities = new Dictionary<string, ValueSpecification>
{
    ["Incom"] = ... // Typo! Runtime error
}
```

**New Approach** (reference-based, compile-time safe):

```csharp
FactorSensitivities = new Dictionary<FactorDefinition, ValueSpecification>
{
    [incomeFactor] = ... // Compile-time checked, refactor-safe
}
```

**Benefits**:

- Compile-time errors for typos
- IDE refactoring support
- No string lookup overhead
- Clear dependencies

### 6. Validation and Robustness

**ArgumentNullException.ThrowIfNull()** throughout:

- All constructors validate required parameters
- All public methods validate inputs
- Consistent error messages

**Range Validation** with pattern matching:

```csharp
public double MovingWillingness
{
    get => _movingWillingness;
    set => _movingWillingness = value is >= 0 and <= 1
        ? value
        : throw new ArgumentException("Must be between 0 and 1.");
}
```

**DataAnnotations** for declarative validation:

```csharp
[Required]
[Range(1, int.MaxValue)]
public required int Count { get; init; }
```

**Benefits**:

- Fail fast with clear error messages
- Prevent invalid states
- Self-documenting constraints
- Consistent validation patterns

### 7. One Type Per File

**Structure**:

- Each `.cs` file contains exactly one public type
- File name matches type name
- Clear namespace organization

**Benefits**:

- Easy navigation (Go to File in IDE)
- Smaller, focused files
- Better Git diffs
- Follows C# community conventions

## Standard Operating Procedure (SOP)

**Correct workflow for using the framework**:

```csharp
// Step 1: Define factors
var incomeFactor = new FactorDefinition { ... };
var housingFactor = new FactorDefinition { ... };

// Step 2: Create population with factor references
var population = new PersonCollection();
population.Add(new GeneratorConfig(seed: 42)
{
    Count = 50_000,
    FactorSensitivities = new Dictionary<FactorDefinition, ValueSpecification>
    {
        [incomeFactor] = Value().InRange(5, 10),
        [housingFactor] = Value().InRange(-8, -4)
    },
    MovingWillingness = Value().InRange(0.6, 0.9),
    RetentionRate = Value().InRange(0.1, 0.4),
    Tags = ["young", "tech"]
});

// Step 3: Build world with cities
var world = new WorldBuilder()
    .AddFactor(incomeFactor)
    .AddFactor(housingFactor)
    .AddCity("Tech City", ..., city => city
        .WithFactorValue(incomeFactor, 120000)
        .WithPersonCollection(population))
    .Build();

// Step 4: Run simulation
var result = await new SimulationBuilder()
    .WithWorld(world)
    .UseStandardPipeline()
    .BuildAndRunAsync();
```

**Key Points**:

- Factors defined first (needed for PersonCollection)
- Populations created using factor references
- Cities built with populations
- World assembles everything
- Simulation executes

## Performance Characteristics

**Small Simulations** (10k-50k persons):

- Memory: ~3-15 MB
- Generation time: <1 second
- Suitable for: Testing, prototyping

**Medium Simulations** (50k-200k persons):

- Memory: ~15-60 MB
- Generation time: ~1-5 seconds
- Suitable for: Real scenarios, analysis

**Large Simulations** (200k-1M persons):

- Memory: ~60-300 MB
- Generation time: ~5-30 seconds
- Suitable for: Large-scale research

**Optimizations**:

- Parallel processing with PLINQ
- ConcurrentDictionary for thread-safe operations
- ValueRange struct (stack allocation)
- Collection expressions (reduced allocations)
- Direct object references (minimal overhead)

## Thread Safety

**Thread-Safe Components**:

- `ConcurrentDictionary<Guid, Person>` in City
- Parallel migration calculations (PLINQ)
- Immutable factor definitions and value ranges

**Not Thread-Safe** (by design):

- Person property setters (simulation is single-threaded phases)
- City.AddPerson/RemovePerson (called from synchronized context)
- World construction (builder pattern)

**Concurrency Model**: Single-threaded simulation phases with parallel calculations within each phase.

## Future Considerations

**Potential Enhancements**:

- Person clustering for mega-scale simulations (5M+ persons)
- Distributed simulation across multiple processes
- Advanced demographics (age, family structures)
- Machine learning integration for migration prediction
- Real-time visualization and monitoring

**Extensibility Points**:

- ISimulationStage - custom pipeline stages
- ISimulationObserver - custom observers
- IAttractionCalculator - custom attraction models
- IMigrationCalculator - custom migration models
- IPersonSpecification - custom person generation strategies

## Summary

dotGeoMigrata uses a **hybrid reference + identity architecture** that combines the performance of direct object
references with the stability of Guid identities. The framework prioritizes **type safety**, **explicit configuration**,
and **performance** while maintaining **clean code** and **clear semantics**.

Key strengths:

- ✅ Type-safe FactorDefinition references
- ✅ Efficient hybrid reference/ID approach
- ✅ Clean circular reference resolution
- ✅ Comprehensive validation and robustness
- ✅ Semantic fluent API
- ✅ No hidden defaults (explicit configuration)
- ✅ Scalable to 1M+ persons
- ✅ Modern C# patterns and idioms