# Plan 4: 种植系统

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 实现完整的种植系统——翻土、播种、浇水、生长、收获、枯萎，与时间/背包/网格系统联动。

**Architecture:** FarmStore (SavableStoreBase) 持有所有已种作物的状态，FarmSystem 处理种植逻辑和每日生长推进（监听 DayChangedEvent），GridSystem 管理土地格子状态。Luban TbCrop 表驱动作物数值。

**Tech Stack:** Unity 2022.3, JulyArch (Store-System-View), JulyCore, Luban, UniTask

---

### Task 1: 添加 Luban 作物配置表

**Files:**
- Modify: `Tools/Luban/gen_cozyyard_tables.py`

- [ ] **Step 1: 添加 create_crop_xlsx 函数**

```python
def create_crop_xlsx():
    wb = Workbook()
    ws = wb.active
    ws.title = "crop"

    headers  = ["##var", "id", "name", "season", "growthDays", "harvestWindow", "seedItemId", "produceItemId", "produceQuantity"]
    comments = ["##",    "ID", "名称",  "适宜季节(0春1夏2秋3冬)", "生长天数", "收获窗口(天)", "种子物品ID", "产出物品ID", "产出数量"]

    rows = [
        ["", 1, "白菜", 2, 3, 4, 2001, 3001, 2],
        ["", 2, "萝卜", 2, 5, 4, 2002, 3002, 2],
        ["", 3, "糯米", 2, 7, 3, 2003, 3003, 3],
        ["", 4, "菊花", 2, 5, 5, 2004, 3004, 2],
        ["", 5, "辣椒", 2, 5, 4, 2005, 3005, 3],
    ]

    write_sheet(ws, headers, comments, rows)
    path = os.path.join(DATAS_DIR, "crop.xlsx")
    wb.save(path)
    print(f"  -> {path}")
```

- [ ] **Step 2: 扩充 item.xlsx 加入种子和产出物品**

修改 `create_item_xlsx` 函数，添加种子和作物产出：

```python
rows = [
    ["", 1001, "杂草纤维", "Material", 99, "清除杂草获得"],
    ["", 1002, "石头",     "Material", 99, "清除石块获得"],
    ["", 1003, "木材",     "Material", 99, "清除树桩获得"],
    # 种子
    ["", 2001, "白菜种子", "Seed", 50, "种植白菜"],
    ["", 2002, "萝卜种子", "Seed", 50, "种植萝卜"],
    ["", 2003, "糯米种子", "Seed", 50, "种植糯米"],
    ["", 2004, "菊花种子", "Seed", 50, "种植菊花"],
    ["", 2005, "辣椒种子", "Seed", 50, "种植辣椒"],
    # 作物产出
    ["", 3001, "白菜",   "Product", 50, "新鲜白菜"],
    ["", 3002, "萝卜",   "Product", 50, "新鲜萝卜"],
    ["", 3003, "糯米",   "Product", 50, "饱满的糯米"],
    ["", 3004, "菊花",   "Product", 50, "新鲜菊花"],
    ["", 3005, "辣椒",   "Product", 50, "新鲜辣椒"],
]
```

- [ ] **Step 3: 在 main 中调用 create_crop_xlsx 并运行脚本**

- [ ] **Step 4: 提交**

```bash
git add -A
git commit -m "feat(luban): add crop config table and seed/produce items"
```

---

### Task 2: 实现 FarmStore

**Files:**
- Create: `Assets/Game/Scripts/Modules/Farm/FarmData.cs`
- Create: `Assets/Game/Scripts/Modules/Farm/IFarmQueries.cs`
- Create: `Assets/Game/Scripts/Modules/Farm/FarmStore.cs`

- [ ] **Step 1: 创建 FarmData.cs**

```csharp
using System;
using System.Collections.Generic;
using JulyCore.Data.Save;

namespace CozyYard
{
    public enum CropGrowthStage
    {
        Seed,
        Sprout,
        Growing,
        Mature,
        Withered
    }

    [Serializable]
    public class CropInstance
    {
        public int CropId;
        public int GridX;
        public int GridY;
        public CropGrowthStage Stage = CropGrowthStage.Seed;
        public int GrowthProgress;
        public int DaysSinceMature;
        public bool WateredToday;
    }

    [Serializable]
    public class FarmData : ISaveData
    {
        public List<CropInstance> Crops = new();

        public SaveImportance Importance => SaveImportance.Normal;
    }
}
```

