# 模拟引擎文档

本目录包含人口迁移模拟的模拟引擎。

## 概述

模拟层提供了运行逐步模拟的编排框架，管理状态、配置和对模拟过程的观察。

## 组件

### 1. SimulationEngine（模拟引擎）

协调所有模拟活动的主编排器。

### 2. SimulationConfiguration（模拟配置）

控制模拟行为的配置参数。

### 3. SimulationState（模拟状态）

跟踪正在运行的模拟的当前状态和统计信息。

### 4. ISimulationObserver（模拟观察者接口）

用于监控模拟进度的观察者接口。

---

## SimulationEngine 设计

### 架构

`SimulationEngine` 遵循**观察者模式**并协调三个主要计算器：

- `AttractionCalculator` - 计算城市吸引指数
- `MigrationCalculator` - 确定迁移流量
- `FeedbackCalculator` - 更新城市因素

### 关键职责

1. **编排**：协调吸引指数、迁移和反馈计算的执行
2. **状态管理**：维护和更新模拟状态
3. **观察者通知**：通知观察者模拟事件
4. **稳定性检测**：监控系统何时达到平衡

---

## 模拟流程

### 初始化

```
1. 创建包含城市、因素和人口群体的 World
2. 创建带参数的 SimulationConfiguration
3. 创建 SimulationEngine(world, configuration)
4. 添加观察者（可选）
5. 调用 engine.Run() 或 engine.Step()
```

### 主循环（Run 方法）

```csharp
void Run()
{
    // 1. 初始化
    NotifyObservers_SimulationStarted()
    
    // 2. 主模拟循环
    while (!IsCompleted && CurrentStep < MaxSteps)
    {
        Step()  // 执行单个模拟步骤
        
        // 检查稳定性（如果启用了）
        if (CheckStabilization())
        {
            MarkStabilized()
            break
        }
    }
    
    // 3. 完成
    MarkCompleted()
    NotifyObservers_SimulationCompleted()
}
```

### 单步执行

每次调用 `Step()` 执行一个完整的模拟周期：

```
┌─────────────────────────────────────────────────┐
│ 步骤 N                                          │
├─────────────────────────────────────────────────┤
│                                                 │
│  1. 对于每个城市和人口群体：                    │
│     ┌─────────────────────────────────────┐    │
│     │ 计算吸引指数                          │    │
│     │  - AttractionCalculator             │    │
│     │  - 生成 AttractionResults           │    │
│     └─────────────────────────────────────┘    │
│              ↓                                  │
│     ┌─────────────────────────────────────┐    │
│     │ 计算迁移流量                        │    │
│     │  - MigrationCalculator              │    │
│     │  - 生成 MigrationFlows              │    │
│     └─────────────────────────────────────┘    │
│                                                 │
│  2. 应用所有迁移流量：                         │
│     ┌─────────────────────────────────────┐    │
│     │ 更新人口数量                        │    │
│     │  - 在城市间移动人口                 │    │
│     │  - 跟踪迁移统计                     │    │
│     └─────────────────────────────────────┘    │
│              ↓                                  │
│  3. 应用反馈效应：                             │
│     ┌─────────────────────────────────────┐    │
│     │ 更新城市因素                        │    │
│     │  - FeedbackCalculator               │    │
│     │  - 基于人口调整因素                 │    │
│     └─────────────────────────────────────┘    │
│              ↓                                  │
│  4. 更新状态并通知：                           │
│     ┌─────────────────────────────────────┐    │
│     │ 推进步骤计数器                      │    │
│     │ 记录统计信息                        │    │
│     │ 通知观察者                          │    │
│     └─────────────────────────────────────┘    │
│                                                 │
└─────────────────────────────────────────────────┘
```

### 详细步骤算法

```csharp
void Step()
{
    if (IsCompleted)
        throw InvalidOperationException
    
    allMigrationFlows = new List<MigrationFlow>()
    
    // 阶段 1：计算所有迁移
    foreach (city in world.Cities)
    {
        foreach (group in city.PopulationGroups)
        {
            // 计算每个城市对该群体的吸引指数
            attractions = attractionCalculator
                .CalculateAttractionForAllCities(world, group)
            
            // 确定该群体想要迁移到哪里
            flows = migrationCalculator
                .CalculateMigrationFlows(city, group, attractions, world, random)
            
            allMigrationFlows.AddRange(flows)
        }
    }
    
    // 阶段 2：应用迁移
    totalMigrants = ApplyMigrations(allMigrationFlows)
    
    // 阶段 3：应用反馈
    foreach (city in world.Cities)
    {
        feedbackCalculator.ApplyFeedback(city, previousPop, currentPop)
    }
    
    // 阶段 4：更新状态
    state.AdvanceStep(totalMigrants)
    RaiseStepCompletedEvent()
    NotifyObservers_StepCompleted()
}
```

