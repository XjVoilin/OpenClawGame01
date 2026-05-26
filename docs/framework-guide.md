# JulyCore + JulyArch v3 框架使用指南

> OpenClaw 在设计技术方案和编写代码时必须参考此文档。

---

## 一、框架全景

```
JulyEvents（事件基础设施）
     ↓
JulyCore（底层）——— 提供系统级服务（资源、UI、音频、网络、场景、存档等）
    ↓ 通过 GF 门面访问
JulyArch v3（上层）——— 提供业务架构（Store-System-Procedure-View-Event）
    ↓ 业务代码继承基类
JulyToolkit（UI 工具组件）+ JulyGame（跨项目业务系统）
    ↓
游戏项目 ——— Store 存数据、System 编排业务、Procedure 异步长流程、View 渲染+输入
```

**JulyCore** 管"引擎能做什么"（加载资源、播音频、发网络请求）。
**JulyArch v3** 管"业务代码怎么组织"（数据放哪、逻辑写哪、UI 怎么刷新）。

---

## 二、JulyCore 关键概念

### Provider（底层能力提供者）

- 接口：`IProvider`，生命周期：`InitAsync() → Shutdown()`
- 注册：`GF.RegisterProvider<IXxxProvider>(instance)`
- 获取：Module 内通过 `GetProvider<IXxxProvider>()`
- 项目中已有的 Provider：Resource、UI、Audio、Scene、Pool、Config、Save、Platform 等

### Module（功能模块）

- 接口：`IModule`，生命周期：`InitAsync() → Update() → Shutdown()`
- 基类：`ModuleBase`，提供 `GetProvider<T>()`、`GetCapability<T>()`、日志方法
- Module 之间通过 `ICapability` 接口互相访问，不直接引用

### GF 门面（全局静态入口）

```csharp
var handle = await GF.Resource.LoadAssetAsync<GameObject>("prefab_path");
await GF.UI.OpenUIFormAsync<MyPanel>();
GF.Audio.PlaySound("click");
await GF.Scene.LoadSceneAsync("SceneName");
var obj = GF.Pool.Spawn("poolName");
GF.Pool.Recycle(obj);
```

### 使用原则

- **不直接 new Provider/Module**，通过 GF 注册和获取
- **不用 Resources.Load**，走 GF.Resource（底层 YooAsset）
- **不用 coroutine**，用 UniTask

---

## 三、JulyArch v3 关键概念

### 已废弃（v3 中不再存在）

以下概念在 JulyArch v3 中已完全移除，代码中不应出现：
- ~~Mutation / IMutation / IMutationContext~~ → 改用 Store 的 internal 写方法
- ~~Query / IStoreQueries / IXxxQueries~~ → 改用 this.GetStore<XxxStore>() 直接访问 public 属性
- ~~Mutate() 扩展方法~~ → 改用 System 直接调 store.InternalMethod()
- ~~IStoreContract~~ → 不再需要
- ~~Command / ICommand~~ → 不再存在
- ~~GameContext / IGameContext~~ → 改用 ArchContext / IArchContext

### 角色总览

| 角色 | 职责 | 基类 | 能力接口 |
|---|---|---|---|
| **Store** | 持有数据 + 业务规则 | `StoreBase<TData>` / `SavableStoreBase<TData>` | `ICanEvent`（发布事件） |
| **System** | 编排业务流程，驱动 Store | `GameSystemBase` | `ICanGetStore` + `ICanEvent` + `ICanGetSystem` + `ICanRunProcedure` |
| **Procedure** | 异步长流程 | `ProcedureBase`（override `OnExecuteAsync`） | 同 System |
| **View** | 读取数据 + 订阅事件 + 驱动 UI | `GameView` / `GameUIView` / `MiniGameView` | `ICanGetStore` + `ICanEvent` + `ICanGetSystem` |

### 数据访问模式（核心变化）

```csharp
// v3：直接获取具体 Store 类，public 读 + internal 写
var store = this.GetStore<FarmStore>();
int water = store.WaterLevel;        // public 读
store.SetWaterLevel(10);             // internal 写（仅同程序集 System 可调）
```

**关键规则**：
- Store 的写方法用 `internal` 修饰 — 同程序集（System）可写，跨程序集（View/外部）只读
- 不再有 Query<>() / Mutate<>() / Mutation 类 / IXxxStore 接口
- ArchContext.RegisterStore 按具体类型直接注册

### 数据流

```
[View] ——用户操作——→ [System.PublicMethod()]
                          │
                          ├→ store.InternalWrite() → this.Publish(event) → [View 刷新]
                          │
                          └→ this.RunProcedure(new XxxProcedure())
                                          │
                                          ├→ store.InternalWrite()
                                          └→ this.Publish(event)
```

### ArchContext（协调中心）

```csharp
var ctx = new ArchContext();
ctx.RegisterStore(new FarmStore());
ctx.RegisterSystem(new FarmSystem());
await ctx.InitializeAsync(ct);
ctx.Update(Time.deltaTime);
ctx.Shutdown();
```

