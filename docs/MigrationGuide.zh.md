# 迁移指南：从旧版到新管线架构

本指南帮助您将现有代码从原始 `SimulationEngine` 迁移到新的 `PipelineSimulationEngine`。

## 为什么要迁移？

新的管线架构提供：
- ✅ **更好的可扩展性** - 轻松添加自定义阶段
- ✅ **更清晰的结构** - 每个阶段职责单一
- ✅ **更强大的算法** - 基于 LogicModel.md 的增强实现
- ✅ **更灵活的配置** - Builder 模式简化设置
- ✅ **向后兼容** - 原有代码仍可正常工作

## 迁移步骤

### 步骤 1：了解新旧差异

#### 旧版代码（仍然有效）
```csharp
var engine = new SimulationEngine(world, configuration);
engine.AddObserver(new ConsoleSimulationObserver());
engine.Run();
```

#### 新版代码（推荐）
```csharp
var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(configuration)
    .UseEnhancedAttractionCalculator()
    .UseEnhancedMigrationCalculator()
    .UseEnhancedFeedbackCalculator()
    .AddConsoleObserver()
    .Build() as PipelineSimulationEngine;

engine?.Run();
```

### 步骤 2：逐步迁移

#### 最小修改迁移

如果您想使用管线架构但保持原有算法：

```csharp
var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(configuration)
    .UseOriginalAttractionCalculator()  // 使用原算法
    .UseOriginalMigrationCalculator()   // 使用原算法
    .UseOriginalFeedbackCalculator()    // 使用原算法
    .AddConsoleObserver()
    .Build() as PipelineSimulationEngine;
```

#### 渐进式迁移

逐个组件切换到增强版：

```csharp
// 第一步：只升级吸引力计算
var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(configuration)
    .UseEnhancedAttractionCalculator()     // ✅ 新算法
    .UseOriginalMigrationCalculator()      // ⏸️ 保持原样
    .UseOriginalFeedbackCalculator()       // ⏸️ 保持原样
    .Build() as PipelineSimulationEngine;

// 验证结果正常后，继续升级其他组件...
```

#### 完全迁移到增强版

```csharp
var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(configuration)
    .UseEnhancedAttractionCalculator()
    .UseEnhancedMigrationCalculator(
        sigmoidSteepness: 1.0,      // 自定义参数
        costSensitivity: 0.01,
        baseMigrationCost: 1.0)
    .UseEnhancedFeedbackCalculator(feedbackRules)
    .AddConsoleObserver()
    .Build() as PipelineSimulationEngine;
```

### 步骤 3：配置新功能

#### 添加人口群体新属性

旧版：
```csharp
var groupDef = new PopulationGroupDefinition(sensitivities)
{
    DisplayName = "年轻专业人士",
    MovingWillingness = 0.3,
    RetentionRate = 0.05
};
```

新版（可选新属性）：
```csharp
var groupDef = new PopulationGroupDefinition(sensitivities)
{
    DisplayName = "年轻专业人士",
    MovingWillingness = 0.3,
    RetentionRate = 0.05,
    SensitivityScaling = 1.2,          // 新增：灵敏度缩放
    AttractionThreshold = 0.1,         // 新增：吸引力阈值
    MinimumAcceptableAttraction = 0.2  // 新增：最低可接受吸引力
};
```

#### 添加城市容量

旧版：
```csharp
var city = new City(factorValues, populationValues)
{
    DisplayName = "北京",
    Location = coordinate,
    Area = 16410
};
```

新版（可选容量限制）：
```csharp
var city = new City(factorValues, populationValues)
{
    DisplayName = "北京",
    Location = coordinate,
    Area = 16410,
    Capacity = 2_000_000  // 新增：最大容纳200万人
};
```

#### 配置反馈规则

```csharp
var feedbackRules = new List<FactorFeedbackRule>
{
    // 房价：价格弹性反馈
    new() 
    { 
        Factor = housingPriceFactor,
        FeedbackType = FeedbackType.PriceCost,
        Elasticity = 0.3
    },

    // 污染：负外部性
    new() 
    { 
        Factor = pollutionFactor,
        FeedbackType = FeedbackType.NegativeExternality,
        ExternalityCoefficient = 0.0001
    },

    // 经济产出：正外部性
    new() 
    { 
        Factor = economicOutputFactor,
        FeedbackType = FeedbackType.PositiveExternality,
        SaturationPoint = 1_000_000
    },

    // 医疗资源：人均资源
    new() 
    { 
        Factor = healthcareFactor,
        FeedbackType = FeedbackType.PerCapitaResource
    }
};

var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(configuration)
    .UseEnhancedFeedbackCalculator(feedbackRules)  // 传入规则
    // ...
    .Build();
```

### 步骤 4：使用自定义管线（高级）

如果需要完全自定义模拟流程：

