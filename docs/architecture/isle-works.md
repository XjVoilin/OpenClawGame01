# 岛工坊 Isle Works - 技术方案

> 对应设计文档：[design/isle-works.md](../design/isle-works.md)
> 框架参考：[framework-guide.md](../framework-guide.md)

---

## 一、模块划分

### 架构角色分配

| 类名 | 角色 | 所属模块/目录 | 职责 |
|---|---|---|---|
| `GridStore` | Store | `Modules/Grid` | 网格数据：地块状态、建筑占位、地形 |
| `InventoryStore` | Store | `Modules/Economy` | 玩家经济：金币、港口待售产品 |
| `TechStore` | Store | `Modules/Tech` | 科技进度：当前时代、已解锁机器/配方 |
| `ConveyorSimSystem` | System (IUpdatableSystem) | `Modules/Production` | **热路径**：每帧推进传送带物品流转 |
| `ProductionSystem` | System (IUpdatableSystem) | `Modules/Production` | **热路径**：每帧推进机器加工进度 |
| `BuildSystem` | System | `Modules/Grid` | 接收建造/拆除命令，验证合法性 |
| `EconomySystem` | System | `Modules/Economy` | 处理卖出、购买、扩岛交易 |
| `IslandSystem` | System | `Modules/Island` | 岛屿地块解锁与价格计算 |
| `TechSystem` | System | `Modules/Tech` | 检测里程碑、触发时代升级 |
| `EraTransitionProcedure` | Procedure | `Modules/Tech` | 时代切换动画编排（await View） |
| `GridView` | View | `Views/World` | 网格渲染：地块、建筑、传送带、物品流动 |
| `GameHUD` | View (Window) | `Views/Windows/GameHUD` | 顶部状态栏：金币、时代、产值 |
| `BuildWindow` | View (Window) | `Views/Windows/BuildWindow` | 建造面板：机器选择、拆除模式 |
| `IslandMapView` | View | `Views/World` | 岛屿地图：可购买地块、迷雾 |
| `PlaceholderVisuals` | View | `Views/World` | 占位符可视化 |
| `GameUIView` | View | `Views/Windows` | UI 窗口基础管理 |

### 项目目录

按功能模块组织，每个模块下按角色类型分子目录（与 GooseMarket 一致）。

```
Assets/Game/
├── Scripts/
│   ├── HotUpdateRegistrar.cs               # 热更注册入口
│   ├── Context/                             # 全局上下文（待建）
│   ├── Shared/                              # 跨模块共享
│   │   ├── UIWindowId.cs
│   │   └── Utils/
│   │       └── Config.cs
│   ├── Modules/
│   │   ├── Grid/                            # 网格与建造
│   │   │   ├── GridStore.cs
│   │   │   ├── GridData.cs
│   │   │   ├── IGridQueries.cs
│   │   │   ├── GridEvents.cs
│   │   │   ├── BuildSystem.cs
│   │   │   └── Direction.cs
│   │   ├── Production/                      # 传送带与加工
│   │   │   ├── ConveyorSimSystem.cs
│   │   │   ├── ProductionSystem.cs
│   │   │   ├── ConveyorSegment.cs
│   │   │   ├── MachineInstance.cs
│   │   │   ├── MachineType.cs
│   │   │   ├── ResourceType.cs
│   │   │   └── SimConstants.cs
│   │   ├── Economy/                         # 经济与交易
│   │   │   ├── InventoryStore.cs
│   │   │   ├── IInventoryQueries.cs
│   │   │   ├── EconomySystem.cs
│   │   │   └── EconomyEvents.cs
│   │   ├── Island/                          # 岛屿扩展
│   │   │   ├── IslandSystem.cs
│   │   │   └── IslandPriceCalculator.cs
│   │   └── Tech/                            # 科技与时代
│   │       ├── TechStore.cs
│   │       ├── ITechQueries.cs
│   │       ├── TechSystem.cs
│   │       ├── TechEvents.cs
│   │       ├── EraTransitionProcedure.cs
│   │       ├── AudioFeedbackManager.cs
│   │       └── ParticleFeedbackManager.cs
│   ├── Providers/                           # Provider 实现
│   │   ├── Config/
│   │   │   ├── LubanConfigProvider.cs
│   │   │   └── LubanUIWindowConfigProvider.cs
│   │   └── Localization/
│   │       └── LubanLocalizationProvider.cs
│   ├── Views/                               # View 层（MonoBehaviour）
│   │   ├── Windows/                         # UI 窗口
│   │   │   ├── GameUIView.cs                # UI 窗口基础
│   │   │   ├── BuildWindow/
│   │   │   │   └── BuildWindow.cs
│   │   │   └── GameHUD/
│   │   │       └── GameHUD.cs
│   │   └── World/                           # 世界场景视图
│   │       ├── GridView.cs
│   │       ├── IslandMapView.cs
│   │       └── PlaceholderVisuals.cs
│   ├── Generated/                           # Luban 自动生成（禁止手改）
│   │   └── Configs/
│   └── Editor/                              # 编辑器工具
│       └── LubanGenerator.cs
├── Res/
│   ├── Configs/                             # Luban 生成的 JSON
│   ├── Prefabs/
│   └── Textures/
├── Art/
│   └── Textures/
└── Scenes/
    └── Main.unity
```

