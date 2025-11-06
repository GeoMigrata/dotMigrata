# 增强逻辑算法文档

本文档详细说明增强版计算器的算法实现，这些算法基于 `LogicModel.md` 的设计规范。

## 增强吸引力计算器 (EnhancedAttractionCalculator)

### 算法概述

实现因素-强度-敏感度三元模型，采用"拉力-推力"思想。

### 核心公式

```
吸引力(C,G) = A_G × (Pull - Push)
其中：
  Pull = Σ(w_i × s_i)  对于所有拉力因素 i
  Push = Σ(w_i × (1 - s_i))  对于所有推力因素 i
  
  s_i = normalize(v_i)  标准化后的因素值 ∈ [0,1]
  w_i = 敏感度权重
  A_G = 群体灵敏度缩放系数（默认 1.0）
```

### 主要特性

1. **拉力-推力分离**
    - 拉力因素（Positive）：高值增加吸引力
    - 推力因素（Negative）：高值降低吸引力
    - 使用 `(1 - s_i)` 处理推力，确保高推力值产生排斥效果

2. **灵敏度缩放**
    - 通过 `PopulationGroupDefinition.SensitivityScaling` (A_G) 调整
    - 允许不同群体对因素变化的敏感程度不同

3. **因素标准化**
    - 支持线性、对数、Sigmoid 三种标准化方式
    - 所有因素值映射到 [0,1] 区间

### 使用示例

```csharp
var calculator = new EnhancedAttractionCalculator();

// 计算单个城市的吸引力
var result = calculator.CalculateAttraction(city, groupDefinition, world);

Console.WriteLine($"吸引力分数: {result.AttractionScore}");
Console.WriteLine($"拉力成分: {result.PullComponent}");
Console.WriteLine($"推力成分: {result.PushComponent}");
```

### 返回值扩展

`AttractionResult` 现在包含：

- `AttractionScore`：综合吸引力分数
- `PullComponent`：拉力成分总和
- `PushComponent`：推力成分总和

---

## 增强迁移计算器 (EnhancedMigrationCalculator)

### 算法概述

实现完整的迁移决策流程，包括：

1. 迁出决策（基于吸引力阈值）
2. 目标城市选择
3. Sigmoid 概率计算
4. 成本衰减
5. 容量约束处理

### 核心公式

#### 1. 迁移概率计算

```
rawRate = 1 / (1 + exp(-k × ΔA))  # Sigmoid 函数
p_C = rawRate × exp(-λ × d)       # 成本衰减
其中：
  ΔA = A(C,G) - A(O,G)  吸引力差异
  k = Sigmoid 陡峭度系数（默认 1.0）
  d = 迁移成本（距离 × baseCost）
  λ = 成本敏感度系数（默认 0.01）
```

#### 2. 有效迁移概率

```
effectiveProbability = (1 - r) × p_C
其中：
  r = 保留率 (RetentionRate)
```

#### 3. 迁出条件

只有同时满足以下条件才考虑迁移：

```
ΔA > τ                      # 吸引力差异大于阈值
A(C,G) > α_min              # 目标吸引力高于最低要求
```

#### 4. 容量约束

```
如果 totalInflow > remainingCapacity:
    scalingFactor = remainingCapacity / totalInflow
    adjustedMigrants = floor(migrants × scalingFactor)
```

### 参数说明

| 参数       | 属性                          | 默认值  | 说明                 |
|----------|-----------------------------|------|--------------------|
| k        | SigmoidSteepness            | 1.0  | Sigmoid 陡峭度，越大转换越快 |
| λ        | CostSensitivity             | 0.01 | 成本敏感度，越大衰减越快       |
| baseCost | BaseMigrationCost           | 1.0  | 单位距离基础成本           |
| τ        | AttractionThreshold         | 0.0  | 吸引力阈值（群体定义）        |
| α_min    | MinimumAcceptableAttraction | 0.0  | 最低可接受吸引力（群体定义）     |
| m        | MovingWillingness           | -    | 迁移意愿上限（群体定义）       |
| r        | RetentionRate               | -    | 保留率（群体定义）          |