---

## 配置

### SimulationConfiguration（模拟配置）

使用以下参数控制模拟行为：

#### MaxSteps（最大步数）

- **类型**：`int`（必需）
- **范围**：> 0
- **说明**：要运行的最大模拟步数
- **示例**：`MaxSteps = 100`

#### StabilizationThreshold（稳定阈值）

- **类型**：`double`
- **范围**：[0, 1]
- **默认值**：0.01
- **说明**：迁移率阈值，低于该值时认为模拟已稳定
- **公式**：`迁移率 = 总迁移人数 / 总人口`
- **示例**：如果阈值为 0.01，则当每步少于 1% 的人口迁移时模拟稳定

#### CheckStabilization（检查稳定性）

- **类型**：`bool`
- **默认值**：`true`
- **说明**：是否检查稳定性并可能提前结束
- **示例**：`CheckStabilization = true`

#### FeedbackSmoothingFactor（反馈平滑因子）

- **类型**：`double`
- **范围**：[0, 1]
- **默认值**：0.3
- **说明**：控制城市因素变化的渐进程度
    - 0.0 = 即时变化
    - 1.0 = 无变化
    - 0.3 = 渐进变化（推荐）

#### RandomSeed（随机种子）

- **类型**：`int?`（可为空）
- **默认值**：`null`（随机种子）
- **说明**：随机数生成器的种子
- **用法**：设置以获得可重现的模拟

### 配置示例

```csharp
var config = new SimulationConfiguration
{
    MaxSteps = 100,
    StabilizationThreshold = 0.01,
    CheckStabilization = true,
    FeedbackSmoothingFactor = 0.3,
    RandomSeed = 42  // 用于可重现性
};
```

---

## 状态管理

### SimulationState（模拟状态）

跟踪模拟的当前状态：

#### 属性

- **CurrentStep**：当前模拟步数（从 0 开始）
- **LastStepMigrations**：上一步中的迁移人数
- **TotalMigrations**：所有步骤的累计迁移人数
- **IsStabilized**：模拟是否已稳定
- **IsCompleted**：模拟是否已完成
- **Random**：此模拟的随机数生成器

#### 状态转换

```
[开始]
   ↓
[运行中] ─→ CurrentStep++，每步更新 TotalMigrations
   ↓
   ├─→ [已稳定] ─→ IsStabilized = true，IsCompleted = true
   └─→ [达到最大步数] ─→ IsCompleted = true
```

---

## 观察者模式

### ISimulationObserver 接口

允许外部组件监控模拟进度而不耦合到引擎。

#### 方法

```csharp
void OnSimulationStarted(SimulationState state)
```

- 在开始时调用一次
- 接收初始状态

```csharp
void OnStepCompleted(SimulationState state, IReadOnlyList<MigrationFlow> flows)
```

- 每步之后调用
- 接收更新的状态和该步骤的迁移流量

```csharp
void OnSimulationCompleted(SimulationState state)
```

- 在结束时调用一次
- 接收最终状态

### 内置观察者：ConsoleSimulationObserver

输出模拟进度的简单控制台记录器。

**构造函数参数：**

- `verbose` (bool)：如果为 true，显示详细的迁移流量

**输出示例：**

```
=== Simulation Started ===
Initial Step: 0

Step 1 completed:
  Migrations this step: 245
  Total migrations: 245
  Migration details:
    CityA -> CityB: 100 people (Young Professionals)
    CityA -> CityC: 145 people (Young Professionals)

Step 2 completed:
  Migrations this step: 198
  Total migrations: 443

...

=== Simulation Completed ===
Final Step: 47
Total Migrations: 8432
Stabilized: true
```

### 自定义观察者

您可以实现自定义观察者用于：

- 数据收集和分析
- 可视化更新
- 记录到文件或数据库
- 实时监控仪表板
- 基于阈值触发事件