```csharp
var pipeline = new SimulationPipeline();

// 添加标准阶段
pipeline.AddStage(new AttractionStage(new EnhancedAttractionCalculator()));

// 添加自定义阶段
pipeline.AddStage(new MyCustomPreprocessingStage { Order = 150 });

pipeline.AddStage(new MigrationStage(new EnhancedMigrationCalculator()));
pipeline.AddStage(new MigrationApplicationStage());

// 添加自定义分析阶段
pipeline.AddStage(new MyCustomAnalysisStage { Order = 350 });

pipeline.AddStage(new FeedbackStage(new EnhancedFeedbackCalculator()));

// 使用自定义管线
var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(configuration)
    .WithCustomPipeline(pipeline)
    .Build() as PipelineSimulationEngine;
```

## 常见问题

### Q1: 旧代码还能用吗？
**A:** 是的！`SimulationEngine` 保持不变，所有旧代码都能正常运行。

### Q2: 新旧算法结果一样吗？
**A:** 不完全一样。增强版实现了更精确的算法（基于 LogicModel.md），但如果使用 `UseOriginal*` 方法，行为与旧版相同。

### Q3: 必须同时升级所有组件吗？
**A:** 不必须。可以混用原始和增强版计算器。

### Q4: 如何验证迁移结果？
**A:**
1. 先使用原始计算器运行，记录结果
2. 逐个切换到增强版，对比差异
3. 使用小规模数据集测试
4. 添加日志观察各阶段输出

### Q5: 性能会受影响吗？
**A:** 管线架构本身开销极小。增强版算法可能稍慢（因为更复杂），但通常差异不大。

## 迁移检查清单

- [ ] 备份原有代码
- [ ] 了解新架构和算法
- [ ] 决定迁移策略（完全/渐进/混合）
- [ ] 更新人口群体定义（添加新属性）
- [ ] 更新城市定义（添加容量）
- [ ] 配置反馈规则（如使用增强反馈）
- [ ] 测试新版本
- [ ] 对比结果
- [ ] 更新文档
- [ ] 部署新版本

## 示例：完整迁移对比

### 迁移前
```csharp
// 创建世界
var world = new World(cities, factorDefinitions, populationGroupDefinitions)
{
    DisplayName = "中国主要城市"
};

// 创建配置
var config = new SimulationConfiguration
{
    MaxSteps = 100,
    FeedbackSmoothingFactor = 0.3
};

// 创建并运行引擎
var engine = new SimulationEngine(world, config);
engine.AddObserver(new ConsoleSimulationObserver());
engine.Run();

// 获取结果
var finalState = engine.State;
Console.WriteLine($"模拟完成：{finalState.CurrentStep} 步");
```

### 迁移后
```csharp
// 创建世界（添加容量）
var cities = CreateCitiesWithCapacity();  // 新增容量属性
var world = new World(cities, factorDefinitions, populationGroupDefinitions)
{
    DisplayName = "中国主要城市"
};

// 创建配置（同旧版）
var config = new SimulationConfiguration
{
    MaxSteps = 100,
    FeedbackSmoothingFactor = 0.3
};

// 定义反馈规则
var feedbackRules = new List<FactorFeedbackRule>
{
    new() { Factor = housingPriceFactor, FeedbackType = FeedbackType.PriceCost },
    new() { Factor = pollutionFactor, FeedbackType = FeedbackType.NegativeExternality }
};

// 使用 Builder 创建引擎
var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(config)
    .UseEnhancedAttractionCalculator()
    .UseEnhancedMigrationCalculator(
        sigmoidSteepness: 1.0,
        costSensitivity: 0.01)
    .UseEnhancedFeedbackCalculator(feedbackRules)
    .AddConsoleObserver()
    .Build() as PipelineSimulationEngine;

// 运行模拟
engine?.Run();

// 获取结果
var finalState = engine?.State;
Console.WriteLine($"模拟完成：{finalState?.CurrentStep} 步");

// 新功能：访问管线
var pipeline = engine?.Pipeline;
Console.WriteLine($"管线阶段数：{pipeline?.Stages.Count}");
```

## 性能优化建议

迁移到新架构后的优化建议：

1. **调整 Sigmoid 陡峭度**：根据数据特点调整 `SigmoidSteepness`
2. **优化成本系数**：调整 `CostSensitivity` 和 `BaseMigrationCost`
3. **合理设置容量**：只为需要限制的城市设置 `Capacity`
4. **精简反馈规则**：只为需要反馈的因素添加规则
5. **调整平滑因子**：较大的 `SmoothingFactor` 可减少波动

## 获取帮助

- 查看 `/src/Simulation/Pipeline/README.zh.md` - 管线架构文档
- 查看 `/src/Logic/EnhancedAlgorithms.zh.md` - 增强算法文档
- 查看 `/src/Logic/LogicModel.md` - 算法设计规范
- 提交 GitHub Issue 获取支持

## 总结

迁移到新架构的好处：
- ✅ 更现代化的设计
- ✅ 更强大的功能
- ✅ 更好的可扩展性
- ✅ 更精确的算法
- ✅ 保持向后兼容

建议采用渐进式迁移策略，逐步验证每个组件的升级效果。