- [ ] **Step 2: 创建 IFarmQueries.cs**

```csharp
using System.Collections.Generic;
using JulyArch;

namespace CozyYard
{
    public interface IFarmQueries : IStoreQueries
    {
        IReadOnlyList<CropInstance> Crops { get; }
        CropInstance GetCropAt(int x, int y);
        bool HasCropAt(int x, int y);
    }
}
```

- [ ] **Step 3: 创建 FarmStore.cs**

```csharp
using System.Collections.Generic;

namespace CozyYard
{
    public class FarmStore : SavableStoreBase<FarmData>, IFarmQueries
    {
        protected override string SaveKey => SaveKeys.FarmData;

        public IReadOnlyList<CropInstance> Crops => Data.Crops;

        public CropInstance GetCropAt(int x, int y)
        {
            for (int i = 0; i < Data.Crops.Count; i++)
            {
                if (Data.Crops[i].GridX == x && Data.Crops[i].GridY == y)
                    return Data.Crops[i];
            }
            return null;
        }

        public bool HasCropAt(int x, int y)
        {
            return GetCropAt(x, y) != null;
        }

        public void AddCrop(CropInstance crop)
        {
            Data.Crops.Add(crop);
            MarkDirty();
        }

        public void RemoveCrop(CropInstance crop)
        {
            Data.Crops.Remove(crop);
            MarkDirty();
        }

        public void MarkDirtyExplicit()
        {
            MarkDirty();
        }
    }
}
```

- [ ] **Step 4: 提交**

```bash
git add -A
git commit -m "feat(farm): add FarmData, FarmStore, IFarmQueries"
```

---

### Task 3: 实现 FarmSystem

**Files:**
- Create: `Assets/Game/Scripts/Modules/Farm/FarmSystem.cs`
- Create: `Assets/Game/Scripts/Modules/Farm/FarmEvents.cs`

- [ ] **Step 1: 创建 FarmEvents.cs**

```csharp
namespace CozyYard
{
    public struct CropPlantedEvent
    {
        public int GridX;
        public int GridY;
        public int CropId;
    }

    public struct CropGrowthEvent
    {
        public int GridX;
        public int GridY;
        public CropGrowthStage NewStage;
    }

    public struct CropWateredEvent
    {
        public int GridX;
        public int GridY;
    }

    public struct CropReadyEvent
    {
        public int GridX;
        public int GridY;
        public int CropId;
    }

    public struct CropWitheredEvent
    {
        public int GridX;
        public int GridY;
    }
}
```

- [ ] **Step 2: 创建 FarmSystem.cs**

