# 快照系统文档

## 概述

dotGeoMigrata 的快照系统为模拟世界、配置和状态提供序列化和反序列化功能。它支持 JSON 和 XML 两种格式，具有一致的命名约定和
ID 策略。

## 架构

### 组件

```
Snapshot/
├── Models/              # 快照的数据传输对象
│   ├── WorldSnapshot.cs
│   ├── InitializationSnapshot.cs
│   ├── CitySnapshot.cs
│   ├── FactorDefinitionSnapshot.cs
│   └── ...
├── Services/            # 快照服务实现
│   ├── ISnapshotService.cs
│   └── SnapshotService.cs
├── Serialization/       # 特定格式的序列化器
│   ├── JsonSnapshotSerializer.cs
│   └── XmlSnapshotSerializer.cs
└── Extensions/          # 辅助扩展方法
    └── SnapshotExtensions.cs
```

### 设计原则

1. **关注点分离**：领域对象保持纯净，DTO 处理序列化
2. **基于 ID 的引用**：对象在快照中通过 ID 引用，在领域中转换为对象引用
3. **格式特定约定**：每种格式遵循各自的最佳实践
4. **不可变性**：领域对象使用对象引用；ID 仅存在于序列化层

## 命名约定

### JSON 格式

#### 结构

- **元数据字段**：使用下划线前缀以区分数据
    - `_version`：格式版本
    - `_initialization`：世界结构和定义
    - `_simulationConfig`：配置参数
    - `_simulation`：当前状态

- **数据字段**：使用 camelCase
    - `displayName`、`factorDefinitions`、`populationGroupDefinitions`、`cities`

#### ID 约定

使用有意义的名称以提高可读性：

```json
{
  "_initialization": {
    "factorDefinitions": {
      "pollution": {
        ...
      },
      "income": {
        ...
      },
      "education": {
        ...
      }
    },
    "populationGroupDefinitions": {
      "youngProfessionals": {
        ...
      },
      "families": {
        ...
      }
    },
    "cities": {
      "beijing": {
        ...
      },
      "shanghai": {
        ...
      }
    }
  }
}
```

### XML 格式

#### 结构

- **XML 结构属性**：小写
    - `id`：元素标识符
    - `ref`：引用另一个元素
    - `version`：格式版本

- **数据属性**：PascalCase（匹配 C# DTO）
    - `DisplayName`、`Intensity`、`Population`、`MovingWillingness`

#### ID 约定

使用带前缀的数字 ID 以保持清晰：

```xml

<World version="1.0" DisplayName="示例世界">
    <World.FactorDefinitions>
        <FactorDefinition id="fd_0" DisplayName="污染"
        .../>
        <FactorDefinition id="fd_1" DisplayName="收入"
        .../>
    </World.FactorDefinitions>

    <World.PopulationGroupDefinitions>
        <PopulationGroupDefinition id="pgd_0" DisplayName="年轻专业人士"
        ...>
        <PopulationGroupDefinition.Sensitivities>
            <FactorSensitivity ref="fd_0" Sensitivity="8"/>
            <FactorSensitivity ref="fd_1" Sensitivity="10"/>
        </PopulationGroupDefinition.Sensitivities>
    </PopulationGroupDefinition>
</World.PopulationGroupDefinitions>

<World.Cities>
<City DisplayName="北京"
...>
<City.FactorValues>
    <FactorValue ref="fd_0" Intensity="45"/>
</City.FactorValues>
</City>
        </World.Cities>
        </World>
```

## ID 策略

### ID 的目的

ID **仅用于序列化**以：

1. 在快照中建立对象间的引用关系
2. 在反序列化期间保持顺序独立性
3. 支持人类可读的快照

### ID 生命周期

```
序列化：
  领域对象（对象引用）
    → 根据集合索引分配 ID
      → 使用 ID 序列化
反序列化：
  带 ID 的快照
    → 构建查找字典（ID → 对象）
      → 使用查找解析引用
        → 领域对象（对象引用）
```

### ID 模式

| 类型   | JSON ID 示例             | XML ID 示例  | 模式                   |
|------|------------------------|------------|----------------------|
| 因素定义 | `"pollution"`          | `"fd_0"`   | 有意义的名称 / `fd_{索引}`   |
| 人口群体 | `"youngProfessionals"` | `"pgd_0"`  | 有意义的名称 / `pgd_{索引}`  |
| 城市   | `"beijing"`            | `"city_0"` | 有意义的名称 / `city_{索引}` |

**注意**：JSON 允许使用有意义的名称以提高可读性，而 XML 使用数字 ID 以保持一致性。两者都在领域模型中转换为对象引用。

## 使用示例

### 导出快照

```csharp
using dotGeoMigrata.Snapshot.Services;

var snapshotService = new SnapshotService();

// 导出世界，可选配置和状态
var snapshot = snapshotService.ExportToSnapshot(
    world, 
    configuration, 
    simulationState);

// 保存为 JSON
await snapshotService.SaveJsonAsync(snapshot, "world.json");

// 保存为 XML
await snapshotService.SaveXmlAsync(snapshot, "world.xml");
```

