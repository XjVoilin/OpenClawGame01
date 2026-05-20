# Plan 1: 基础框架与等距网格系统

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 清理灵药师残留代码，建立退休小院项目骨架，实现等距网格系统的核心数据结构和基础渲染。

**Architecture:** 使用 JulyArch Store-System-View 模式。GridStore 持有网格数据（格子状态、占用信息），GridSystem 处理坐标转换和格子操作逻辑，GridView 负责等距渲染和点击交互。新项目命名空间为 `CozyYard`。

**Tech Stack:** Unity 2022.3 LTS, URP 2D, JulyArch (Store-System-View), JulyCore (GF facade), Luban, HybridCLR, UniTask

---

### Task 1: 初始化 Git 仓库

**Files:**
- Create: `.gitignore`

- [ ] **Step 1: 创建 .gitignore**

```gitignore
# Unity
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]ser[Ss]ettings/
MemoryCaptures/
/[Aa]ssets/Plugins/Editor/JetBrains*

# IDE
.vs/
.vscode/
.idea/
*.csproj
*.sln
*.suo
*.tmp
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db

# OS
.DS_Store
Thumbs.db

# Bundles (generated)
Bundles/

# Luban generated
Assets/Game/Res/Configs/
Assets/Game/Scripts/Generated/
```

- [ ] **Step 2: 初始化 Git 仓库并提交**

Run:
```bash
git init
git add .gitignore
git commit -m "chore: init git repository with .gitignore"
```

- [ ] **Step 3: 提交当前项目作为基线**

Run:
```bash
git add -A
git commit -m "chore: baseline commit - spirit healer project state"
```

---

### Task 2: 清理灵药师专有模块

**Files:**
- Delete: `Assets/Game/Scripts/Modules/Diagnosis/` (entire folder)
- Delete: `Assets/Game/Scripts/Modules/Prescription/` (entire folder)
- Delete: `Assets/Game/Scripts/Modules/Encounter/` (entire folder)
- Delete: `Assets/Game/Scripts/Modules/Garden/` (entire folder)
- Delete: `Assets/Game/Scripts/Modules/GameLoop/` (entire folder)
- Delete: `Assets/Game/Scripts/Modules/Visitor/` (entire folder)
- Delete: `Assets/Game/Scripts/Modules/Time/` (entire folder)
- Delete: `Assets/Game/Scripts/Modules/Player/` (entire folder)
- Delete: `Assets/Game/Scripts/Modules/Inventory/` (entire folder)
- Delete: `Assets/Game/Scripts/Modules/Milestone/` (entire folder)
- Delete: `Assets/Game/Scripts/Views/Windows/` (entire folder)
- Delete: `Assets/Game/Scripts/Views/Windows/GameHUD/`
- Delete: `Assets/Game/Scripts/Views/Windows/VisitorWindow/`
- Delete: `Assets/Game/Scripts/Views/Windows/PrescriptionWindow/`
- Delete: `Assets/Game/Scripts/Views/Windows/TreatmentResultWindow/`
- Delete: `Tools/Luban/gen_spiritHealer_tables.py`
- Delete: `Tools/Luban/DataTables/Datas/` (Spirit Healer 数据)

- [ ] **Step 1: 删除灵药师业务模块**

Run:
```bash
rm -rf Assets/Game/Scripts/Modules/Diagnosis
rm -rf Assets/Game/Scripts/Modules/Prescription
rm -rf Assets/Game/Scripts/Modules/Encounter
rm -rf Assets/Game/Scripts/Modules/Garden
rm -rf Assets/Game/Scripts/Modules/GameLoop
rm -rf Assets/Game/Scripts/Modules/Visitor
rm -rf Assets/Game/Scripts/Modules/Time
rm -rf Assets/Game/Scripts/Modules/Player
rm -rf Assets/Game/Scripts/Modules/Inventory
rm -rf Assets/Game/Scripts/Modules/Milestone
```

- [ ] **Step 2: 删除灵药师 UI 和 Views**

Run:
```bash
rm -rf Assets/Game/Scripts/Views/Windows
```

- [ ] **Step 3: 删除灵药师 Luban 数据和生成脚本**

Run:
```bash
rm -f Tools/Luban/gen_spiritHealer_tables.py
```

- [ ] **Step 4: 提交清理结果**

Run:
```bash
git add -A
git commit -m "chore: remove spirit healer specific modules and views"
```

---

### Task 3: 命名空间重命名与 Shared 文件重置

**Files:**
- Modify: `Assets/Game/Scripts/Shared/Enums/Enums.cs`
- Modify: `Assets/Game/Scripts/Shared/Events/GameplayEvents.cs`
- Modify: `Assets/Game/Scripts/Shared/Events/TimeEvents.cs`
- Modify: `Assets/Game/Scripts/Shared/UIWindowId.cs`
- Modify: `Assets/Game/Scripts/Shared/SaveKeys.cs`
- Modify: `Assets/Game/Scripts/Shared/SavableStoreBase.cs`
- Modify: `Assets/Game/Scripts/Shared/Utils/Config.cs`
- Modify: `Assets/Game/Scripts/HotUpdateRegistrar.cs`
- Delete: `Assets/Game/Scripts/Views/Windows/GameUIView.cs`