**组织原则**：
- `Modules/{功能}/` 下放该功能的 Store、System、Events、Data、Mutations、Procedures
- 文件少的模块不必建子目录，直接放模块根
- `Views/Windows/` 放 UI 窗口（继承窗口基类），每个窗口一个子文件夹
- `Views/World/` 放世界场景 View（GridView、IslandMapView 等）
- `Shared/` 放被多个模块共用的枚举、常量、工具类
- `Providers/` 放 JulyCore Provider 实现（Config、Localization 等）

---

## 二、核心数据结构

### 2.1 网格与地块

```csharp
namespace IsleWorks.Data
{
    public enum TileType : byte
    {
        Locked,     // 未购买（迷雾）
        Normal,     // 普通地块
        Water,      // 水域（需填海）
        Mountain,   // 山地（需平整）
        Port,       // 港口（卖产品）
    }

    public enum Direction : byte
    {
        Up = 0, Right = 1, Down = 2, Left = 3
    }
}
```

```csharp
namespace IsleWorks.Data
{
    /// <summary>
    /// 网格数据 —— GridStore 的 TData。
    /// 使用扁平数组，索引 = x + y * Width，缓存友好。
    /// </summary>
    public class GridData
    {
        public int Width;
        public int Height;
        public TileType[] Tiles;            // 地块类型
        public int[] BuildingIds;           // 每格的建筑 ID（0 = 空）
        public List<MachineInstance> Machines;
        public List<ConveyorSegment> Conveyors;
        public int NextBuildingId;          // 自增 ID 分配器
    }
}
```

### 2.2 机器与传送带

```csharp
namespace IsleWorks.Simulation
{
    /// <summary>
    /// 机器实例 —— 运行时状态，非配表。
    /// 配表数据（加工时间、输入输出配方）通过 MachineType 查 Luban 表。
    /// </summary>
    public class MachineInstance
    {
        public int Id;
        public int MachineTypeId;       // 对应 Luban 配表 ID
        public Vector2Int Position;     // 左下角格子坐标
        public Vector2Int Size;         // 占位（1×1 或 2×2）

        // 运行时加工状态
        public ResourceType[] InputSlots;       // 当前输入缓冲
        public int InputCount;
        public ResourceType OutputSlot;         // 当前输出缓冲（None = 空）
        public float ProcessTimer;              // 加工剩余时间
        public bool IsProcessing;
    }

    /// <summary>
    /// 传送带段 —— 一条直线方向的传送带。
    /// 物品用环形缓冲存储，避免 GC。
    /// </summary>
    public class ConveyorSegment
    {
        public int Id;
        public Vector2Int Position;
        public Direction Direction;

        // 物品槽（固定容量，环形缓冲）
        public ResourceType[] Slots;    // 容量 = SimConstants.ConveyorCapacity
        public int HeadIndex;           // 队头（出口端）
        public int Count;               // 当前物品数

        // 连接关系（初始化时构建，运行时只读）
        public int NextSegmentId;       // 下游传送带/机器 ID（-1 = 无）
        public int PrevSegmentId;       // 上游传送带/机器 ID（-1 = 无）
        public bool IsBlocked;          // 出口端堵塞
    }

    public static class SimConstants
    {
        public const int ConveyorCapacity = 3;
        public const float ConveyorMoveInterval = 0.5f; // 每 0.5 秒推进一格
    }
}
```

### 2.3 资源与配方

