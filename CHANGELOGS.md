## Version 0.6.2-beta Highlights

**Version 0.6.2-beta** adds comprehensive snapshot persistence for events, configurations, and custom transforms.

### Snapshot System Enhancements

- **Event persistence** - SimulationEvents with triggers and effects are now serialized to snapshots
- **Configuration persistence** - SimulationConfig and StandardModelConfig included in snapshots
- **Custom transform support** - FactorDefinition now persists custom ITransformFunction names
- **Snapshot version bump** - Updated to v0.6 format with backward compatibility

### New Snapshot Models

- `SimulationEventXml` - Persists event display name, description, completion status, trigger, and effect
- `EventTriggerXml` - Supports Tick, Periodic, Continuous trigger types (ConditionalTrigger with limitations)
- `EventEffectXml` - Supports FactorChange, Feedback, and Composite effects
- `SimulationConfigXml` - All simulation parameters
- `StandardModelConfigXml` - All model parameters including parallelism settings
- `FactorDefXml.CustomTransformName` - Stores custom transform function names

### Limitations & Notes

**ConditionalTrigger Persistence:**

- Conditional triggers with custom lambda functions cannot be fully serialized
- Condition expressions stored as strings (for documentation)
- Must be manually reconstructed when loading snapshots with conditional events

**Custom Transform Persistence:**

- Built-in transforms (Linear, Logarithmic, Sigmoid) fully supported
- Custom ITransformFunction implementations store name only
- Falls back to Linear transform if custom transform unavailable during load
- Recommendation: Use built-in transforms or document custom transforms separately

**Event Effect Persistence:**

- FactorChangeEffect fully supported with all application types and durations
- FeedbackEffect stores strategy name (manual reconstruction needed)
- CompositeEffect supported recursively

### API Updates

**Snapshot version:**

```csharp
var snapshot = SnapshotConverter.ToSnapshot(world);
// snapshot.Version == "0.6"
// snapshot.Events contains all active SimulationEvents
// snapshot.SimulationConfig contains simulation parameters
// snapshot.ModelConfig contains model parameters
```

**Loading with custom transforms:**

```csharp
var world = SnapshotConverter.ToWorld(snapshot);
// Built-in custom transforms automatically restored
// Custom implementations must be reapplied manually after load
```

### Architecture Benefits

1. **Full Simulation State** - Snapshots now capture complete simulation state including events and configs
2. **Reproducibility** - Events and configurations ensure deterministic replay
3. **Backward Compatible** - Old v0.5 snapshots still loadable (missing fields default)
4. **Lightweight** - ~200 LOC added, no new dependencies

---

## Version 0.6.1-beta Highlights

**Version 0.6.1-beta** addresses Priority 1-2 improvements from the framework analysis, focusing on configuration
robustness, parallel execution, and extensibility.

### Configuration Improvements

- **SimulationConfig validation in constructor** - All properties now validate during initialization, ensuring
  configurations are always valid
- **Proper validation feedback** - Individual properties throw `ArgumentOutOfRangeException` immediately on invalid
  values
- **Cross-property validation** - `Validate()` method now only checks cross-property constraints (e.g.,
  MinTicksBeforeStabilityCheck < MaxTicks)

### Performance Enhancements

- **Parallel event execution** - EventStage now supports parallel execution for better performance with multiple events
- **Configurable parallelism** - New `WithParallelEvents(bool, int?)` builder method to control parallel behavior
- **Default parallel-on** - Events execute in parallel by default (can be disabled for deterministic execution)

### Extensibility - Custom Transform Functions

- **ITransformFunction interface** - Allows custom normalization functions beyond built-in Linear/Log/Sigmoid
- **TransformFunctions static class** - Built-in implementations: `TransformFunctions.Linear`, `.Logarithmic`,
  `.Sigmoid`
- **FactorDefinition.TransformFunction** - New property for custom transforms (takes precedence over enum-based
  Transform)
- **Backward compatible** - Existing `TransformType` enum still works, custom functions are opt-in

### API Updates

**New SimulationBuilder methods:**

```csharp
builder.WithParallelEvents(useParallel: true, maxDegreeOfParallelism: 4);  // Configure parallel event execution
```

**Custom transform example:**