- [ ] **Step 1: 重命名 SavableStoreBase.cs 命名空间**

将 `namespace SpiritHealer` 改为 `namespace CozyYard`：

```csharp
using Cysharp.Threading.Tasks;
using JulyArch;
using JulyCore;
using JulyCore.Data.Save;

namespace CozyYard
{
    public abstract class SavableStoreBase<TData> : StoreBase<TData>, IAsyncLoadable
        where TData : class, ISaveData, new()
    {
        protected abstract string SaveKey { get; }

        async UniTask IAsyncLoadable.OnLoadAsync()
        {
            Data = await GF.Save.LoadAndRegisterAsync<TData>(SaveKey);
        }

        protected void MarkDirty()
        {
            GF.Save.MarkDirty(SaveKey);
        }

        protected override void OnShutdown()
        {
            GF.Save.Unregister(SaveKey);
        }
    }
}
```

- [ ] **Step 2: 重写 Enums.cs**

```csharp
namespace CozyYard
{
    public enum Season
    {
        Spring = 0,
        Summer = 1,
        Autumn = 2,
        Winter = 3
    }

    public enum TimePhase
    {
        Dawn,
        Morning,
        Noon,
        Afternoon,
        Evening,
        Night
    }

    public enum CellState
    {
        Unexplored,
        Obstacle,
        Empty,
        Soil,
        Water,
        Paved
    }

    public enum BuildingCategory
    {
        House,
        Production,
        Livestock,
        Functional,
        Decoration
    }

    public enum AnimalType
    {
        Poultry,
        Aquatic,
        Pet
    }

    public enum ItemType
    {
        Material,
        Seed,
        Product,
        Tool
    }
}
```

- [ ] **Step 3: 重写 Events — TimeEvents.cs**

```csharp
namespace CozyYard
{
    public struct PhaseChangedEvent
    {
        public TimePhase OldPhase;
        public TimePhase NewPhase;
    }

    public struct DayChangedEvent
    {
        public int NewDay;
        public Season CurrentSeason;
    }

    public struct SeasonChangedEvent
    {
        public Season OldSeason;
        public Season NewSeason;
    }
}
```

- [ ] **Step 4: 重写 Events — GameplayEvents.cs**

```csharp
namespace CozyYard
{
    public struct InventoryChangedEvent { }

    public struct BuildingPlacedEvent
    {
        public int BuildingId;
        public int GridX;
        public int GridY;
    }

    public struct BuildingRemovedEvent
    {
        public int GridX;
        public int GridY;
    }

    public struct CropHarvestedEvent
    {
        public int CropId;
        public int Quantity;
    }

    public struct OrderCompletedEvent
    {
        public int OrderId;
    }

    public struct MilestoneAchievedEvent
    {
        public int MilestoneId;
    }

    public struct GridCellChangedEvent
    {
        public int GridX;
        public int GridY;
        public CellState NewState;
    }
}
```

- [ ] **Step 5: 重写 UIWindowId.cs**

```csharp
namespace CozyYard
{
    public static class UIWindowId
    {
        public const int GameHUD = 1001;
        public const int InventoryWindow = 1002;
        public const int BuildWindow = 1003;
        public const int CraftWindow = 1004;
        public const int VisitorWindow = 1005;
        public const int MilestoneWindow = 1006;
        public const int RecipeBookWindow = 1007;
        public const int PhoneWindow = 1008;
        public const int ShopWindow = 1009;
        public const int SettingsWindow = 1010;
    }
}
```

- [ ] **Step 6: 重写 SaveKeys.cs**

```csharp
namespace CozyYard
{
    public static class SaveKeys
    {
        public const string GridData = "Save_GridData";
        public const string TimeData = "Save_TimeData";
        public const string FarmData = "Save_FarmData";
        public const string BuildData = "Save_BuildData";
        public const string AnimalData = "Save_AnimalData";
        public const string CraftData = "Save_CraftData";
        public const string VisitorData = "Save_VisitorData";
        public const string InventoryData = "Save_InventoryData";
        public const string MilestoneData = "Save_MilestoneData";
        public const string ExpansionData = "Save_ExpansionData";
    }
}
```

- [ ] **Step 7: 重写 Config.cs（CfgTable 便捷入口，暂时为空壳）**

```csharp
namespace CozyYard
{
    public static class CfgTable
    {
        // 配表入口在 Luban 生成后填充
    }
}
```

- [ ] **Step 8: 删除 GameUIView.cs（后续重新创建）**

Run:
```bash
rm -f Assets/Game/Scripts/Views/Windows/GameUIView.cs
rm -f Assets/Game/Scripts/Views/Windows/GameUIView.cs.meta
```

- [ ] **Step 9: 提交命名空间重命名和 Shared 重置**

Run:
```bash
git add -A
git commit -m "refactor: rename namespace to CozyYard, reset shared files for new project"
```

---

### Task 4: 创建新模块目录结构