**示例：**

```csharp
public class DataCollectorObserver : ISimulationObserver
{
    private List<StepData> _data = new();
    
    public void OnStepCompleted(SimulationState state, IReadOnlyList<MigrationFlow> flows)
    {
        _data.Add(new StepData
        {
            Step = state.CurrentStep,
            Migrations = state.LastStepMigrations,
            FlowCount = flows.Count
        });
    }
    
    // 其他方法...
}
```

---

## 稳定性检测

### 算法

模拟可以自动检测系统何时稳定（达到平衡）。

```csharp
bool CheckStabilization()
{
    if (CurrentStep < 2)
        return false  // 至少需要 2 步
    
    totalPopulation = world.Population
    if (totalPopulation == 0)
        return true  // 空世界是稳定的
    
    migrationRate = LastStepMigrations / totalPopulation
    
    return migrationRate < StabilizationThreshold
}
```

### 解释

- **迁移率**：上一步中迁移的总人口比例
- **阈值**：通常为 0.01（1%）
- **稳定**：当迁移低于阈值时，系统处于平衡状态

### 重要性

稳定性检测允许模拟：

1. **提前结束**：达到平衡时结束（节省计算）
2. **识别收敛**：识别到稳定状态
3. **研究平衡**：研究系统的平衡属性

---

## 事件

引擎提供 C# 事件用于集成：

### StepCompleted 事件

```csharp
event EventHandler<SimulationStepEventArgs> StepCompleted
```

**EventArgs 属性：**

- `State`：当前模拟状态
- `MigrationFlows`：此步骤的流量

### SimulationCompleted 事件

```csharp
event EventHandler<SimulationCompletedEventArgs> SimulationCompleted
```

**EventArgs 属性：**

- `FinalState`：最终模拟状态

---

## 使用示例

### 基本模拟

```csharp
// 1. 创建世界
var world = new World(cities, factorDefinitions);

// 2. 配置
var config = new SimulationConfiguration { MaxSteps = 100 };

// 3. 创建引擎
var engine = new SimulationEngine(world, config);

// 4. 添加观察者
engine.AddObserver(new ConsoleSimulationObserver(verbose: true));

// 5. 运行
engine.Run();
```

### 逐步模拟

```csharp
var engine = new SimulationEngine(world, config);

while (!engine.State.IsCompleted && engine.State.CurrentStep < 100)
{
    engine.Step();
    
    // 每步之后的自定义逻辑
    AnalyzeResults(engine.State);
    
    if (ShouldStopEarly())
        break;
}
```

### 使用事件处理程序

```csharp
var engine = new SimulationEngine(world, config);

engine.StepCompleted += (sender, args) =>
{
    Console.WriteLine($"Step {args.State.CurrentStep}: {args.MigrationFlows.Count} flows");
};

engine.SimulationCompleted += (sender, args) =>
{
    Console.WriteLine($"Completed at step {args.FinalState.CurrentStep}");
};

engine.Run();
```

---

## 性能考虑

### 计算复杂度

对于 N 个城市和 P 个人口群体，每步：

- **吸引指数计算**：O(N × P × F)，其中 F 是每个城市的平均因素数
- **迁移计算**：O(N × P × N) = O(N² × P)
- **反馈应用**：O(N)

**总体**：每步 O(N² × P × F)

### 优化策略

1. **提前稳定**：启用 `CheckStabilization` 以在达到平衡时结束
2. **因素缓存**：如果因素不变，可以缓存标准化的因素值
3. **空间索引**：对于大量城市，考虑空间数据结构
4. **并行处理**：城市-群体对可以独立处理（未来增强）

---

## 使用的设计模式

1. **观察者模式**：用于模拟监控
2. **策略模式**：计算器可以交换（尽管目前是具体的）
3. **模板方法**：`Run()` 定义模拟骨架
4. **依赖注入**：通过构造函数注入 World 和配置

---

## 扩展点

模拟引擎可以通过以下方式扩展：

1. **自定义观察者**：实现 `ISimulationObserver`
2. **自定义配置**：扩展 `SimulationConfiguration`
3. **事件处理程序**：订阅 `StepCompleted` 和 `SimulationCompleted` 事件
4. **自定义状态跟踪**：扩展 `SimulationState`（但目前内部方法限制了这一点）

对于算法自定义，可以扩展或替换逻辑层中的计算器。