### 导入快照

```csharp
// 从 JSON 加载
var snapshot = await snapshotService.LoadJsonAsync("world.json");

// 导入世界（将 ID 转换为对象引用）
var world = snapshotService.ImportWorld(snapshot);

// 如果存在，导入配置
var config = snapshotService.ImportSimulationConfiguration(snapshot);
```

### JSON 示例

```json
{
  "displayName": "示例世界",
  "_version": "1.0",
  "_initialization": {
    "factorDefinitions": {
      "pollution": {
        "displayName": "污染",
        "type": "Negative",
        "minValue": "0",
        "maxValue": "100",
        "transform": "Linear"
      }
    },
    "populationGroupDefinitions": {
      "youngProfessionals": {
        "displayName": "年轻专业人士",
        "movingWillingness": 0.8,
        "retentionRate": 0.3,
        "factorSensitivities": {
          "pollution": {
            "sensitivity": 8
          }
        }
      }
    },
    "cities": {
      "beijing": {
        "displayName": "北京",
        "area": 16410,
        "location": {
          "latitude": 39.9042,
          "longitude": 116.4074
        },
        "factorValues": {
          "pollution": {
            "intensity": 75
          }
        },
        "populationGroupValues": {
          "youngProfessionals": {
            "population": 5000000
          }
        }
      }
    }
  },
  "_simulationConfig": {
    "maxSteps": 100,
    "stabilizationThreshold": 0.01,
    "checkStabilization": true,
    "feedbackSmoothingFactor": 0.3,
    "randomSeed": 42
  }
}
```

## 最佳实践

### 创建快照时

1. **在 JSON 中使用有意义的 ID**：有助于可读性和调试
2. **保持一致的 ID 模式**：使用文档中的模式以保持清晰
3. **包含版本**：始终指定格式版本以实现未来兼容性
4. **保存前验证**：确保世界结构有效

### 修改快照时

1. **不要破坏引用**：确保所有引用的 ID 都存在
2. **维护结构**：遵循既定的模式
3. **保留类型信息**：保持类型字符串有效（例如 "Positive"、"Negative"）
4. **检查数字格式**：为数值使用适当的精度

### 扩展系统时

1. **更新两种格式**：保持 JSON 和 XML 同步
2. **记录新字段**：更新此文档
3. **保持向后兼容性**：如有破坏性更改，考虑版本迁移
4. **测试两个序列化器**：验证往返（保存→加载）是否有效

## 模式验证

### JSON 模式

待添加：用于验证的 JSON 模式文件。

### XML 模式

请参阅 `example/SnapshotSchema.xsd` 以获取 XML 模式定义。

## 错误处理

快照系统提供详细的错误消息：

```csharp
try {
    var world = snapshotService.ImportWorld(snapshot);
} 
catch (InvalidOperationException ex) {
    // 常见错误：
    // - "Factor 'fd_5' not found in factor definitions."
    // - "Population group 'pgd_3' not found in population group definitions."
    // - "Factor definition not found in collection."
    Console.WriteLine($"导入错误: {ex.Message}");
}
```

## 性能考虑

### 内存

- 快照在序列化/反序列化期间将所有数据保存在内存中
- 大型世界（许多城市/因素）可能需要大量内存

### 速度

- JSON 对于小到中型快照通常更快
- XML 为非常大的分层数据提供更好的结构
- 考虑对 I/O 绑定操作使用异步操作

### 优化提示

1. 对非常大的快照使用流式处理（未来增强）
2. 如果多次加载，缓存反序列化的查找
3. 考虑对大型快照文件进行压缩

## 版本管理

当前版本：**1.0**

### 未来版本

引入破坏性更改时：

1. 增加版本号
2. 实现迁移逻辑
3. 支持读取旧版本
4. 记录迁移指南

## 故障排除

### 常见问题

**问题**："Factor not found in factor definitions"

- **原因**：ID 引用在定义部分不存在
- **解决方案**：验证所有引用的 ID 都已定义

**问题**："Cannot implicitly convert type"

- **原因**：快照数据中的类型不匹配
- **解决方案**：检查数值类型和枚举字符串

**问题**："Invalid factor type: XYZ"

- **原因**：快照中的枚举值无效
- **解决方案**：使用有效值："Positive"、"Negative"

**问题**：XML 反序列化静默失败

- **原因**：属性名称不匹配
- **解决方案**：验证属性名称与模式匹配（区分大小写）

## 另请参阅

- [主 README](../../../README.zh.md) - 项目概述
- [逻辑层文档](../../Logic/README.zh.md) - 核心算法
- [模拟引擎文档](../../Simulation/README.zh.md) - 模拟设计
- [示例快照](../../../example/) - 示例文件