**Files:**
- Create: `Assets/Game/Scripts/Modules/Grid/` (folder)
- Create: `Assets/Game/Scripts/Modules/Time/` (folder)
- Create: `Assets/Game/Scripts/Modules/Farm/` (folder)
- Create: `Assets/Game/Scripts/Modules/Build/` (folder)
- Create: `Assets/Game/Scripts/Modules/Animal/` (folder)
- Create: `Assets/Game/Scripts/Modules/Craft/` (folder)
- Create: `Assets/Game/Scripts/Modules/Visitor/` (folder)
- Create: `Assets/Game/Scripts/Modules/Inventory/` (folder)
- Create: `Assets/Game/Scripts/Modules/Milestone/` (folder)
- Create: `Assets/Game/Scripts/Modules/Expansion/` (folder)
- Create: `Assets/Game/Scripts/Views/Windows/` (folder)

- [ ] **Step 1: 创建模块目录**

Run:
```bash
mkdir -p Assets/Game/Scripts/Modules/Grid
mkdir -p Assets/Game/Scripts/Modules/Time
mkdir -p Assets/Game/Scripts/Modules/Farm
mkdir -p Assets/Game/Scripts/Modules/Build
mkdir -p Assets/Game/Scripts/Modules/Animal
mkdir -p Assets/Game/Scripts/Modules/Craft
mkdir -p Assets/Game/Scripts/Modules/Visitor
mkdir -p Assets/Game/Scripts/Modules/Inventory
mkdir -p Assets/Game/Scripts/Modules/Milestone
mkdir -p Assets/Game/Scripts/Modules/Expansion
mkdir -p Assets/Game/Scripts/Views/Windows
```

注意：空目录不会被 Git 跟踪，后续有文件加入时自动纳入版本管理。

---

### Task 5: 实现 Grid 数据层（GridData + GridStore）

**Files:**
- Create: `Assets/Game/Scripts/Modules/Grid/GridData.cs`
- Create: `Assets/Game/Scripts/Modules/Grid/GridStore.cs`
- Create: `Assets/Game/Scripts/Modules/Grid/IGridQueries.cs`

- [ ] **Step 1: 创建 GridData.cs**

```csharp
using System;
using System.Collections.Generic;
using JulyCore.Data.Save;

namespace CozyYard
{
    [Serializable]
    public class GridCellData
    {
        public int X;
        public int Y;
        public CellState State = CellState.Unexplored;
        public int OccupantId;
        public int ObstacleId;
    }

    [Serializable]
    public class GridData : ISaveData
    {
        public int Width = 12;
        public int Height = 12;
        public List<GridCellData> Cells = new();

        public SaveImportance Importance => SaveImportance.Normal;
    }
}
```

- [ ] **Step 2: 创建 IGridQueries.cs**

```csharp
using JulyArch;

namespace CozyYard
{
    public interface IGridQueries : IStoreQueries
    {
        int Width { get; }
        int Height { get; }
        GridCellData GetCell(int x, int y);
        bool IsInBounds(int x, int y);
        bool IsCellEmpty(int x, int y);
        bool IsCellBuildable(int x, int y);
        bool CanPlaceAt(int x, int y, int sizeX, int sizeY);
    }
}
```

- [ ] **Step 3: 创建 GridStore.cs**

```csharp
using System;

namespace CozyYard
{
    public class GridStore : SavableStoreBase<GridData>, IGridQueries
    {
        protected override string SaveKey => SaveKeys.GridData;

        public int Width => Data.Width;
        public int Height => Data.Height;

        public GridCellData GetCell(int x, int y)
        {
            if (!IsInBounds(x, y)) return null;
            return Data.Cells[y * Data.Width + x];
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Data.Width && y >= 0 && y < Data.Height;
        }

        public bool IsCellEmpty(int x, int y)
        {
            var cell = GetCell(x, y);
            return cell != null && cell.State == CellState.Empty && cell.OccupantId == 0;
        }

        public bool IsCellBuildable(int x, int y)
        {
            var cell = GetCell(x, y);
            if (cell == null) return false;
            return (cell.State == CellState.Empty || cell.State == CellState.Paved)
                   && cell.OccupantId == 0;
        }

        public bool CanPlaceAt(int x, int y, int sizeX, int sizeY)
        {
            for (int dx = 0; dx < sizeX; dx++)
            {
                for (int dy = 0; dy < sizeY; dy++)
                {
                    if (!IsCellBuildable(x + dx, y + dy)) return false;
                }
            }
            return true;
        }

        public void SetCellState(int x, int y, CellState state)
        {
            var cell = GetCell(x, y);
            if (cell == null) return;
            cell.State = state;
            MarkDirty();
        }

        public void SetOccupant(int x, int y, int occupantId)
        {
            var cell = GetCell(x, y);
            if (cell == null) return;
            cell.OccupantId = occupantId;
            MarkDirty();
        }

        public void ClearOccupant(int x, int y)
        {
            SetOccupant(x, y, 0);
        }

        public void InitializeGrid(int width, int height)
        {
            Data.Width = width;
            Data.Height = height;
            Data.Cells.Clear();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Data.Cells.Add(new GridCellData { X = x, Y = y, State = CellState.Unexplored });
                }
            }
            MarkDirty();
        }
    }
}
```

