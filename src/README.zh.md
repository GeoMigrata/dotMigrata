# 源代码组织结构

本目录包含 dotGeoMigrata 模拟框架的源代码。

## 目录结构

```
src/
├── Core/           # 核心领域模型和实体
├── Logic/          # 业务逻辑计算
└── Simulation/     # 模拟编排和状态管理
```

## 层级说明

### Core（核心层）

**路径：** `src/Core/Domain/`

包含表示世界、城市、人口和因素的基础领域模型。

**子目录：**

- **Entities**：主要领域实体（World、City、PopulationGroup）
- **Values**：值对象（FactorDefinition、FactorValue、FactorSensitivity、Coordinate）
- **Enums**：枚举类型（FactorType、TransformType）

**关键类：**

- `World` - 整个模拟的顶层容器
- `City` - 表示具有位置、因素和人口的城市
- `PopulationGroup` - 具有共同迁移行为的居民子集
- `FactorDefinition` - 定义因素的元数据和标准化规则
- `FactorValue` - 城市中因素的当前值
- `Coordinate` - 地理位置及距离计算

**特点：**

- 纯领域模型
- 不依赖其他层
- 尽可能使用不可变对象
- 在构造函数/初始化器中进行丰富的验证

---

### Logic（逻辑层）

**路径：** `src/Logic/`

包含实现模拟核心逻辑的算法和计算器。

**子目录：**

- **Attraction**：城市吸引力计算
- **Migration**：迁移流量判定
- **Feedback**：迁移后因素更新

**关键类：**

- `AttractionCalculator` - 计算人口群体对城市的吸引力
- `MigrationCalculator` - 根据吸引力差异确定迁移流量
- `FeedbackCalculator` - 迁移后更新城市因素

**参见：** [Logic README](Logic/README.zh.md) 查看详细算法文档

---

### Simulation（模拟层）

**路径：** `src/Simulation/`

包含编排逐步执行的模拟引擎。

**子目录：**

- **Engine**：主模拟编排器和观察者
- **Configuration**：模拟参数
- **State**：运行时状态跟踪

**关键类：**

- `SimulationEngine` - 协调所有计算的主编排器
- `SimulationConfiguration` - 配置参数（最大步数、阈值等）
- `SimulationState` - 跟踪当前步数、迁移和状态
- `ISimulationObserver` - 监控模拟进度的接口
- `ConsoleSimulationObserver` - 内置控制台日志记录器

**参见：** [Simulation README](Simulation/README.zh.md) 查看详细引擎设计文档

---

## 依赖流向

```
Simulation（模拟层）
    ↓ （依赖于）
Logic（逻辑层）
    ↓ （依赖于）
Core（核心层）
```

- **Core** 无依赖（纯领域模型）
- **Logic** 依赖 Core（使用领域模型）
- **Simulation** 依赖 Logic 和 Core（编排所有内容）

---

## 命名空间约定

所有命名空间遵循模式：`dotGeoMigrata.{层级}.{子文件夹}`

示例：

- `dotGeoMigrata.Core.Domain.Entities`
- `dotGeoMigrata.Logic.Attraction`
- `dotGeoMigrata.Simulation.Engine`

---

## 设计原则

1. **关注点分离**：每个层级都有明确的职责
2. **领域驱动设计**：核心模型代表业务领域
3. **单一职责**：每个类都有一个明确的目的
4. **依赖倒置**：高层依赖抽象
5. **不可变性**：在适当的地方优先使用不可变对象
6. **验证**：在构造时进行验证

---

## 扩展点

### 添加新计算器

1. 在 `Logic/` 中创建新类
2. 遵循现有计算器的模式
3. 添加完整的 XML 文档
4. 在 `Logic/README.zh.md` 中记录算法

### 添加新观察者

1. 实现 `ISimulationObserver` 接口
2. 通过 `engine.AddObserver()` 添加到模拟
3. 根据需要响应模拟事件

### 添加新领域实体

1. 添加到 `Core/Domain/Entities/` 或 `Core/Domain/Values/`
2. 遵循现有模式（值使用 record，实体使用 class）
3. 在初始化器中添加验证
4. 使用 XML 注释进行文档记录

---

## 代码质量标准

- **XML 文档**：所有公共类型和成员必须有 `/// <summary>` 注释
- **验证**：在构造函数和属性初始化器中验证输入
- **命名**：使用清晰、描述性的名称，遵循 C# 约定
- **空安全**：使用 C# 的可空引用类型（项目中 `enable`）
- **异常消息**：提供清晰、可操作的错误消息
- **只读**：对不应更改的字段使用 `readonly`

---

## 快速开始

使用此框架：

1. **创建世界**：
   ```csharp
   var world = new World(cities, factorDefinitions);
   ```

2. **配置模拟**：
   ```csharp
   var config = new SimulationConfiguration { MaxSteps = 100 };
   ```

3. **创建引擎**：
   ```csharp
   var engine = new SimulationEngine(world, config);
   ```

4. **运行**：
   ```csharp
   engine.Run();
   ```

查看各子目录中的 README 文件以获取详细文档。