---

## 四、各角色代码模板

### Store（数据层）

```csharp
public class FarmData
{
    public int Gold;
    public List<CropInstance> Crops = new();
}

public class FarmStore : StoreBase<FarmData>
{
    // public 读属性
    public int Gold => Data.Gold;
    public IReadOnlyList<CropInstance> Crops => Data.Crops;

    // internal 写方法（仅同程序集 System 可调用）
    internal void AddGold(int amount) => Data.Gold += amount;
    internal void AddCrop(CropInstance crop) => Data.Crops.Add(crop);
    internal void SetCropWatered(int index, bool watered) => Data.Crops[index].Watered = watered;
}
```

### System（业务编排）

```csharp
public class FarmSystem : GameSystemBase
{
    protected override void OnInitialize()
    {
        var store = this.GetStore<FarmStore>();
        // 初始化逻辑
    }

    // 公开方法供 View 调用
    public void WaterCrop(int cropIndex)
    {
        var store = this.GetStore<FarmStore>();
        store.SetCropWatered(cropIndex, true);
        this.Publish(new CropWateredEvent(cropIndex));
    }

    public void SellProduct(int productValue)
    {
        var store = this.GetStore<FarmStore>();
        store.AddGold(productValue);
        this.Publish(new GoldChangedEvent());
    }
}
```

### Procedure（异步长流程）

```csharp
public class HarvestProcedure : ProcedureBase
{
    private readonly int _cropIndex;

    public HarvestProcedure(int cropIndex) => _cropIndex = cropIndex;

    protected override async UniTask OnExecuteAsync(CancellationToken ct)
    {
        var store = this.GetStore<FarmStore>();

        // 异步操作（如播放动画、加载资源）
        await UniTask.Delay(500, cancellationToken: ct);

        store.RemoveCrop(_cropIndex);
        store.AddGold(50);

        this.Publish(new CropHarvestedEvent(_cropIndex));
        this.Publish(new GoldChangedEvent());
    }
}

// System 中触发
public void HarvestCrop(int cropIndex)
{
    this.RunProcedure(new HarvestProcedure(cropIndex), _cts.Token).Forget();
}
```

### View（UI / 场景渲染）

```csharp
public class FarmHudView : GameUIView
{
    [SerializeField] private Text _goldText;

    protected override void OnViewEnable()
    {
        this.Subscribe<GoldChangedEvent>(OnGoldChanged);

        // 初始刷新
        var store = this.GetStore<FarmStore>();
        _goldText.text = store.Gold.ToString();
    }

    // OnViewDisable 时 base 自动 UnsubscribeAll

    private void OnGoldChanged(GoldChangedEvent evt)
    {
        var store = this.GetStore<FarmStore>();
        _goldText.text = store.Gold.ToString();
    }
}
```

---

## 五、关键规则速查

| 规则 | 原因 |
|---|---|
| **Store 用 public 读 + internal 写** | View 只读，System 可写，编译期保证 |
| **不再有 Mutation / Query** | v3 简化，Store 直接暴露方法 |
| **System 不持有 View 引用** | 需要 await View 时拆 Procedure |
| **Procedure 每次 new** | 一次性实例，不复用 |
| **Store 是唯一数据源** | 需要被第二个类读的数据就放 Store |
| **热路径零 GC Alloc** | OnUpdate 里不 new List/string/lambda |
| **事件订阅在 OnViewEnable，退订靠基类 OnDisable** | GameView.OnDisable 自动 UnsubscribeAll |
| **异步必须传 CancellationToken** | 场景切换/游戏退出时能取消 |

---

## 六、能力接口速查（ArchExtensions）

所有实现 `IArchNode` 的角色都可以用对应能力：

```csharp
this.GetStore<XxxStore>()            // 获取 Store（返回具体类，public 读 + internal 写）
this.GetSystem<XxxSystem>()          // 获取 System
this.Subscribe<XxxEvent>(handler)    // 订阅事件
this.Unsubscribe<XxxEvent>(handler)  // 退订事件
this.Publish(new XxxEvent())         // 发布事件
this.RunProcedure(proc, ct)          // 运行 Procedure
```

---

## 七、项目结构约定

按功能模块组织：

```
Scripts/
├── Shared/                  # 跨模块共享的枚举、常量、工具
└── Modules/
    ├── Farm/
    │   ├── FarmStore.cs
    │   ├── FarmData.cs
    │   ├── FarmSystem.cs
    │   ├── FarmEvents.cs
    │   └── Procedures/
    ├── Inventory/
    │   ├── InventoryStore.cs
    │   ├── InventorySystem.cs
    │   └── Procedures/
    └── ...
Views/
├── Windows/                 # UI 窗口
└── World/                   # 世界场景视图
```

原则：每个模块下放该模块的 Store/System/Events/Procedures，View 单独放顶层。