```csharp
namespace IsleWorks.Data
{
    /// <summary>
    /// 资源类型枚举。对应 Luban resource 表的 ID。
    /// MVP 阶段硬编码枚举，后期可改为纯 int ID 查表。
    /// </summary>
    public enum ResourceType : int
    {
        None = 0,
        // 基础资源
        Wood = 101,
        Ore = 102,
        Coal = 103,
        Water = 104,
        Oil = 105,
        // 中间产品
        Plank = 201,        // 木材 → 木板
        Ingot = 202,        // 矿石 → 金属锭
        Plastic = 203,      // 石油 → 塑料
        // 高级产品
        Tool = 301,         // 金属锭 + 木板
        CircuitBoard = 302, // 塑料 + 金属锭
        Automaton = 401,    // 电路板 + 工具
    }

    /// <summary>
    /// 配方 —— Luban 配表结构。
    /// 一个配方描述：输入资源列表 → 输出资源 + 加工时间。
    /// </summary>
    public class Recipe
    {
        public int Id;
        public ResourceType[] Inputs;       // 需要的输入（有序）
        public ResourceType Output;
        public float ProcessTime;           // 秒
        public int RequiredEra;             // 解锁所需时代
    }
}
```

### 2.4 经济与科技

```csharp
namespace IsleWorks.Data
{
    public class InventoryData
    {
        public int Gold;
        public int TotalProductionValue;    // 累计产值（触发里程碑用）
    }

    public class TechData
    {
        public int CurrentEra;              // 0=石器, 1=铜器, 2=蒸汽, 3=电气
        public HashSet<int> UnlockedMachineTypes;
        public HashSet<int> UnlockedRecipes;
    }
}
```

---

## 三、关键接口

### 3.1 Store 查询接口

```csharp
public interface IGridQueries : IStoreQueries
{
    int Width { get; }
    int Height { get; }
    TileType GetTile(int x, int y);
    int GetBuildingId(int x, int y);
    bool CanPlace(Vector2Int pos, Vector2Int size);
    MachineInstance GetMachine(int id);
    ConveyorSegment GetConveyor(int id);
    IReadOnlyList<MachineInstance> AllMachines { get; }
    IReadOnlyList<ConveyorSegment> AllConveyors { get; }
}

public interface IInventoryQueries : IStoreQueries
{
    int Gold { get; }
    int TotalProductionValue { get; }
}

public interface ITechQueries : IStoreQueries
{
    int CurrentEra { get; }
    bool IsMachineUnlocked(int machineTypeId);
    bool IsRecipeUnlocked(int recipeId);
}
```

### 3.2 System 公开方法（View 调用入口）

```csharp
// BuildSystem —— View 触发建造/拆除
public class BuildSystem : GameSystemBase
{
    public void PlaceBuilding(Vector2Int pos, int machineTypeId) { /* 验证 → Mutate */ }
    public void PlaceConveyor(Vector2Int pos, Direction dir) { /* 验证 → Mutate */ }
    public void RemoveBuilding(Vector2Int pos) { /* 验证 → Mutate（退款） */ }
}

// EconomySystem —— 卖产品、买地块
public class EconomySystem : GameSystemBase
{
    public void SellAtPort() { /* 收集港口产品 → Mutate 加金币 */ }
    public void PurchaseTile(Vector2Int tilePos) { /* 验证金币 → Mutate 解锁地块 */ }
}

// TechSystem —— 里程碑检测
public class TechSystem : GameSystemBase
{
    public void CheckMilestone() { /* 读 TotalProductionValue → 触发 Procedure */ }
}
```

---

## 四、数据流

### 4.1 建造流程

```
[BuildPanelView] 玩家点击机器图标，进入建造模式
    → [GridView] 玩家点击网格位置
    → BuildSystem.PlaceBuilding(pos, typeId)
        → 验证：CanPlace? 金币足够? 已解锁?
        → this.Mutate(new PlaceBuildingMutation(pos, typeId))
            → GridStore：写入 BuildingIds[]、创建 MachineInstance
            → InventoryStore：扣金币
        → this.Publish(new BuildingPlacedEvent(pos, typeId))
    → [GridView] 收到事件 → 创建建筑 SpriteRenderer
    → [HudView] 收到事件 → 刷新金币显示
```

### 4.2 传送带物品流转（热路径）

```
ConveyorSimSystem.OnUpdate(deltaTime):
    _moveTimer += deltaTime
    if _moveTimer < SimConstants.ConveyorMoveInterval: return
    _moveTimer -= SimConstants.ConveyorMoveInterval

    // 从尾到头遍历（避免同帧重复推进）
    for each segment in AllConveyors (reverse order):
        if segment.Count == 0: continue
        
        head item = segment.Slots[segment.HeadIndex]
        next = GetNextTarget(segment.NextSegmentId)
        
        if next 能接收:
            移出 head item → 推入 next
            segment.IsBlocked = false
        else:
            segment.IsBlocked = true  // 堵塞向上游传播
```