```csharp
using JulyArch;

namespace CozyYard
{
    public class FarmSystem : GameSystemBase
    {
        private FarmStore _store;
        private GridSystem _gridSystem;
        private InventorySystem _inventorySystem;
        private TimeSystem _timeSystem;

        // 作物生长天数配置 (后续从Luban TbCrop读取)
        private static readonly int[] CropGrowthDays = { 0, 3, 5, 7, 5, 5 };
        private static readonly int[] CropHarvestWindow = { 0, 4, 4, 3, 5, 4 };
        private static readonly int[] CropProduceId = { 0, 3001, 3002, 3003, 3004, 3005 };
        private static readonly int[] CropProduceQty = { 0, 2, 2, 3, 2, 3 };

        protected override void OnInitialize()
        {
            _store = GetStore<FarmStore>();
            _gridSystem = GetSystem<GridSystem>();
            _inventorySystem = GetSystem<InventorySystem>();
            _timeSystem = GetSystem<TimeSystem>();

            this.Subscribe<DayChangedEvent>(OnDayChanged);
        }

        /// <summary>翻土：将空地变为土地。</summary>
        public bool TillSoil(int x, int y)
        {
            bool success = _gridSystem.TillSoil(x, y);
            if (success)
            {
                _timeSystem.ConsumeTime(10);
            }
            return success;
        }

        /// <summary>播种：消耗种子，在土地上种下作物。</summary>
        public bool PlantCrop(int x, int y, int cropId, int seedItemId)
        {
            var cell = _gridSystem.GetCell(x, y);
            if (cell == null || cell.State != CellState.Soil) return false;
            if (_store.HasCropAt(x, y)) return false;
            if (!_inventorySystem.HasItem(seedItemId)) return false;

            _inventorySystem.RemoveItem(seedItemId, 1);

            var crop = new CropInstance
            {
                CropId = cropId,
                GridX = x,
                GridY = y,
                Stage = CropGrowthStage.Seed,
                GrowthProgress = 0,
                DaysSinceMature = 0,
                WateredToday = false
            };

            _store.AddCrop(crop);
            _timeSystem.ConsumeTime(15);

            Publish(new CropPlantedEvent { GridX = x, GridY = y, CropId = cropId });
            return true;
        }

        /// <summary>浇水：标记今天已浇水，加速生长。</summary>
        public bool WaterCrop(int x, int y)
        {
            var crop = _store.GetCropAt(x, y);
            if (crop == null) return false;
            if (crop.WateredToday) return false;
            if (crop.Stage == CropGrowthStage.Mature || crop.Stage == CropGrowthStage.Withered) return false;

            crop.WateredToday = true;
            _store.MarkDirtyExplicit();
            _timeSystem.ConsumeTime(5);

            Publish(new CropWateredEvent { GridX = x, GridY = y });
            return true;
        }

        /// <summary>收获：获得产出物品，移除作物。</summary>
        public bool HarvestCrop(int x, int y)
        {
            var crop = _store.GetCropAt(x, y);
            if (crop == null || crop.Stage != CropGrowthStage.Mature) return false;

            int produceId = GetProduceId(crop.CropId);
            int produceQty = GetProduceQty(crop.CropId);

            if (!_inventorySystem.AddItem(produceId, produceQty)) return false;

            _store.RemoveCrop(crop);
            _gridSystem.GetCell(x, y).OccupantId = 0;
            _timeSystem.ConsumeTime(10);

            Publish(new CropHarvestedEvent { CropId = crop.CropId, Quantity = produceQty });
            return true;
        }

        /// <summary>移除枯萎作物。</summary>
        public void RemoveWithered(int x, int y)
        {
            var crop = _store.GetCropAt(x, y);
            if (crop == null || crop.Stage != CropGrowthStage.Withered) return;

            _store.RemoveCrop(crop);
            _gridSystem.GetCell(x, y).OccupantId = 0;
            _timeSystem.ConsumeTime(5);
        }

        private void OnDayChanged(DayChangedEvent e)
        {
            ProcessDailyGrowth();
        }

        private void ProcessDailyGrowth()
        {
            for (int i = _store.Crops.Count - 1; i >= 0; i--)
            {
                var crop = _store.Crops[i];

                if (crop.Stage == CropGrowthStage.Withered) continue;

                if (crop.Stage == CropGrowthStage.Mature)
                {
                    crop.DaysSinceMature++;
                    int harvestWindow = GetHarvestWindow(crop.CropId);
                    if (crop.DaysSinceMature > harvestWindow)
                    {
                        crop.Stage = CropGrowthStage.Withered;
                        Publish(new CropWitheredEvent { GridX = crop.GridX, GridY = crop.GridY });
                    }
                    continue;
                }

                // 生长推进
                int growthIncrement = crop.WateredToday ? 2 : 1;
                crop.GrowthProgress += growthIncrement;
                crop.WateredToday = false;

                int totalGrowthNeeded = GetGrowthDays(crop.CropId) * 2; // *2 因为浇水给2点
                float ratio = (float)crop.GrowthProgress / totalGrowthNeeded;

                CropGrowthStage newStage;
                if (ratio >= 1f)
                    newStage = CropGrowthStage.Mature;
                else if (ratio >= 0.6f)
                    newStage = CropGrowthStage.Growing;
                else if (ratio >= 0.2f)
                    newStage = CropGrowthStage.Sprout;
                else
                    newStage = CropGrowthStage.Seed;

                if (newStage != crop.Stage)
                {
                    crop.Stage = newStage;
                    Publish(new CropGrowthEvent { GridX = crop.GridX, GridY = crop.GridY, NewStage = newStage });

                    if (newStage == CropGrowthStage.Mature)
                    {
                        Publish(new CropReadyEvent { GridX = crop.GridX, GridY = crop.GridY, CropId = crop.CropId });
                    }
                }
            }

            _store.MarkDirtyExplicit();
        }

        private int GetGrowthDays(int cropId)
        {
            if (cropId >= 0 && cropId < CropGrowthDays.Length) return CropGrowthDays[cropId];
            return 5;
        }

        private int GetHarvestWindow(int cropId)
        {
            if (cropId >= 0 && cropId < CropHarvestWindow.Length) return CropHarvestWindow[cropId];
            return 4;
        }

        private int GetProduceId(int cropId)
        {
            if (cropId >= 0 && cropId < CropProduceId.Length) return CropProduceId[cropId];
            return 3001;
        }

        private int GetProduceQty(int cropId)
        {
            if (cropId >= 0 && cropId < CropProduceQty.Length) return CropProduceQty[cropId];
            return 1;
        }
    }
}
```

