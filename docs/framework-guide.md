# JulyCore + JulyArch 框架使用指南

> OpenClaw 在设计技术方案和编写代码时必须参考此文档。

---

## 一、两层框架的关系

```
JulyCore（底层）——— 提供系统级服务（资源、UI、音频、网络、场景、存档等）
    ↓ 通过 GF 门面访问
JulyArch（上层）——— 提供业务架构（Store-System-Mutation-Procedure-View-Event）
    ↓ 业务代码继承基类
游戏项目 ——— Store 存数据、System 帧逻辑+命令、Procedure 异步编排、View 渲染+输入
```

**JulyCore** 管"引擎能做什么"（加载资源、播音频、发网络请求）。
**JulyArch** 管"业务代码怎么组织"（数据放哪、逻辑写哪、UI 怎么刷新）。

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
// 资源加载
var handle = await GF.Resource.LoadAssetAsync<GameObject>("prefab_path");
// UI
await GF.UI.OpenUIFormAsync<MyPanel>();
// 音频
GF.Audio.PlaySound("click");
// 场景
await GF.Scene.LoadSceneAsync("SceneName");
// 对象池
var obj = GF.Pool.Spawn("poolName");
GF.Pool.Recycle(obj);
```

### 使用原则

- **不直接 new Provider/Module**，通过 GF 注册和获取
- **不用 Resources.Load**，走 GF.Resource（底层 YooAsset）
- **不用 coroutine**，用 UniTask

---

## 三、JulyArch 关键概念

### 角色总览

| 角色 | 职责 | 基类 | 能做 | 不能做 |
|---|---|---|---|---|
| **Store** | 持有业务数据 | `StoreBase<TData>` | 自身读写、发事件 | 不调 Mutate、不调 System |
| **System** | 帧逻辑 + 接收命令 | `GameSystemBase` | Query/Mutate/Publish/RunProcedure | **不持有 View** |
| **Mutation** | 同步原子状态变更 | `readonly struct` 实现 `IMutation` | 通过 `ctx.GetStore<T>()` 跨 Store 写 | 不做异步、不调 System |
| **Procedure** | 异步长流程编排 | `ProcedureBase` | Query/Mutate/Publish/await View/嵌套 Procedure | — |
| **View** | 渲染 + 用户输入 | `GameView`（MonoBehaviour） | Subscribe 事件刷新 UI、调 System 公开方法 | 不直接改 Store（走 Mutate） |
| **EventBus** | 同步事件广播 | — | 解耦 Store ↔ View 通信 | — |

### 数据流

```
[View] ——用户操作——→ [System.PublicMethod()]
                          │
                          ├→ Mutate(...) → [Store 改数据] → Event → [View 刷新]
                          │
                          └→ RunProcedure(new XxxProcedure(viewRefs))
                                          │
                                          ├→ await view.PlayAsync(ct)
                                          ├→ Mutate(...)
                                          └→ await RunProcedure(childProcedure)
```

### GameContext（协调中心）

```csharp
// 创建
var ctx = new GameContext();

// 注册（初始化阶段）
ctx.RegisterStore(new MyStore());
ctx.RegisterSystem(new MySystem());

// 初始化（按顺序：Store.Load → OnReady → System.OnInit → OnStart）
await ctx.InitializeAsync(ct);

// 帧驱动（放在 MonoBehaviour.Update 中）
ctx.Update(Time.deltaTime);

// 关闭
ctx.Shutdown();
```

---

## 四、各角色代码模板

### Store（数据层）

```csharp
// 1. 定义数据类
public class FactoryData
{
    public int Gold;
    public int CurrentEra;
    public List<MachineInstance> Machines = new();
}

// 2. 定义查询接口（外部只读访问）
public interface IFactoryQueries : IStoreQueries
{
    int Gold { get; }
    int CurrentEra { get; }
    IReadOnlyList<MachineInstance> Machines { get; }
}

// 3. 实现 Store
public class FactoryStore : StoreBase<FactoryData>, IFactoryQueries
{
    public int Gold => Data.Gold;
    public int CurrentEra => Data.CurrentEra;
    public IReadOnlyList<MachineInstance> Machines => Data.Machines;

    // Store 内部可以暴露写方法供 Mutation 使用
    public void AddGold(int amount) => Data.Gold += amount;
    public void SetEra(int era) => Data.CurrentEra = era;
}
```

### Mutation（同步状态变更）

```csharp
// 推荐 readonly struct，零 GC
public readonly struct SellProductMutation : IMutation
{
    private readonly int _productValue;

    public SellProductMutation(int productValue) => _productValue = productValue;

    public MutationResult Execute(IMutationContext ctx)
    {
        var store = ctx.GetStore<FactoryStore>();
        store.AddGold(_productValue);
        // 可跨 Store 操作
        // var otherStore = ctx.GetStore<OtherStore>();
        return MutationResult.Success();
    }
}