- [ ] **Step 4: 提交 Grid 数据层**

Run:
```bash
git add -A
git commit -m "feat(grid): add GridData, GridStore, IGridQueries"
```

---

### Task 6: 实现 GridSystem（逻辑层）

**Files:**
- Create: `Assets/Game/Scripts/Modules/Grid/GridSystem.cs`
- Create: `Assets/Game/Scripts/Modules/Grid/IsometricUtils.cs`

- [ ] **Step 1: 创建 IsometricUtils.cs（坐标转换工具）**

```csharp
using UnityEngine;

namespace CozyYard
{
    public static class IsometricUtils
    {
        public const float TileWidth = 1f;
        public const float TileHeight = 0.5f;

        public static Vector2 GridToWorld(int gridX, int gridY)
        {
            float worldX = (gridX - gridY) * TileWidth * 0.5f;
            float worldY = (gridX + gridY) * TileHeight * 0.5f;
            return new Vector2(worldX, -worldY);
        }

        public static Vector2Int WorldToGrid(Vector2 worldPos)
        {
            float invX = worldPos.x / (TileWidth * 0.5f);
            float invY = -worldPos.y / (TileHeight * 0.5f);

            float gridX = (invX + invY) * 0.5f;
            float gridY = (invY - invX) * 0.5f;

            return new Vector2Int(Mathf.RoundToInt(gridX), Mathf.RoundToInt(gridY));
        }

        public static int GetSortingOrder(int gridX, int gridY, int heightOffset = 0)
        {
            return -(gridX + gridY) * 10 - heightOffset;
        }
    }
}
```

- [ ] **Step 2: 创建 GridSystem.cs**

```csharp
using JulyArch;
using UnityEngine;

namespace CozyYard
{
    public class GridSystem : GameSystemBase
    {
        private GridStore _store;

        public int Width => _store.Width;
        public int Height => _store.Height;

        protected override void OnInitialize()
        {
            _store = GetStore<GridStore>();
        }

        public void InitializeNewGrid(int width, int height)
        {
            _store.InitializeGrid(width, height);
            GenerateObstacles();
        }

        public GridCellData GetCell(int x, int y) => _store.GetCell(x, y);
        public bool IsInBounds(int x, int y) => _store.IsInBounds(x, y);
        public bool CanPlaceAt(int x, int y, int sizeX, int sizeY) => _store.CanPlaceAt(x, y, sizeX, sizeY);

        public bool ClearObstacle(int x, int y)
        {
            var cell = _store.GetCell(x, y);
            if (cell == null || cell.State != CellState.Obstacle) return false;

            cell.ObstacleId = 0;
            _store.SetCellState(x, y, CellState.Empty);
            Publish(new GridCellChangedEvent { GridX = x, GridY = y, NewState = CellState.Empty });
            return true;
        }

        public bool TillSoil(int x, int y)
        {
            var cell = _store.GetCell(x, y);
            if (cell == null || cell.State != CellState.Empty) return false;

            _store.SetCellState(x, y, CellState.Soil);
            Publish(new GridCellChangedEvent { GridX = x, GridY = y, NewState = CellState.Soil });
            return true;
        }

        public bool PlaceOccupant(int x, int y, int sizeX, int sizeY, int occupantId)
        {
            if (!_store.CanPlaceAt(x, y, sizeX, sizeY)) return false;

            for (int dx = 0; dx < sizeX; dx++)
            {
                for (int dy = 0; dy < sizeY; dy++)
                {
                    _store.SetOccupant(x + dx, y + dy, occupantId);
                }
            }
            return true;
        }

        public void RemoveOccupant(int x, int y, int sizeX, int sizeY)
        {
            for (int dx = 0; dx < sizeX; dx++)
            {
                for (int dy = 0; dy < sizeY; dy++)
                {
                    _store.ClearOccupant(x + dx, y + dy);
                }
            }
        }

        public Vector2 GridToWorldPosition(int x, int y)
        {
            return IsometricUtils.GridToWorld(x, y);
        }

        public Vector2Int WorldToGridPosition(Vector2 worldPos)
        {
            return IsometricUtils.WorldToGrid(worldPos);
        }

        private void GenerateObstacles()
        {
            var random = new System.Random(42);
            int totalCells = _store.Width * _store.Height;
            int obstacleCount = Mathf.RoundToInt(totalCells * 0.4f);

            int placed = 0;
            while (placed < obstacleCount)
            {
                int x = random.Next(0, _store.Width);
                int y = random.Next(0, _store.Height);
                var cell = _store.GetCell(x, y);
                if (cell.State == CellState.Unexplored)
                {
                    cell.State = CellState.Obstacle;
                    cell.ObstacleId = random.Next(1, 4);
                    placed++;
                }
            }

            // 起始位置 (中心附近 3x3) 保持为空地
            int cx = _store.Width / 2;
            int cy = _store.Height / 2;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = cx + dx;
                    int ny = cy + dy;
                    if (_store.IsInBounds(nx, ny))
                    {
                        _store.SetCellState(nx, ny, CellState.Empty);
                        _store.GetCell(nx, ny).ObstacleId = 0;
                    }
                }
            }
        }
    }
}
```

