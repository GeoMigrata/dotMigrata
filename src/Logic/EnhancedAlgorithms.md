# Enhanced Algorithms Documentation

This document details the enhanced calculator algorithm implementations based on the LogicModel.md design specification.

## Enhanced Attraction Calculator

### Algorithm Overview

Implements the factor-intensity-sensitivity model using pull-push theory.

### Core Formula

```
Attraction(C,G) = A_G × (Pull - Push)
Where:
  Pull = Σ(w_i × s_i)  for all pull factors i
  Push = Σ(w_i × (1 - s_i))  for all push factors i
  
  s_i = normalize(v_i)  normalized factor value ∈ [0,1]
  w_i = sensitivity weight
  A_G = group sensitivity scaling coefficient (default 1.0)
```

### Key Features

1. **Pull-Push Separation**
    - Pull factors (Positive): Higher values increase attraction
    - Push factors (Negative): Higher values decrease attraction
    - Use `(1 - s_i)` for push to ensure high push values create repulsion

2. **Sensitivity Scaling**
    - Adjusted via `PopulationGroupDefinition.SensitivityScaling` (A_G)
    - Allows different groups to have varying sensitivity to factor changes

3. **Factor Normalization**
    - Supports Linear, Logarithmic, and Sigmoid normalization
    - All factor values mapped to [0,1] range

### Usage Example

```csharp
var calculator = new EnhancedAttractionCalculator();

// Calculate attraction for a single city
var result = calculator.CalculateAttraction(city, groupDefinition, world);

Console.WriteLine($"Attraction score: {result.AttractionScore}");
Console.WriteLine($"Pull component: {result.PullComponent}");
Console.WriteLine($"Push component: {result.PushComponent}");
```

### Extended Return Value

`AttractionResult` now includes:

- `AttractionScore`: Comprehensive attraction score
- `PullComponent`: Sum of pull components
- `PushComponent`: Sum of push components

---

## Enhanced Migration Calculator

### Algorithm Overview

Implements complete migration decision workflow:

1. Emigration decision (based on attraction thresholds)
2. Destination city selection
3. Sigmoid probability calculation
4. Cost decay
5. Capacity constraint handling

### Core Formulas

#### 1. Migration Probability

```
rawRate = 1 / (1 + exp(-k × ΔA))  # Sigmoid function
p_C = rawRate × exp(-λ × d)       # Cost decay
Where:
  ΔA = A(C,G) - A(O,G)  attraction difference
  k = sigmoid steepness coefficient (default 1.0)
  d = migration cost (distance × baseCost)
  λ = cost sensitivity coefficient (default 0.01)
```

#### 2. Effective Migration Probability

```
effectiveProbability = (1 - r) × p_C
Where:
  r = retention rate (RetentionRate)
```

#### 3. Emigration Conditions

Migration is considered only when both conditions are met:

```
ΔA > τ                      # Attraction difference exceeds threshold
A(C,G) > α_min              # Destination attraction above minimum
```

#### 4. Capacity Constraints

```
If totalInflow > remainingCapacity:
    scalingFactor = remainingCapacity / totalInflow
    adjustedMigrants = floor(migrants × scalingFactor)
```

### Parameter Reference

| Parameter | Property                    | Default | Description                                       |
|-----------|-----------------------------|---------|---------------------------------------------------|
| k         | SigmoidSteepness            | 1.0     | Sigmoid steepness, higher = faster transition     |
| λ         | CostSensitivity             | 0.01    | Cost sensitivity, higher = faster decay           |
| baseCost  | BaseMigrationCost           | 1.0     | Base cost per unit distance                       |
| τ         | AttractionThreshold         | 0.0     | Attraction threshold (group definition)           |
| α_min     | MinimumAcceptableAttraction | 0.0     | Minimum acceptable attraction (group definition)  |
| m         | MovingWillingness           | -       | Moving willingness upper limit (group definition) |
| r         | RetentionRate               | -       | Retention rate (group definition)                 |

### Usage Example

