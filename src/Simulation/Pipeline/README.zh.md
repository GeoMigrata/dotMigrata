# 模拟管线架构文档

## 概述

新的模拟引擎采用现代化的管线（Pipeline）架构，提供更高的可扩展性、模块化和低耦合度。管线架构将模拟流程分解为独立的阶段（Stage），每个阶段负责特定的功能。

## 核心组件

### ISimulationStage - 模拟阶段接口

定义了管线中单个阶段的契约：

```csharp
public interface ISimulationStage
{
    string Name { get; }           // 阶段名称
    int Order { get; }             // 执行顺序（升序）
    SimulationStageResult Execute(SimulationContext context);
}
```

### SimulationContext - 模拟上下文

在管线各阶段之间流转的上下文对象：

```csharp
public sealed class SimulationContext
{
    public required World World { get; init; }           // 世界状态
    public required SimulationState State { get; init; } // 模拟状态
    public required Random Random { get; init; }         // 随机数生成器
    public Dictionary<string, object> SharedData { get; init; }  // 共享数据
}
```

### ISimulationPipeline - 模拟管线接口

管理和执行阶段集合：

```csharp
public interface ISimulationPipeline
{
    void AddStage(ISimulationStage stage);
    bool RemoveStage(ISimulationStage stage);
    bool Execute(SimulationContext context);
    IReadOnlyList<ISimulationStage> Stages { get; }
}
```

## 默认管线阶段

### 1. AttractionStage (Order: 100)

**功能**：计算所有城市对各人口群体的吸引力

**输入**：

- World（城市、因素、人口群体定义）
- IAttractionCalculator（吸引力计算器）

**输出**：

- SharedData["Attractions"] - 吸引力结果字典
    - Key: 人口群体名称
    - Value: 所有城市的吸引力结果列表

**使用的计算器**：

- `AttractionCalculator` - 原始实现
- `EnhancedAttractionCalculator` - 增强版（拉力-推力模型）

### 2. MigrationStage (Order: 200)

**功能**：基于吸引力差异计算迁移流

**输入**：

- SharedData["Attractions"] - 前一阶段的吸引力结果
- IMigrationCalculator（迁移计算器）

**输出**：

- SharedData["MigrationFlows"] - 迁移流列表

**使用的计算器**：

- `MigrationCalculator` - 原始实现
- `EnhancedMigrationCalculator` - 增强版（Sigmoid概率、成本衰减）

### 3. MigrationApplicationStage (Order: 300)

**功能**：应用迁移流，更新城市人口

**输入**：

- SharedData["MigrationFlows"] - 迁移流列表

**输出**：

- SharedData["PreviousPopulations"] - 迁移前的城市人口字典
- SharedData["TotalMigrants"] - 总迁移人数

**处理逻辑**：

1. 记录迁移前各城市人口
2. 按源城市分组，减少人口（迁出）
3. 按目标城市分组，增加人口（迁入）
4. 保存结果供下一阶段使用

### 4. FeedbackStage (Order: 400)

**功能**：根据人口变化更新城市因素

**输入**：

- SharedData["PreviousPopulations"] - 迁移前人口
- IFeedbackCalculator（反馈计算器）

**输出**：

- 更新后的城市因素值

**使用的计算器**：

- `FeedbackCalculator` - 原始实现
- `EnhancedFeedbackCalculator` - 增强版（多种反馈机制）

## 管线引擎使用方法

### 方法1：使用 SimulationEngineBuilder（推荐）

```csharp
var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(config)
    .UseEnhancedAttractionCalculator()
    .UseEnhancedMigrationCalculator(
        sigmoidSteepness: 1.0,
        costSensitivity: 0.01)
    .UseEnhancedFeedbackCalculator(feedbackRules)
    .AddConsoleObserver()
    .Build();

if (engine is PipelineSimulationEngine pipelineEngine)
{
    pipelineEngine.Run();
}
```

### 方法2：直接创建 PipelineSimulationEngine

```csharp
var attractionCalc = new EnhancedAttractionCalculator();
var migrationCalc = new EnhancedMigrationCalculator();
var feedbackCalc = new EnhancedFeedbackCalculator(feedbackRules);

var engine = new PipelineSimulationEngine(
    world,
    config,
    attractionCalc,
    migrationCalc,
    feedbackCalc);

engine.AddObserver(new ConsoleSimulationObserver());
engine.Run();
```

### 方法3：自定义管线