```csharp
// Built-in transforms via new interface
var factor1 = new FactorDefinition
{
    DisplayName = "Income",
    Type = FactorType.Positive,
    MinValue = 20000,
    MaxValue = 100000,
    TransformFunction = TransformFunctions.Logarithmic  // Using interface
};

// Custom transform function
public class ExponentialTransform : ITransformFunction
{
    public string Name => "Exponential";

    public double Transform(double value, double min, double max)
    {
        var range = max - min;
        if (range == 0) return 0;
        var normalized = (value - min) / range;
        return Math.Pow(normalized, 2);  // Emphasize higher values
    }
}

var factor2 = new FactorDefinition
{
    DisplayName = "Quality",
    Type = FactorType.Positive,
    MinValue = 0,
    MaxValue = 100,
    TransformFunction = new ExponentialTransform()  // Custom implementation
};
```

### Breaking Changes

None - All changes are backward compatible. Existing code continues to work without modifications.

### Architecture Benefits

1. **Type-Safe Configuration** - Invalid configurations fail fast during construction, not at runtime
2. **Better Performance** - Parallel event execution leverages multi-core systems
3. **Extensible Transforms** - Researchers can implement domain-specific normalization functions
4. **Lightweight** - ~150 LOC added, no new dependencies

---

## Version 0.6.0-beta Highlights

**Version 0.6.0-beta** introduces a comprehensive **Event System** as a core simulation mechanism alongside cities,
factors, and persons.

### Breaking Changes

- **FeedbackStage replaced with EventStage** - Feedback strategies now work through the unified event system
- **`WithFeedbackInterval` renamed to `WithEventInterval`** - Reflects broader event system usage
- Events are now first-class simulation components with their own pipeline stage

### New Features - Event System

- **`ISimulationEvent`** - Core abstraction for simulation events that modify world state (in
  `dotMigrata.Simulation.Events.Interfaces`)
- **Event Triggers** (`dotMigrata.Simulation.Events.Triggers`) - Multiple trigger types for flexible event scheduling:
  - `TickTrigger` - Fire once at a specific tick
  - `PeriodicTrigger` - Fire repeatedly at regular intervals
  - `ContinuousTrigger` - Fire continuously within a time window
  - `ConditionalTrigger` - Fire when custom conditions are met (extension point for ECA patterns)
- **Event Effects** (`dotMigrata.Simulation.Events.Effects`) - Flexible effect system for factor modifications:
  - `FactorChangeEffect` - Change city factor values with multiple application types
  - `FeedbackEffect` - Adapter for existing feedback strategies (backward compatible)
  - `CompositeEffect` - Apply multiple effects simultaneously
- **Effect Application Types** (`dotMigrata.Simulation.Events.Enums`):
  - `EffectApplicationType.Absolute` - Set factor to exact value
  - `EffectApplicationType.Delta` - Add/subtract from current value
  - `EffectApplicationType.Multiply` - Multiply current value
  - `EffectApplicationType.LinearTransition` - Transition linearly over duration
  - `EffectApplicationType.LogarithmicTransition` - Transition logarithmically (fast then slow)
- **ValueSpecification Integration** - Reuses existing infrastructure for flexible value generation (fixed, range,
  approximate)
- **EventStage** (`dotMigrata.Simulation.Events.EventStage`) - Pipeline stage that executes events based on their
  triggers
- **Type-Safe Design** - Events use object references (FactorDefinition, City) instead of string IDs

### Builder API Updates

**New methods:**

```csharp
builder.WithEvent(ISimulationEvent)
builder.WithFactorChange(displayName, factor, valueSpec, trigger, applicationType, duration, cityFilter)
builder.WithEventInterval(interval)  // Renamed from WithFeedbackInterval
```

**Backward compatible:**

```csharp
builder.WithFeedback(strategy)  // Now creates periodic events internally
```

### Usage Examples

```csharp
using dotMigrata.Simulation.Events.Effects;
using dotMigrata.Simulation.Events.Enums;
using dotMigrata.Simulation.Events.Triggers;

// One-time event at tick 50
builder.WithFactorChange(
    "Earthquake",
    infrastructureFactor,
    ValueSpecification.Fixed(30),
    new TickTrigger(50),
    EffectApplicationType.Absolute);

// Periodic event every 10 ticks
builder.WithFactorChange(
    "Housing Subsidy",
    housingCostFactor,
    ValueSpecification.Fixed(-5),
    new PeriodicTrigger(10, startTick: 20, endTick: 80),
    EffectApplicationType.Delta);

// Linear transition over 20 ticks
builder.WithFactorChange(
    "Green Initiative",
    pollutionFactor,
    ValueSpecification.Fixed(20),
    new TickTrigger(30),
    EffectApplicationType.LinearTransition,
    EffectDuration.Over(20));

// Conditional event (extension point for complex ECA patterns)
using dotMigrata.Simulation.Events;

builder.WithEvent(new SimulationEvent(
    "Congestion Response",
    new ConditionalTrigger(
        ctx => ctx.World.Cities.Any(c => c.Population > c.Capacity * 0.9),
        cooldown: 10),
    new FactorChangeEffect(...)));
```