- [ ] **Step 3: 提交**

```bash
git add -A
git commit -m "feat(farm): add FarmSystem with plant/water/harvest/daily growth"
```

---

### Task 4: 注册 Farm 模块并连接 GridView

**Files:**
- Modify: `Assets/Game/Scripts/HotUpdateRegistrar.cs`
- Modify: `Assets/Game/Scripts/Views/GridView.cs`

- [ ] **Step 1: 在 HotUpdateRegistrar 中注册**

在 `RegisterStores` 中添加：
```csharp
ctx.RegisterStore(new FarmStore());
```

在 `RegisterSystems` 中添加：
```csharp
ctx.RegisterSystem(new FarmSystem());
```

- [ ] **Step 2: 修改 GridView 的 OnTileClicked 支持种植操作**

扩展 GridView 的点击逻辑，支持多种操作模式：

```csharp
private FarmSystem _farmSystem;

// 在 OnViewEnable 中获取
_farmSystem = this.GetSystem<FarmSystem>();

// 订阅种植相关事件
this.Subscribe<CropPlantedEvent>(OnCropPlanted);
this.Subscribe<CropGrowthEvent>(OnCropGrowth);
this.Subscribe<CropWateredEvent>(OnCropWatered);
this.Subscribe<CropWitheredEvent>(OnCropWithered);

private void OnTileClicked(int x, int y)
{
    var cell = _gridSystem.GetCell(x, y);
    switch (cell.State)
    {
        case CellState.Obstacle:
            _gridSystem.ClearObstacle(x, y);
            break;
        case CellState.Empty:
            _farmSystem.TillSoil(x, y);
            break;
        case CellState.Soil:
            var crop = _farmSystem.GetCropAt(x, y);
            if (crop == null)
            {
                // 暂时播种第一种作物(白菜)做测试，后续通过UI选择
                _farmSystem.PlantCrop(x, y, 1, 2001);
            }
            else if (crop.Stage == CropGrowthStage.Mature)
            {
                _farmSystem.HarvestCrop(x, y);
            }
            else if (!crop.WateredToday && crop.Stage != CropGrowthStage.Withered)
            {
                _farmSystem.WaterCrop(x, y);
            }
            else if (crop.Stage == CropGrowthStage.Withered)
            {
                _farmSystem.RemoveWithered(x, y);
            }
            break;
    }
}
```

注意：需要在 FarmSystem 中添加一个公开的 GetCropAt 方法供 View 查询：
```csharp
public CropInstance GetCropAt(int x, int y) => _store.GetCropAt(x, y);
```

- [ ] **Step 3: 更新 GridView 的 GetSpriteForState 添加 Soil 渲染**

确保 Soil 状态使用不同的 Sprite（已有 _soilTileSprite 字段）。

- [ ] **Step 4: 提交**

```bash
git add -A
git commit -m "feat(farm): register FarmSystem, integrate with GridView click logic"
```

---

## 计划完成标志

当以上 4 个 Task 全部完成后，种植系统应处于以下状态：
- Luban 有 crop.xlsx 配置（5种秋季作物）和对应的种子/产出物品
- FarmStore 持有所有作物实例和状态
- FarmSystem 支持：翻土、播种、浇水、收获、清除枯萎
- 每日结算时自动推进生长（浇水给2点，不浇给1点）
- 成熟后超过收获窗口自动枯萎
- 点击交互集成到 GridView（临时测试用）
- 所有操作消耗游戏时间

接下来进入 **Plan 5: 建造系统**。