### 使用示例

```csharp
var calculator = new EnhancedMigrationCalculator
{
    SigmoidSteepness = 1.5,      // 更陡峭的概率曲线
    CostSensitivity = 0.02,      // 更高的成本敏感度
    BaseMigrationCost = 2.0      // 更高的基础成本
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
                     $"{flow.MigrantCount} 人 " +
                     $"(概率: {flow.MigrationProbability:P2})");
}
```

### 概率抽样

#### 小规模人口 (≤100)

使用二项式抽样，每个人独立决策：

```csharp
migrants = population.Count(person => random.NextDouble() < probability);
```

#### 大规模人口 (>100)

使用正态近似：

```csharp
mean = population × probability
variance = population × probability × (1 - probability)
stdDev = sqrt(variance)
sample = mean + stdDev × N(0,1)  # Box-Muller 变换
migrants = clamp(round(sample), 0, population)
```

---

## 增强反馈计算器 (EnhancedFeedbackCalculator)

### 算法概述

实现多种因素反馈机制，根据人口变化动态更新城市因素。

### 反馈类型

#### 1. PerCapitaResource（人均资源）

**适用因素**：医生/万人、公共服务、教育资源等

**公式**：

```
newValue = oldValue / populationRatio
```

**说明**：假设总资源量不变，人均值与人口成反比。

#### 2. PriceCost（价格/成本）

**适用因素**：房价、租金、生活成本等

**公式**：

```
ΔPrice = ε × (ΔP / P) × currentValue
newValue = currentValue + ΔPrice

其中：
  ε = 弹性系数（默认 0.3）
  ΔP = 人口变化
  P = 原人口
```

**说明**：价格变化与人口变化比例成正比。

#### 3. NegativeExternality（负外部性）

**适用因素**：污染、拥堵、犯罪率等

**公式**：

```
newValue = oldValue + β × ΔP

其中：
  β = 外部性系数（默认 0.0001）
  ΔP = 人口变化
```

**说明**：负外部性随人口线性增加。

#### 4. PositiveExternality（正外部性）

**适用因素**：经济产出、创新能力、就业机会等

**公式**：

```
saturationRatio = currentPopulation / saturationPoint
growthFactor = 1 - tanh(saturationRatio)
relativeGrowth = ΔP / currentPopulation
actualGrowth = relativeGrowth × growthFactor
newValue = oldValue × (1 + actualGrowth)

其中：
  saturationPoint = 饱和人口（默认 1,000,000）
```

**说明**：正外部性增长，但随人口接近饱和点而递减。

#### 5. None（无反馈）

因素值不随人口变化。

### 平滑处理

所有更新都经过指数平滑：

```
finalValue = currentValue + α × (targetValue - currentValue)
其中：
  α = SmoothingFactor ∈ [0,1]（默认 0.2）
```

### 使用示例

```csharp
// 定义反馈规则
var feedbackRules = new List<FactorFeedbackRule>
{
    new() 
    { 
        Factor = housingPriceFactor,
        FeedbackType = FeedbackType.PriceCost,
        Elasticity = 0.3  // 30% 弹性
    },
    new() 
    { 
        Factor = pollutionFactor,
        FeedbackType = FeedbackType.NegativeExternality,
        ExternalityCoefficient = 0.0001  // 每增加1人，污染增加0.0001
    },
    new() 
    { 
        Factor = economicOutputFactor,
        FeedbackType = FeedbackType.PositiveExternality,
        SaturationPoint = 500_000  // 50万人口为饱和点
    },
    new() 
    { 
        Factor = doctorsPerCapitaFactor,
        FeedbackType = FeedbackType.PerCapitaResource
    }
};

// 创建计算器
var calculator = new EnhancedFeedbackCalculator(feedbackRules)
{
    SmoothingFactor = 0.2  // 20% 平滑
};

// 应用反馈
calculator.ApplyFeedback(city, previousPopulation, currentPopulation);
```

### 动态添加规则

```csharp
var calculator = new EnhancedFeedbackCalculator();

// 运行时添加规则
calculator.AddFeedbackRule(new FactorFeedbackRule
{
    Factor = customFactor,
    FeedbackType = FeedbackType.PriceCost,
    Elasticity = 0.5
});
```