### Architecture Benefits

1. **Unified Mechanism** - Events and feedback use the same infrastructure
2. **Extensible** - Easy to add custom triggers, effects, and event types
3. **Type-Safe** - No string-based IDs, uses object references
4. **Lightweight** - Reuses existing ValueSpecification, no heavy dependencies
5. **Visualization-Ready** - Events have display names and descriptions for UI integration
6. **Flexible Timing** - Support for one-time, periodic, continuous, and conditional execution

### Migration Guide

**v0.5.2 code:**

```csharp
builder.WithFeedback(strategy)
    .WithFeedbackInterval(10);
```

**v0.6.0 equivalent:**

```csharp
builder.WithFeedback(strategy)  // Still works! Converted to events internally
    .WithEventInterval(10);      // Renamed method
```

**New event-based approach:**

```csharp
using dotMigrata.Simulation.Events;
using dotMigrata.Simulation.Events.Effects;
using dotMigrata.Simulation.Events.Enums;
using dotMigrata.Simulation.Events.Triggers;

builder.WithEvent(new SimulationEvent(
    $"Feedback: {strategy.Name}",
    new PeriodicTrigger(10),
    new FeedbackEffect(strategy)));
```

---

## Version 0.5.2-beta Highlights

**Version 0.5.2-beta** introduces major architectural improvements for type safety, extensibility, and design clarity:

### Breaking Changes

- **`AttributeValueBuilder` removed** - Use `ValueSpecification` static methods directly instead of
  `Attribute("name").InRange()` pattern
- **`GeneratorConfig` removed** - Replaced with `StandardPersonGenerator` implementing
  `IPersonGenerator<StandardPerson>`
- **FeedbackStrategy constructors changed** - Now use `FactorDefinition` parameter instead of `string factorName`
- **World enforces single person type** - Throws `WorldValidationException` if mixing different person types

### New Features

- **`IPersonGenerator<TPerson>` interface** - Type-safe generator interface for creating custom person generators
- **`StandardPersonGenerator`** - Clean, type-safe replacement for `GeneratorConfig`
- **Person type validation** - World constructor validates all persons are of the same concrete type
- **Type-safe FeedbackStrategy** - Use `FactorDefinition` references instead of string-based lookups

### API Changes

**Before (0.5.1-beta):**

```csharp
using static dotMigrata.Generator.AttributeValueBuilder;

collection.Add(new GeneratorConfig
{
    Count = 1000,
    FactorSensitivities = new Dictionary<FactorDefinition, ValueSpecification>
    {
        [factor] = Attribute("Sensitivity").InRange(3, 8)
    },
    MovingWillingness = MovingWillingness().InRange(0.4, 0.7)
});

var feedback = new CongestionFeedbackStrategy("Quality of Life");
```

**After (0.5.2-beta):**

```csharp
collection.Add(new StandardPersonGenerator
{
    Count = 1000,
    FactorSensitivities = new Dictionary<FactorDefinition, ValueSpecification>
    {
        [factor] = ValueSpecification.InRange(3, 8)
    },
    MovingWillingness = ValueSpecification.InRange(0.4, 0.7)
});

var feedback = new CongestionFeedbackStrategy(qualityOfLifeFactor);
```

### Custom Person Type Generation

**Before (PersonFactory pattern):**

```csharp
collection.Add(new GeneratorConfig
{
    Count = 1000,
    PersonFactory = (sens, will, ret, scale, thresh, minAtt, tags) =>
        new CustomPerson(sens) { /* 7 parameters */ }
});
```

**After (IPersonGenerator pattern):**

```csharp
public class CustomPersonGenerator : IPersonGenerator<CustomPerson>
{
    public required int Count { get; init; }
    public required ValueSpecification CustomProperty { get; init; }
    // Only properties you need
    
    public IEnumerable<CustomPerson> Generate(IEnumerable<FactorDefinition> factors)
    {
        // Clean generation logic
    }
}

collection.Add(new CustomPersonGenerator { Count = 1000, /* ... */ });
```