- [ ] **Step 3: 提交 GridSystem**

Run:
```bash
git add -A
git commit -m "feat(grid): add GridSystem with isometric utils, obstacle generation"
```

---

### Task 7: 更新 HotUpdateRegistrar

**Files:**
- Modify: `Assets/Game/Scripts/HotUpdateRegistrar.cs`

- [ ] **Step 1: 重写 HotUpdateRegistrar.cs**

```csharp
using Cysharp.Threading.Tasks;
using CozyYard.Aot;
using JulyArch;
using JulyCore;
using JulyCore.Provider.Config;
using JulyCore.Provider.Localization;
using JulyCore.Provider.Resource;
using JulyCore.Provider.Save;
using JulyCore.Provider.UI;
using JulyCore.Provider.Audio;
using JulyCore.Provider.Pool;
#if JULYGF_DEBUG
using JulyCore.Provider.GM;
#endif

namespace CozyYard
{
    public class HotUpdateRegistrar : IHotUpdateRegistrar, IAppArch
    {
        public IGameContext GetArchitecture() => AppArch.Context;

        public void Register(GameContext ctx)
        {
            RegisterProviders();
            RegisterStores(ctx);
            RegisterSystems(ctx);
        }

        private void RegisterProviders()
        {
            var resourceProvider = GF.Resolve<IResourceProvider>();
            var poolProvider = GF.Resolve<IPoolProvider>();

            var saveProvider = new PlayerPrefsSaveProvider();
            GF.RegisterProvider<ISaveProvider>(saveProvider);

            var configProvider = new LubanConfigProvider(resourceProvider);
            GF.RegisterProvider<IConfigProvider>(configProvider);

            GF.RegisterProvider<ILocalizationProvider>(new LubanLocalizationProvider(configProvider));
            GF.RegisterProvider<IUIProvider>(new UIProvider(resourceProvider, poolProvider));
            GF.RegisterProvider<IAudioProvider>(new UnityAudioProvider(resourceProvider, poolProvider));

#if JULYGF_DEBUG
            RegisterGMCommands();
#endif
        }

#if JULYGF_DEBUG
        private static void RegisterGMCommands()
        {
        }
#endif

        private void RegisterStores(GameContext ctx)
        {
            ctx.RegisterStore(new GridStore());
        }

        private void RegisterSystems(GameContext ctx)
        {
            ctx.RegisterSystem(new GridSystem());
        }

        public async UniTask OnGameLaunch()
        {
            ConfigureUI();

            await GF.Scene.SwitchAsync("Main");

            var gridSystem = AppArch.Context.GetSystem<GridSystem>();
            if (gridSystem.Width == 0)
            {
                gridSystem.InitializeNewGrid(12, 12);
            }

            GF.UI.Open(UIWindowId.GameHUD);
        }

        private static void ConfigureUI()
        {
            GF.UI.SetWindowConfig(new LubanUIWindowConfigProvider());
        }
    }
}
```

- [ ] **Step 2: 确认 AOT 命名空间引用**

检查 `Assets/Game/ScriptsAot/` 下是否有对 `SpiritHealer` 命名空间的引用，如果有需要改为 `CozyYard`。搜索所有 `.cs` 文件中的 `SpiritHealer` 并替换为 `CozyYard`。

Run:
```bash
grep -r "SpiritHealer" Assets/Game/ScriptsAot/ --include="*.cs" -l
```

对找到的每个文件，将 `namespace SpiritHealer` 和 `using SpiritHealer` 替换为 `namespace CozyYard` 和 `using CozyYard`。注意 AOT 下的子命名空间如 `SpiritHealer.Aot` 应改为 `CozyYard.Aot`。

- [ ] **Step 3: 提交 HotUpdateRegistrar 更新**

Run:
```bash
git add -A
git commit -m "feat: rewrite HotUpdateRegistrar for CozyYard with GridStore/GridSystem"
```

---

### Task 8: 创建 Luban 配表生成脚本

**Files:**
- Create: `Tools/Luban/gen_cozyyard_tables.py`

- [ ] **Step 1: 创建 gen_cozyyard_tables.py**