---

## 群体定义新属性

`PopulationGroupDefinition` 新增属性：

```csharp
public sealed record PopulationGroupDefinition
{
    // 原有属性
    public string DisplayName { get; init; }
    public double MovingWillingness { get; init; }  // m: 迁移意愿上限
    public double RetentionRate { get; init; }      // r: 保留率
    
    // 新增属性
    public double SensitivityScaling { get; init; } = 1.0;        // A_G: 灵敏度缩放
    public double AttractionThreshold { get; init; } = 0.0;       // τ: 吸引力阈值
    public double MinimumAcceptableAttraction { get; init; } = 0.0; // α_min: 最低吸引力
    
    public IReadOnlyList<FactorSensitivity> Sensitivities { get; }
}
```

### 示例配置

```csharp
var youngProfessionals = new PopulationGroupDefinition(sensitivities)
{
    DisplayName = "年轻专业人士",
    MovingWillingness = 0.3,           // 最多30%愿意迁移
    RetentionRate = 0.05,              // 至少5%保留
    SensitivityScaling = 1.2,          // 对因素变化更敏感
    AttractionThreshold = 0.1,         // 吸引力差需>0.1
    MinimumAcceptableAttraction = 0.2  // 目标吸引力需>0.2
};
```

---

## 城市容量

`City` 类新增容量属性：

```csharp
public class City
{
    // ...其他属性
    
    public int? Capacity { get; init; }  // null 或 0 表示无限容量
}
```

### 示例

```csharp
var smallCity = new City(factorValues, populationGroupValues)
{
    DisplayName = "小城市",
    Location = coordinate,
    Area = 100,
    Capacity = 50_000  // 最多容纳5万人
};
```

---

## 完整使用示例

```csharp
// 1. 创建因素定义
var incomeFactor = new FactorDefinition { ... };
var pollutionFactor = new FactorDefinition { ... };

// 2. 创建人口群体定义（带新属性）
var groupDef = new PopulationGroupDefinition(sensitivities)
{
    DisplayName = "中产阶级",
    MovingWillingness = 0.25,
    RetentionRate = 0.1,
    SensitivityScaling = 1.0,
    AttractionThreshold = 0.05,
    MinimumAcceptableAttraction = 0.1
};

// 3. 创建城市（带容量）
var city = new City(factorValues, populationValues)
{
    DisplayName = "测试城市",
    Location = new Coordinate { Latitude = 39.9, Longitude = 116.4 },
    Area = 16410,
    Capacity = 2_000_000  // 200万容量
};

// 4. 配置反馈规则
var feedbackRules = new List<FactorFeedbackRule>
{
    new() { Factor = pollutionFactor, FeedbackType = FeedbackType.NegativeExternality }
};

// 5. 使用增强计算器
var attractionCalc = new EnhancedAttractionCalculator();
var migrationCalc = new EnhancedMigrationCalculator
{
    SigmoidSteepness = 1.5,
    CostSensitivity = 0.02
};
var feedbackCalc = new EnhancedFeedbackCalculator(feedbackRules);

// 6. 运行模拟
var engine = new PipelineSimulationEngine(
    world, config,
    attractionCalc, migrationCalc, feedbackCalc);
engine.Run();
```

---

## 性能优化建议

1. **大规模人口**：使用正态近似而非逐个抽样
2. **容量约束**：仅在必要时计算（Capacity != null）
3. **反馈规则**：只为需要反馈的因素定义规则
4. **平滑因子**：较大的平滑因子减少计算频率

## 与原算法的对比

| 特性    | 原算法     | 增强算法           |
|-------|---------|----------------|
| 吸引力计算 | 简单加权和   | 拉力-推力分离        |
| 迁移概率  | Tanh 函数 | Sigmoid + 成本衰减 |
| 容量约束  | 无       | 有，比例缩减         |
| 反馈机制  | 占位符     | 多种类型反馈         |
| 群体阈值  | 无       | 多层次阈值控制        |
| 可配置性  | 低       | 高              |