// 调用方（System / View / Procedure 都可以）
this.Mutate(new SellProductMutation(150));
```

### System（帧逻辑 + 命令入口）

```csharp
public class ConveyorSystem : GameSystemBase, IUpdatableSystem
{
    private FactoryStore _factoryStore;

    protected override void OnInitialize()
    {
        // 用 internal GetStore 获取（基类 protected 暴露）
        // 通过 ArchExtensions: this.Query<IFactoryQueries>() 只读
    }

    public void OnUpdate(float deltaTime)
    {
        // 每帧物品流转逻辑（热路径，零 GC Alloc）
    }

    // 公开方法供 View 调用
    public void PlaceMachine(Vector2Int pos, MachineType type)
    {
        // 验证 → Mutate → 可选 Publish 事件
        this.Mutate(new PlaceMachineMutation(pos, type));
    }
}
```

### Procedure（异步长流程）

```csharp
public class UnlockNewEraProcedure : ProcedureBase
{
    private readonly EraTransitionView _transitionView;

    // View 引用通过构造函数注入，不走框架
    public UnlockNewEraProcedure(EraTransitionView transitionView)
    {
        _transitionView = transitionView;
    }

    public override async UniTask ExecuteAsync(CancellationToken ct)
    {
        // 1. 播放过渡动画（await View）
        await _transitionView.PlayEraTransition(ct);

        // 2. 改数据
        this.Mutate(new AdvanceEraMutation());

        // 3. 发事件通知 View 刷新
        this.Publish(new EraChangedEvent());

        // 4. 可嵌套子 Procedure
        await this.RunProcedure(new UnlockMachinesProcedure(), ct);
    }
}

// System 中触发
public void OnEraThresholdReached(EraTransitionView view)
{
    this.RunProcedure(new UnlockNewEraProcedure(view), _cts.Token).Forget();
}
```

### View（UI / 场景渲染）

```csharp
public class FactoryHudView : GameView
{
    [SerializeField] private Text _goldText;

    public override IGameContext GetArchitecture() => FactoryContext.Instance;

    protected override void OnViewEnable()
    {
        // 订阅事件刷新 UI
        this.Subscribe<GoldChangedEvent>(OnGoldChanged);
        
        // 初始刷新
        var q = this.Query<IFactoryQueries>();
        _goldText.text = q.Gold.ToString();
    }

    // OnViewDisable 时 base 自动 UnsubscribeAll

    private void OnGoldChanged(GoldChangedEvent evt)
    {
        var q = this.Query<IFactoryQueries>();
        _goldText.text = q.Gold.ToString();
    }
}
```

---

## 五、关键规则速查

| 规则 | 原因 |
|---|---|
| **System 不持有 View 引用** | 需要 await View 时拆 Procedure |
| **Mutation 必须同步** | 保证 Store 状态原子性 |
| **Procedure 每次 new** | 一次性实例，不复用 |
| **View 引用走构造函数注入到 Procedure** | 框架不维护 View 注册表 |
| **Store 是唯一数据源** | 需要被第二个类读的数据就放 Store |
| **热路径零 GC Alloc** | OnUpdate 里不 new List/string/lambda |
| **事件订阅在 OnViewEnable，退订靠基类 OnDisable** | GameView.OnDisable 自动 UnsubscribeAll |
| **异步必须传 CancellationToken** | 场景切换/游戏退出时能取消 |
| **Mutation 推荐 readonly struct** | 减少 GC，语义上强调不可变 |

---

## 六、扩展方法速查（ArchExtensions）

所有 `IArchNode`（Store / System / Procedure / View 的共同标记）都可以用：

```csharp
this.Query<IXxxQueries>()           // 只读查询 Store
this.GetSystem<XxxSystem>()         // 获取 System
this.Mutate(new XxxMutation())      // 执行 Mutation
this.Mutate<XxxStore>(s => s.Xxx()) // lambda Mutation（单 Store 简写）
this.Subscribe<XxxEvent>(handler)   // 订阅事件
this.Unsubscribe<XxxEvent>(handler) // 退订事件
this.Publish(new XxxEvent())        // 发布事件
this.RunProcedure(proc, ct)         // 运行 Procedure
```

---

## 七、项目结构约定（OpenClawGame01）

```
Scripts/
├── Stores/          # 所有 Store
├── Systems/         # 所有 System
├── Mutations/       # 所有 Mutation
├── Procedures/      # 所有 Procedure
├── Views/           # 所有 View（MonoBehaviour）
├── Events/          # 事件定义
├── Data/            # 纯数据类（Store 的 TData）
└── Context/         # GameContext 子类 + 注册逻辑
```