```csharp
var calculator = new EnhancedMigrationCalculator
{
    SigmoidSteepness = 1.5,      // Steeper probability curve
    CostSensitivity = 0.02,      // Higher cost sensitivity
    BaseMigrationCost = 2.0      // Higher base cost
};

var flows = calculator.CalculateMigrationFlows(
    sourceCity,
    groupDefinition,
    attractions,
    world,
    random);

foreach (var flow in flows)
{
    Console.WriteLine($"{flow.SourceCity.DisplayName} -> " +
                     $"{flow.DestinationCity.DisplayName}: " +
                     $"{flow.MigrantCount} people " +
                     $"(probability: {flow.MigrationProbability:P2})");
}
```

### Probabilistic Sampling

#### Small Population (≤100)

Binomial sampling, each person decides independently:

```csharp
migrants = population.Count(person => random.NextDouble() < probability);
```

#### Large Population (>100)

Normal approximation:

```csharp
mean = population × probability
variance = population × probability × (1 - probability)
stdDev = sqrt(variance)
sample = mean + stdDev × N(0,1)  # Box-Muller transform
migrants = clamp(round(sample), 0, population)
```

---

## Enhanced Feedback Calculator

### Algorithm Overview

Implements multiple factor feedback mechanisms, dynamically updating city factors based on population changes.

### Feedback Types

#### 1. PerCapitaResource

**Applicable Factors**: Doctors per capita, public services, education resources

**Formula**:

```
newValue = oldValue / populationRatio
```

**Description**: Assumes total resources unchanged, per-capita value inversely proportional to population.

#### 2. PriceCost

**Applicable Factors**: Housing price, rent, cost of living
**Formula**:

```
ΔPrice = ε × (ΔP / P) × currentValue
newValue = currentValue + ΔPrice

Where:
  ε = elasticity coefficient (default 0.3)
  ΔP = population change
  P = original population
```

**Description**: Price change proportional to population change ratio.

#### 3. NegativeExternality

**Applicable Factors**: Pollution, congestion, crime rate
**Formula**:

```
newValue = oldValue + β × ΔP

Where:
  β = externality coefficient (default 0.0001)
  ΔP = population change
```

**Description**: Negative externalities increase linearly with population.

#### 4. PositiveExternality

**Applicable Factors**: Economic output, innovation capacity, employment opportunities
**Formula**:

```
saturationRatio = currentPopulation / saturationPoint
growthFactor = 1 - tanh(saturationRatio)
relativeGrowth = ΔP / currentPopulation
actualGrowth = relativeGrowth × growthFactor
newValue = oldValue × (1 + actualGrowth)

Where:
  saturationPoint = saturation population (default 1,000,000)
```

**Description**: Positive externalities grow but with diminishing returns as population approaches saturation.

#### 5. None

Factor value unchanged with population.

### Smoothing

All updates undergo exponential smoothing:

```
finalValue = currentValue + α × (targetValue - currentValue)

Where:
  α = SmoothingFactor ∈ [0,1] (default 0.2)
```

### Usage Example

```csharp
// Define feedback rules
var feedbackRules = new List<FactorFeedbackRule>
{
    new() 
    { 
        Factor = housingPriceFactor,
        FeedbackType = FeedbackType.PriceCost,
        Elasticity = 0.3  // 30% elasticity
    },
    new() 
    { 
        Factor = pollutionFactor,
        FeedbackType = FeedbackType.NegativeExternality,
        ExternalityCoefficient = 0.0001  // +0.0001 pollution per person
    },
    new() 
    { 
        Factor = economicOutputFactor,
        FeedbackType = FeedbackType.PositiveExternality,
        SaturationPoint = 500_000  // 500k saturation
    },
    new() 
    { 
        Factor = doctorsPerCapitaFactor,
        FeedbackType = FeedbackType.PerCapitaResource
    }
};
// Create calculator
var calculator = new EnhancedFeedbackCalculator(feedbackRules)
{
    SmoothingFactor = 0.2  // 20% smoothing
};
// Apply feedback
calculator.ApplyFeedback(city, previousPopulation, currentPopulation);
```

### Dynamic Rule Addition

```csharp
var calculator = new EnhancedFeedbackCalculator();

// Add rules at runtime
calculator.AddFeedbackRule(new FactorFeedbackRule
{
    Factor = customFactor,
    FeedbackType = FeedbackType.PriceCost,
    Elasticity = 0.5
});
```