```python
"""
生成 CozyYard 退休小院 Luban 配置表 Excel 文件
运行: python gen_cozyyard_tables.py
"""
import os
from openpyxl import Workbook
from openpyxl.styles import Font, PatternFill, Alignment, Border, Side

DATAS_DIR = os.path.join(os.path.dirname(__file__), "DataTables", "Datas", "CozyYard")

HEADER_FONT = Font(bold=True, size=11)
META_FILL = PatternFill(start_color="D9E1F2", end_color="D9E1F2", fill_type="solid")
COMMENT_FILL = PatternFill(start_color="E2EFDA", end_color="E2EFDA", fill_type="solid")
THIN_BORDER = Border(
    left=Side(style="thin"),
    right=Side(style="thin"),
    top=Side(style="thin"),
    bottom=Side(style="thin"),
)


def style_meta_rows(ws, num_cols, num_meta_rows=3):
    for row_idx in range(1, num_meta_rows + 1):
        fill = META_FILL if row_idx <= 2 else COMMENT_FILL
        for col_idx in range(1, num_cols + 1):
            cell = ws.cell(row=row_idx, column=col_idx)
            cell.font = HEADER_FONT
            cell.fill = fill
            cell.border = THIN_BORDER
            cell.alignment = Alignment(horizontal="center")


def auto_width(ws):
    for col in ws.columns:
        max_len = 0
        col_letter = col[0].column_letter
        for cell in col:
            val = str(cell.value) if cell.value is not None else ""
            max_len = max(max_len, len(val.encode("utf-8")))
        ws.column_dimensions[col_letter].width = min(max_len + 4, 30)


def write_sheet(ws, headers, comments, rows):
    ws.append(["##var"] + headers[1:])
    ws.append(["##"] + comments[1:])
    for row in rows:
        ws.append(row)
    style_meta_rows(ws, len(headers), num_meta_rows=2)
    auto_width(ws)


def create_tables_xlsx():
    """__tables__.xlsx - Luban 表定义"""
    wb = Workbook()
    ws = wb.active
    ws.title = "tables"

    cols = ["##var", "full_name", "value_type", "read_schema_from_file", "input", "mode", "index", "group", "comment", "output", "tags"]
    ws.append(cols)

    tables = [
        ["", "TbItem",       "Item",       "false", "item.xlsx",       "map", "id", "", "物品总表",     "", ""],
        ["", "TbCrop",       "Crop",       "false", "crop.xlsx",       "map", "id", "", "作物配置表",   "", ""],
        ["", "TbTree",       "Tree",       "false", "tree.xlsx",       "map", "id", "", "树木配置表",   "", ""],
        ["", "TbAnimal",     "Animal",     "false", "animal.xlsx",     "map", "id", "", "动物配置表",   "", ""],
        ["", "TbBuilding",   "Building",   "false", "building.xlsx",   "map", "id", "", "建筑配置表",   "", ""],
        ["", "TbRecipe",     "Recipe",     "false", "recipe.xlsx",     "map", "id", "", "制作配方表",   "", ""],
        ["", "TbVisitor",    "Visitor",    "false", "visitor.xlsx",    "map", "id", "", "来客配置表",   "", ""],
        ["", "TbOrder",      "Order",      "false", "order.xlsx",      "map", "id", "", "订单模板表",   "", ""],
        ["", "TbMilestone",  "Milestone",  "false", "milestone.xlsx",  "map", "id", "", "里程碑表",     "", ""],
        ["", "TbSeason",     "Season",     "false", "season.xlsx",     "map", "id", "", "季节表",       "", ""],
        ["", "TbTime",       "TimeCfg",    "false", "time.xlsx",       "map", "id", "", "时间配置表",   "", ""],
        ["", "TbExpansion",  "Expansion",  "false", "expansion.xlsx",  "map", "id", "", "扩建区域表",   "", ""],
        ["", "TbObstacle",   "Obstacle",   "false", "obstacle.xlsx",   "map", "id", "", "障碍物表",     "", ""],
        ["", "TbShop",       "ShopItem",   "false", "shop.xlsx",       "map", "id", "", "商店商品表",   "", ""],
        ["", "TbUIWindow",   "UIWindow",   "false", "uiwindow.xlsx",   "map", "id", "", "UI窗口表",     "", ""],
        ["", "TbLanguage",   "Language",   "false", "language.xlsx",   "map", "key", "", "多语言表",    "", ""],
    ]

    for t in tables:
        ws.append(t)

    for col_idx in range(1, len(cols) + 1):
        cell = ws.cell(row=1, column=col_idx)
        cell.font = HEADER_FONT
        cell.fill = META_FILL
        cell.border = THIN_BORDER
    auto_width(ws)

    path = os.path.join(DATAS_DIR, "__tables__.xlsx")
    wb.save(path)
    print(f"  -> {path}")


def create_obstacle_xlsx():
    wb = Workbook()
    ws = wb.active
    ws.title = "obstacle"

    headers  = ["##var", "id", "name",   "clearTime", "dropItemId", "dropQuantity"]
    comments = ["##",    "ID", "名称",    "清除耗时(分钟)", "掉落物品ID", "掉落数量"]

    rows = [
        ["", 1, "杂草",  15, 1001, 2],
        ["", 2, "石头",  30, 1002, 3],
        ["", 3, "树桩",  60, 1003, 5],
    ]

    write_sheet(ws, headers, comments, rows)
    path = os.path.join(DATAS_DIR, "obstacle.xlsx")
    wb.save(path)
    print(f"  -> {path}")


def create_item_xlsx():
    wb = Workbook()
    ws = wb.active
    ws.title = "item"

    headers  = ["##var", "id",   "name",    "type",  "stackLimit", "desc"]
    comments = ["##",    "ID",   "名称",     "类型",   "堆叠上限",    "描述"]

    rows = [
        ["", 1001, "杂草纤维", "Material", 99, "清除杂草获得"],
        ["", 1002, "石头",     "Material", 99, "清除石块获得"],
        ["", 1003, "木材",     "Material", 99, "清除树桩获得"],
    ]

    write_sheet(ws, headers, comments, rows)
    path = os.path.join(DATAS_DIR, "item.xlsx")
    wb.save(path)
    print(f"  -> {path}")


def create_uiwindow_xlsx():
    wb = Workbook()
    ws = wb.active
    ws.title = "uiwindow"

    headers  = ["##var", "id", "desc", "windowName", "isNeedBlackMask", "isClickBlankQuit", "enterAnimType", "exitAnimType", "isIgnoreSafeArea", "uiLayer"]
    comments = ["##",    "ID", "描述",  "窗口名称",    "需要黑色遮罩",     "点击空白关闭",       "进入动画",       "退出动画",       "忽略安全区域",      "UI层级"]

    rows = [
        ["", 1001, "游戏HUD",     "GameHUD",           False, False, 0, 0, True, 1],
        ["", 1002, "背包",         "InventoryWindow",   True,  True,  1, 1, False, 2],
        ["", 1003, "建造面板",     "BuildWindow",       True,  True,  1, 1, False, 2],
        ["", 1004, "制作界面",     "CraftWindow",       True,  True,  1, 1, False, 2],
        ["", 1005, "来客对话",     "VisitorWindow",     True,  True,  1, 1, False, 2],
        ["", 1006, "里程碑",       "MilestoneWindow",   True,  True,  1, 1, False, 2],
        ["", 1007, "配方本",       "RecipeBookWindow",  True,  True,  1, 1, False, 2],
        ["", 1008, "问妈",         "PhoneWindow",       True,  True,  1, 1, False, 2],
        ["", 1009, "货郎商店",     "ShopWindow",        True,  True,  1, 1, False, 2],
        ["", 1010, "设置",         "SettingsWindow",    True,  True,  1, 1, False, 2],
    ]

    write_sheet(ws, headers, comments, rows)
    path = os.path.join(DATAS_DIR, "uiwindow.xlsx")
    wb.save(path)
    print(f"  -> {path}")


def create_language_xlsx():
    wb = Workbook()
    ws = wb.active
    ws.title = "language"

    headers  = ["##var", "key", "cn"]
    comments = ["##",    "键名", "中文"]

    rows = [
        ["", "season_spring", "春"],
        ["", "season_summer", "夏"],
        ["", "season_autumn", "秋"],
        ["", "season_winter", "冬"],
        ["", "phase_dawn",    "清晨"],
        ["", "phase_morning", "上午"],
        ["", "phase_noon",    "正午"],
        ["", "phase_afternoon", "下午"],
        ["", "phase_evening", "傍晚"],
        ["", "phase_night",   "夜晚"],
    ]

    write_sheet(ws, headers, comments, rows)
    path = os.path.join(DATAS_DIR, "language.xlsx")
    wb.save(path)
    print(f"  -> {path}")


if __name__ == "__main__":
    os.makedirs(DATAS_DIR, exist_ok=True)
    print("Generating CozyYard Luban Excel files...")
    create_tables_xlsx()
    create_obstacle_xlsx()
    create_item_xlsx()
    create_uiwindow_xlsx()
    create_language_xlsx()
    print("Done!")
```