### Benefits

- **Type Safety**: Removed all string-based lookups (AttributeValueBuilder, FeedbackStrategy factor names)
- **Extensibility**: Clean `IPersonGenerator<TPerson>` interface for custom types
- **Design Clarity**: Single person type per world enforced at runtime
- **Better API**: Simpler, more discoverable interfaces

### Migration Guide

1. **Remove `using static dotMigrata.Generator.AttributeValueBuilder`**
2. **Replace `Attribute("name").Method()` with `ValueSpecification.Method()`**
3. **Replace `new GeneratorConfig` with `new StandardPersonGenerator`**
4. **Update FeedbackStrategy** to pass `FactorDefinition` instead of string
5. **Custom generators**: Implement `IPersonGenerator<TPerson>` instead of using `PersonFactory`

See [USAGE.md](USAGE.md) for complete updated examples.

## Version 0.5.1-beta Highlights

**Version 0.5.1-beta** adds support for custom person type generation and improved snapshot compatibility:

### New Features

- **PersonFactory Support** - Generate custom person types using the `PersonFactory` property in `GeneratorConfig`
- **Custom Person Generation** - Define how custom properties should be generated for your domain-specific person types
- **Snapshot Compatibility** - Snapshot system now works with `PersonBase` instead of being hardcoded to
  `StandardPerson`
- **Bug Fixes** - Fixed template duplication bug that would return the same instance for custom person types

### Example: Custom Person Generation

```csharp
collection.Add(new GeneratorConfig
{
    Count = 10000,
    FactorSensitivities = new Dictionary<FactorDefinition, ValueSpecification>
    {
        [incomeFactor] = Attribute("IncomeSensitivity").InRange(3, 8)
    },
    MovingWillingness = MovingWillingness().InRange(0.4, 0.7),
    RetentionRate = RetentionRate().InRange(0.3, 0.6),

    // PersonFactory for custom person types
    PersonFactory = (sensitivities, willingness, retention, scaling, threshold, minAttraction, tags) =>
    {
        return new DemographicPerson(sensitivities)
        {
            MovingWillingness = willingness,
            RetentionRate = retention,
            Age = random.Next(18, 65),
            Income = random.Next(25000, 120000),
            EducationLevel = "Bachelor"
        };
    }
});
```

See [USAGE.md](USAGE.md) for complete examples.

## Version 0.5.0-beta Highlights

**Version 0.5.0-beta** introduces a major architectural refactoring with inheritance-based person types:

### Breaking Changes

- **Person class removed entirely** - The monolithic `Person` class has been replaced with an inheritance-based
  architecture
- **PersonBase abstract class** - New base class with essential migration properties (`MovingWillingness`,
  `RetentionRate`, `FactorSensitivities`)
- **StandardPerson implementation** - Concrete class that replaces the old `Person` class with additional properties (
  `SensitivityScaling`, `AttractionThreshold`, `MinimumAcceptableAttraction`, `Tags`)
- **Type-based extensibility** - Custom person types can now inherit from `PersonBase` for domain-specific properties
- **Updated interfaces** - All interfaces (`IAttractionCalculator`, `IMigrationCalculator`) now use `PersonBase` instead
  of `Person`

### Enhanced Features

- **Inheritance-based architecture**: Define custom person types by inheriting from `PersonBase`
- **Pattern matching support**: `StandardAttractionCalculator` uses pattern matching for `StandardPerson` properties
- **Universal tags support**: All person types inherit `Tags` property from `PersonBase` for consistent categorization
- **Backward compatibility**: `StandardPerson` provides all functionality of the removed `Person` class
- **Enhanced Exception System**: Comprehensive exception hierarchy with `DotMigrataException`, `ConfigurationException`,
  `GeneratorSpecificationException`, `WorldValidationException`, and `SnapshotException`
- **Improved Value Specifications**: Named attribute methods like `MovingWillingness()`, `RetentionRate()` for clearer,
  more expressive code
- **Normal Distribution Support**: `Approximately(mean, stdDev)` method for realistic value generation
- **Configurable Parallelism**: Control parallel processing with `UseParallelProcessing` and `MaxDegreeOfParallelism`
- **Modern C# Features**: Uses C# latest features including records, init-only properties, and collection expressions