---

## Population Group Definition New Attributes

`PopulationGroupDefinition` new properties:

```csharp
public sealed record PopulationGroupDefinition
{
    // Existing properties
    public string DisplayName { get; init; }
    public double MovingWillingness { get; init; }  // m: moving willingness limit
    public double RetentionRate { get; init; }      // r: retention rate
    
    // New properties
    public double SensitivityScaling { get; init; } = 1.0;        // A_G: sensitivity scaling
    public double AttractionThreshold { get; init; } = 0.0;       // τ: attraction threshold
    public double MinimumAcceptableAttraction { get; init; } = 0.0; // α_min: minimum attraction
    
    public IReadOnlyList<FactorSensitivity> Sensitivities { get; }
}
```

### Example Configuration

```csharp
var youngProfessionals = new PopulationGroupDefinition(sensitivities)
{
    DisplayName = "Young Professionals",
    MovingWillingness = 0.3,           // Max 30% willing to migrate
    RetentionRate = 0.05,              // At least 5% retained
    SensitivityScaling = 1.2,          // More sensitive to factor changes
    AttractionThreshold = 0.1,         // Attraction diff must exceed 0.1
    MinimumAcceptableAttraction = 0.2  // Destination attraction must exceed 0.2
};
```

---

## City Capacity

`City` class new capacity property:

```csharp
public class City
{
    // ...other properties
    
    public int? Capacity { get; init; }  // null or 0 = unlimited capacity
}
```

### Example

```csharp
var smallCity = new City(factorValues, populationGroupValues)
{
    DisplayName = "Small City",
    Location = coordinate,
    Area = 100,
    Capacity = 50_000  // Maximum capacity 50k people
};
```

---

## Complete Usage Example

```csharp
// 1. Create factor definitions
var incomeFactor = new FactorDefinition { ... };
var pollutionFactor = new FactorDefinition { ... };

// 2. Create population group definition (with new attributes)
var groupDef = new PopulationGroupDefinition(sensitivities)
{
    DisplayName = "Middle Class",
    MovingWillingness = 0.25,
    RetentionRate = 0.1,
    SensitivityScaling = 1.0,
    AttractionThreshold = 0.05,
    MinimumAcceptableAttraction = 0.1
};

// 3. Create city (with capacity)
var city = new City(factorValues, populationValues)
{
    DisplayName = "Test City",
    Location = new Coordinate { Latitude = 39.9, Longitude = 116.4 },
    Area = 16410,
    Capacity = 2_000_000  // 2 million capacity
};

// 4. Configure feedback rules
var feedbackRules = new List<FactorFeedbackRule>
{
    new() { Factor = pollutionFactor, FeedbackType = FeedbackType.NegativeExternality }
};

// 5. Use enhanced calculators
var attractionCalc = new EnhancedAttractionCalculator();
var migrationCalc = new EnhancedMigrationCalculator
{
    SigmoidSteepness = 1.5,
    CostSensitivity = 0.02
};
var feedbackCalc = new EnhancedFeedbackCalculator(feedbackRules);

// 6. Run simulation
var engine = new PipelineSimulationEngine(
    world, config,
    attractionCalc, migrationCalc, feedbackCalc);
engine.Run();
```

---

## Performance Optimization Recommendations

1. **Large Populations**: Use normal approximation instead of individual sampling
2. **Capacity Constraints**: Calculate only when necessary (Capacity != null)
3. **Feedback Rules**: Define rules only for factors requiring feedback
4. **Smoothing Factor**: Larger smoothing factor reduces calculation frequency

## Comparison with Original Algorithms

| Feature                | Original            | Enhanced                      |
|------------------------|---------------------|-------------------------------|
| Attraction Calculation | Simple weighted sum | Pull-push separation          |
| Migration Probability  | Tanh function       | Sigmoid + cost decay          |
| Capacity Constraints   | None                | Yes, proportional scaling     |
| Feedback Mechanisms    | Placeholders        | Multiple feedback types       |
| Group Thresholds       | None                | Multi-level threshold control |
| Configurability        | Low                 | High                          |