- [ ] **Step 2: 运行生成脚本**

Run:
```bash
cd Tools/Luban
python gen_cozyyard_tables.py
```

Expected: 在 `Tools/Luban/DataTables/Datas/CozyYard/` 下生成 `__tables__.xlsx`、`obstacle.xlsx`、`item.xlsx`、`uiwindow.xlsx`、`language.xlsx`。

- [ ] **Step 3: 提交 Luban 配表脚本和生成数据**

Run:
```bash
git add -A
git commit -m "feat(luban): add CozyYard table generation script with initial data"
```

---

### Task 9: 创建基础 GridView（等距渲染）

**Files:**
- Create: `Assets/Game/Scripts/Views/GridView.cs`

- [ ] **Step 1: 创建 GridView.cs**

```csharp
using JulyArch;
using UnityEngine;

namespace CozyYard
{
    public class GridView : GameView
    {
        [SerializeField] private Sprite _emptyTileSprite;
        [SerializeField] private Sprite _obstacleTileSprite;
        [SerializeField] private Sprite _soilTileSprite;
        [SerializeField] private Sprite _highlightSprite;
        [SerializeField] private Transform _tilesParent;

        private GridSystem _gridSystem;
        private SpriteRenderer[,] _tileRenderers;
        private GameObject _highlightObj;

        public override IGameContext GetArchitecture() => AppArch.Context;

        protected override void OnViewEnable()
        {
            _gridSystem = this.GetSystem<GridSystem>();
            this.Subscribe<GridCellChangedEvent>(OnCellChanged);
            RenderGrid();
            CreateHighlight();
        }

        private void Update()
        {
            UpdateHighlight();
        }

        private void RenderGrid()
        {
            int w = _gridSystem.Width;
            int h = _gridSystem.Height;
            _tileRenderers = new SpriteRenderer[w, h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var cell = _gridSystem.GetCell(x, y);
                    var worldPos = IsometricUtils.GridToWorld(x, y);

                    var go = new GameObject($"Tile_{x}_{y}");
                    go.transform.SetParent(_tilesParent != null ? _tilesParent : transform);
                    go.transform.localPosition = new Vector3(worldPos.x, worldPos.y, 0);

                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = GetSpriteForState(cell.State);
                    sr.sortingOrder = IsometricUtils.GetSortingOrder(x, y);

                    _tileRenderers[x, y] = sr;
                }
            }
        }

        private void CreateHighlight()
        {
            _highlightObj = new GameObject("Highlight");
            _highlightObj.transform.SetParent(transform);
            var sr = _highlightObj.AddComponent<SpriteRenderer>();
            sr.sprite = _highlightSprite;
            sr.sortingOrder = 9999;
            sr.color = new Color(1f, 1f, 1f, 0.5f);
            _highlightObj.SetActive(false);
        }

        private void UpdateHighlight()
        {
            var mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var gridPos = IsometricUtils.WorldToGrid(new Vector2(mouseWorld.x, mouseWorld.y));

            if (_gridSystem.IsInBounds(gridPos.x, gridPos.y))
            {
                _highlightObj.SetActive(true);
                var worldPos = IsometricUtils.GridToWorld(gridPos.x, gridPos.y);
                _highlightObj.transform.localPosition = new Vector3(worldPos.x, worldPos.y, 0);
            }
            else
            {
                _highlightObj.SetActive(false);
            }

            if (Input.GetMouseButtonDown(0) && _gridSystem.IsInBounds(gridPos.x, gridPos.y))
            {
                OnTileClicked(gridPos.x, gridPos.y);
            }
        }

        private void OnTileClicked(int x, int y)
        {
            var cell = _gridSystem.GetCell(x, y);
            if (cell.State == CellState.Obstacle)
            {
                _gridSystem.ClearObstacle(x, y);
            }
        }

        private void OnCellChanged(GridCellChangedEvent evt)
        {
            if (_tileRenderers != null && evt.GridX < _tileRenderers.GetLength(0) && evt.GridY < _tileRenderers.GetLength(1))
            {
                _tileRenderers[evt.GridX, evt.GridY].sprite = GetSpriteForState(evt.NewState);
            }
        }

        private Sprite GetSpriteForState(CellState state)
        {
            return state switch
            {
                CellState.Empty => _emptyTileSprite,
                CellState.Soil => _soilTileSprite ?? _emptyTileSprite,
                CellState.Obstacle => _obstacleTileSprite ?? _emptyTileSprite,
                CellState.Unexplored => _obstacleTileSprite ?? _emptyTileSprite,
                _ => _emptyTileSprite
            };
        }
    }
}
```