**零 GC 保证**：遍历用 `for (int i = ...)` 而非 LINQ/foreach-on-interface；物品存储在固定容量数组（环形缓冲），不 new/resize。

### 4.3 机器加工流程（热路径）

```
ProductionSystem.OnUpdate(deltaTime):
    for each machine in AllMachines:
        if machine.IsProcessing:
            machine.ProcessTimer -= deltaTime
            if machine.ProcessTimer <= 0:
                machine.OutputSlot = recipe.Output
                machine.IsProcessing = false
                // 尝试推出到下游传送带
        else if machine.OutputSlot == None:
            if 输入缓冲满足配方:
                消耗输入 → 开始加工
                machine.ProcessTimer = recipe.ProcessTime
                machine.IsProcessing = true
```

### 4.4 时代升级流程

```
EconomySystem 卖产品时:
    → Mutate(new SellProductMutation(...))
    → Publish(new ProductSoldEvent(value))

TechSystem 订阅 ProductSoldEvent:
    → 读 InventoryStore.TotalProductionValue
    → 达到里程碑阈值?
    → this.RunProcedure(new EraTransitionProcedure(eraTransitionView), _cts.Token).Forget()

EraTransitionProcedure.ExecuteAsync:
    → await _transitionView.PlayEraAnimation(ct)   // 全屏动画
    → this.Mutate(new AdvanceEraMutation())         // 推进时代、解锁机器/配方
    → this.Publish(new EraChangedEvent(newEra))     // View 刷新
```

---

## 五、依赖关系

### 框架依赖

| 框架 | 使用的能力 | 说明 |
|---|---|---|
| **JulyCore - Resource** | `GF.Resource.LoadAssetAsync` | 加载机器 prefab、地块贴图 |
| **JulyCore - Audio** | `GF.Audio.PlaySound` | 建造/拆除/卖出音效 |
| **JulyCore - Config** | `GF.Config` | 加载 Luban 配表（机器表、配方表、里程碑表） |
| **JulyCore - Save** | `GF.Save` | 存档/读档（GridData + InventoryData + TechData 序列化） |
| **JulyCore - Pool** | `GF.Pool` | 传送带物品 Sprite 的对象池复用 |
| **JulyArch - 全部** | Store / System / Mutation / Procedure / View / EventBus | 业务架构基座 |

### 框架缺失能力

| 缺失 | 影响 | 应对 |
|---|---|---|
| JulyArch 无内置 Tilemap 集成 | GridView 需要自己管理 Tilemap/SpriteRenderer | 项目层自行封装 `GridRenderer` 组件 |
| JulyCore 无 Input 抽象层（新版可能有） | 建造模式的输入处理 | 项目层用 Unity InputSystem 或自封装 |

---

## 六、Luban 配表设计

### 6.1 机器表 `machine`

| 字段 | 类型 | 说明 |
|---|---|---|
| id | int | 机器类型 ID |
| name | string | 显示名 |
| size_x | int | 占位宽 |
| size_y | int | 占位高 |
| recipe_id | int | 对应配方 ID（0 = 采矿机/传送带等特殊类型） |
| cost | int | 建造金币成本 |
| refund_ratio | float | 拆除退款比例（0.0~1.0） |
| required_era | int | 解锁所需时代 |
| sprite_key | string | 资源 key |

### 6.2 配方表 `recipe`

| 字段 | 类型 | 说明 |
|---|---|---|
| id | int | 配方 ID |
| inputs | int[] | 输入资源 ID 列表 |
| output | int | 输出资源 ID |
| process_time | float | 加工时间（秒） |
| required_era | int | 解锁所需时代 |

### 6.3 资源表 `resource`

| 字段 | 类型 | 说明 |
|---|---|---|
| id | int | 资源 ID |
| name | string | 显示名 |
| sell_price | int | 基础售价 |
| depth | int | 加工深度（0=原料, 1=初加工, 2=中间件, 3=终端） |
| sprite_key | string | 资源 key |

### 6.4 里程碑表 `milestone`

| 字段 | 类型 | 说明 |
|---|---|---|
| id | int | 里程碑 ID |
| required_value | int | 需要的累计产值 |
| unlock_era | int | 解锁的时代 |
| unlock_machines | int[] | 解锁的机器 ID 列表 |
| unlock_recipes | int[] | 解锁的配方 ID 列表 |