```csharp
var pipeline = new SimulationPipeline();

// 添加自定义阶段
pipeline.AddStage(new AttractionStage(new EnhancedAttractionCalculator()));
pipeline.AddStage(new CustomPreprocessingStage());  // 自定义阶段
pipeline.AddStage(new MigrationStage(new EnhancedMigrationCalculator()));
pipeline.AddStage(new MigrationApplicationStage());
pipeline.AddStage(new CustomAnalysisStage());       // 自定义阶段
pipeline.AddStage(new FeedbackStage(new EnhancedFeedbackCalculator()));

var engine = new PipelineSimulationEngine(world, config, pipeline);
engine.Run();
```

## 创建自定义阶段

实现 `ISimulationStage` 接口：

```csharp
public class CustomAnalysisStage : ISimulationStage
{
    public string Name => "Custom Analysis";
    public int Order => 350; // 在迁移应用和反馈之间执行
    public SimulationStageResult Execute(SimulationContext context)
    {
        try
        {
            // 从共享数据获取迁移流
            if (context.SharedData.TryGetValue("MigrationFlows", 
                out var flowsObj) && flowsObj is List<MigrationFlow> flows)
            {
                // 执行自定义分析
                var analysis = AnalyzeMigrationPatterns(flows);

                // 将结果存入共享数据
                context.SharedData["MigrationAnalysis"] = analysis;

                return SimulationStageResult.Successful(
                    "Analysis completed", analysis);
            }

            return SimulationStageResult.Failed("Migration flows not found");
        }
        catch (Exception ex)
        {
            return SimulationStageResult.Failed($"Analysis failed: {ex.Message}");
        }
    }
}
```

## 优势

### 1. 高度模块化

- 每个阶段职责单一
- 可独立开发、测试和维护
- 易于理解和调试

### 2. 可扩展性

- 轻松添加新阶段
- 通过 Order 属性控制执行顺序
- 支持在任意位置插入自定义逻辑

### 3. 低耦合

- 阶段间通过 SharedData 通信
- 不直接依赖其他阶段
- 使用接口而非具体实现

### 4. 灵活配置

- 可选择不同的计算器实现
- 支持完全自定义的管线
- Builder 模式简化配置

### 5. 易于测试

- 每个阶段可独立单元测试
- 可模拟上下文和共享数据
- 便于集成测试

## 向后兼容性

原始的 `SimulationEngine` 仍然保留并正常工作。用户可以：

- 继续使用 `SimulationEngine`（原始非管线版本）
- 迁移到 `PipelineSimulationEngine`（新管线版本）
- 通过 `SimulationEngineBuilder` 灵活选择

## 最佳实践

1. **使用 Builder 模式**：通过 `SimulationEngineBuilder` 配置引擎
2. **选择合适的计算器**：根据需求选择原始或增强版计算器
3. **定义清晰的阶段顺序**：使用 100 的倍数为 Order 值，便于插入新阶段
4. **合理使用 SharedData**：使用描述性的键名，避免冲突
5. **错误处理**：在自定义阶段中妥善处理异常，返回失败结果
6. **文档化自定义阶段**：为自定义阶段编写清晰的文档

## 性能考虑

- 管线本身开销极小
- 主要性能取决于各阶段的计算器实现
- SharedData 使用字典，查找效率 O(1)
- 建议大规模模拟时使用性能分析工具

## 示例：完整使用流程

```csharp
// 1. 准备数据
var world = CreateWorld();
var config = new SimulationConfiguration 
{ 
    MaxSteps = 100,
    FeedbackSmoothingFactor = 0.2
};

// 2. 定义反馈规则（可选）
var feedbackRules = new List<FactorFeedbackRule>
{
    new() { 
        Factor = housingPriceFactor, 
        FeedbackType = FeedbackType.PriceCost,
        Elasticity = 0.3 
    },
    new() { 
        Factor = pollutionFactor, 
        FeedbackType = FeedbackType.NegativeExternality,
        ExternalityCoefficient = 0.0001 
    }
};

// 3. 构建引擎
var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(config)
    .UseEnhancedAttractionCalculator()
    .UseEnhancedMigrationCalculator(sigmoidSteepness: 1.5)
    .UseEnhancedFeedbackCalculator(feedbackRules)
    .AddConsoleObserver()
    .Build() as PipelineSimulationEngine;

// 4. 运行模拟
engine?.Run();

// 5. 分析结果
var finalState = engine?.State;
Console.WriteLine($"模拟完成：{finalState?.CurrentStep} 步");
```