- [ ] **Step 2: 提交 GridView**

Run:
```bash
git add -A
git commit -m "feat(grid): add GridView with isometric rendering and click interaction"
```

---

### Task 10: 场景搭建与验证

**Files:**
- Modify: `Assets/Game/Scenes/Main.unity` (在 Unity 编辑器中操作)

- [ ] **Step 1: 在 Unity 编辑器中设置场景**

在 Main 场景中：
1. 创建空 GameObject 命名为 `GridView`
2. 挂载 `GridView.cs` 脚本
3. 创建临时的等距菱形 Sprite（可用 Unity 内置方形 Sprite 旋转45度缩放作为占位）
4. 将 Sprite 引用拖入 GridView 的 Inspector 字段
5. 确保 Camera 为 Orthographic 模式，Size 适当（约 8-10）

- [ ] **Step 2: 创建占位 Sprite 资源**

在 `Assets/Game/Arts/Tiles/` 创建文件夹，准备最少两个不同颜色的菱形 Sprite：
- 绿色菱形 = 空地
- 灰色菱形 = 障碍物

可通过代码生成 Texture2D 或在图片编辑器中制作 64×32 像素菱形图片。

- [ ] **Step 3: 运行游戏验证**

在 Unity 编辑器中 Play：
- Expected: 看到 12×12 的等距网格渲染出来
- Expected: 约 40% 格子显示为障碍物（灰色），中心 3×3 为空地（绿色）
- Expected: 鼠标移动时有高亮跟随
- Expected: 点击障碍物格子变为空地

- [ ] **Step 4: 提交最终验证通过的状态**

Run:
```bash
git add -A
git commit -m "feat(grid): scene setup with placeholder sprites, grid rendering verified"
```

---

## 计划完成标志

当以上 10 个 Task 全部完成后，项目应处于以下状态：
- Git 仓库已初始化，有清晰的提交历史
- 灵药师代码已完全清理
- 新命名空间 `CozyYard` 已建立
- 等距网格系统可运行（数据层 + 逻辑层 + 渲染层）
- 点击障碍物可清除
- Luban 配表框架已搭好（含 obstacle、item 初始数据）
- 模块目录结构已准备就绪，供后续计划填充

接下来进入 **Plan 2: 时间系统**。