### 6.5 地块价格表 `tile_price`

| 字段 | 类型 | 说明 |
|---|---|---|
| index | int | 第 N 块购买的地块（1-based） |
| price | int | 价格（指数增长，由配表控制） |

---

## 七、存档方案

### 序列化结构

```csharp
[Serializable]
public class IsleWorksSaveData
{
    public GridSaveData Grid;
    public InventorySaveData Inventory;
    public TechSaveData Tech;
    public float PlayTime;
}

[Serializable]
public class GridSaveData
{
    public int Width, Height;
    public byte[] Tiles;            // TileType 直接转 byte
    public MachineSave[] Machines;
    public ConveyorSave[] Conveyors;
}

[Serializable]
public struct MachineSave
{
    public int TypeId;
    public int PosX, PosY;
    public int InputCount;
    public int[] InputSlots;        // ResourceType as int
    public int OutputSlot;
    public float ProcessTimer;
}

[Serializable]
public struct ConveyorSave
{
    public int PosX, PosY;
    public byte Dir;
    public int[] Slots;
    public int HeadIndex, Count;
}
```

存档通过 `GF.Save` 序列化为 JSON（Luban JsonSerializer 或 Unity JsonUtility），读档时反序列化重建运行时对象。

---

## 八、MVP 分期

### Phase 1：核心可玩（目标 4 周）

- [ ] GridStore + GridView：8×8 网格渲染，地块显示
- [ ] BuildSystem：放置/拆除采矿机、冶炼炉、传送带
- [ ] ConveyorSimSystem：物品在传送带上流动
- [ ] ProductionSystem：采矿机产出矿石、冶炼炉矿石→金属锭
- [ ] 港口卖出产品换金币
- [ ] 2 种基础资源（木材、矿石）+ 2 种加工品（木板、金属锭）
- [ ] HudView：金币显示

### Phase 2：扩岛与经济（目标 3 周）

- [ ] IslandMapView + 购买地块 + 地形（水域、山地）
- [ ] 扩岛费用指数增长
- [ ] 完整 5 种基础资源 + 全部配方链
- [ ] BuildPanelView：机器选择面板
- [ ] 存档/读档

### Phase 3：时代系统（目标 3 周）

- [ ] TechStore + TechSystem + 里程碑配表
- [ ] 4 个时代各自的新机器和新机制
  - 铜器：组合机（多输入）
  - 蒸汽：发电站 + 能量传输线
  - 电气：智能分拣机 + 条件路由
- [ ] EraTransitionProcedure 动画

### Phase 4：打磨（目标 2 周）

- [ ] 蓝图系统（保存/加载产线布局）
- [ ] 成就系统
- [ ] 音效、粒子特效
- [ ] Steam 成就集成
- [ ] 性能优化（大地图压力测试）

---

## 九、风险点

| 风险 | 影响 | 应对方案 |
|---|---|---|
| 传送带模拟帧开销 | 大地图（32×32+）可能超 30FPS 预算 | 分帧更新（每帧只处理 N 条传送带）；非视口内的传送带降频更新 |
| Tilemap 渲染性能 | 大量 tile 变更时 Tilemap.SetTile 开销 | 批量 SetTilesBlock；脏标记只更新变化区域 |
| 存档数据量 | 大地图机器+传送带+物品状态 | 二进制序列化替代 JSON；增量存档 |
| 资源网复杂度 | 玩家可能构建环形传送带导致死循环 | 环检测：建造时检查是否形成环路，禁止或标红警告 |
| 物品视觉同步 | 逻辑层 0.5s 一跳 vs 视觉需要平滑移动 | 渲染层做插值：View 读当前帧进度做 Lerp，不影响逻辑层 |

---

## 十、技术方案自检

- [x] 数据结构支撑 GDD 所有玩法？— 网格/机器/传送带/资源/配方/时代全覆盖
- [x] 热路径标注并零 GC？— ConveyorSimSystem + ProductionSystem 用 for+数组，无 LINQ/lambda
- [x] 模块间通信方式明确？— System→Store 走 Mutation，Store→View 走 EventBus，异步编排走 Procedure
- [x] 存档/加载考虑？— 第七节完整设计
- [x] 扩展性（新增资源/机器/配方时改哪些文件）？— 只改 Luban 配表 + ResourceType 枚举，代码层零修改
