# Sprout Lands 全面视觉集成 实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 将 Sprout Lands 资源包中未使用的 ~320 个素材接入游戏，覆盖物品图标、世界装饰、建筑可视化三大方向，让游戏从"色块原型"升级为"像素画风格"。

**Architecture:** 通过 Luban 配置表驱动 sprite 映射（添加 `iconSprite` 字段），避免硬编码。UI 层通过异步加载 sprite 替换色块占位符。世界层扩充障碍物/装饰/树木 sprite 种类，建筑层用实际 sprite 替代彩色方块。

**Tech Stack:** Unity 2022 + URP 2D, C#, Luban (Excel→JSON→C#), YooAsset (按文件名加载), openpyxl (Python 生成 Excel)

---

## 文件结构总览

### 需修改的文件

| 文件 | 职责 |
|------|------|
| `Tools/Luban/gen_cozyyard_tables.py` | 为 Item/Building/Obstacle 表添加 `iconSprite` 字段 |
| `Assets/Game/Scripts/Views/GridView.cs` | 扩充障碍物/装饰/树木/建筑渲染 |
| `Assets/Game/Scripts/Views/Windows/InventoryWindow/InventoryWindow.cs` | 加载并显示物品图标 |
| `Assets/Game/Scripts/Views/Windows/ShopWindow/ShopWindow.cs` | 商店条目添加图标 |
| `Assets/Game/Scripts/Views/Windows/CraftWindow/CraftWindow.cs` | 配方条目添加图标 |
| `Assets/Game/Scripts/Views/Windows/BuildWindow/BuildWindow.cs` | 建筑条目添加图标 |
| `Assets/Game/Scripts/Editor/UIPrefabGenerator.cs` | ShopEntry/CraftEntry/BuildEntry 添加 Icon Image |

### 需新建的文件

| 文件 | 职责 |
|------|------|
| `Assets/Game/Scripts/Shared/SpriteLoader.cs` | 统一异步 sprite 加载工具（带缓存） |

### 需重新生成的文件（Luban 自动生成，不手动编辑）

| 文件 | 变更原因 |
|------|---------|
| `Assets/Game/Scripts/Generated/Configs/Item.cs` | 新增 `IconSprite` 字段 |
| `Assets/Game/Scripts/Generated/Configs/Building.cs` | 新增 `IconSprite` 字段 |
| `Assets/Game/Scripts/Generated/Configs/Obstacle.cs` | 新增 `IconSprite` 字段 |

---

## Phase A: 物品图标接入

> 120 个 `SL_Item_*` sprite 已提取但零使用。接入后背包、商店、配方界面从色块变为像素画图标。

### Task 1: Luban Item 表添加 iconSprite 字段

**Files:**
- Modify: `Tools/Luban/gen_cozyyard_tables.py` — `create_item_xlsx()` 函数

- [ ] **Step 1: 修改 create_item_xlsx() 添加 iconSprite 列**

在 `gen_cozyyard_tables.py` 的 `create_item_xlsx()` 函数中，headers/types/comments 各增加一列，并为每个物品行指定对应的 `SL_Item_*` sprite 名：

```python
def create_item_xlsx():
    wb = Workbook()
    ws = wb.active
    ws.title = "item"

    headers  = ["##var", "id",   "nameKey",  "type",  "stackLimit", "descKey",       "iconSprite"]
    types    = ["##type", "int", "string",  "string", "int",        "string",        "string"]
    comments = ["##",    "ID",   "名称key",  "类型",   "堆叠上限",    "描述key",        "图标sprite名"]

    rows = [
        # Materials (1001-1003): 杂草纤维/石头/木材
        ["", 1001, "item_1001", "Material", 99, "item_1001_desc", "SL_Item_c0_r0"],   # 杂草纤维
        ["", 1002, "item_1002", "Material", 99, "item_1002_desc", "SL_Item_c1_r0"],   # 石头
        ["", 1003, "item_1003", "Material", 99, "item_1003_desc", "SL_Item_c2_r0"],   # 木材
        # Seeds (2001-2005)
        ["", 2001, "item_2001", "Seed", 50, "item_2001_desc", "SL_Item_c0_r3"],       # 白菜种子
        ["", 2002, "item_2002", "Seed", 50, "item_2002_desc", "SL_Item_c1_r3"],       # 萝卜种子
        ["", 2003, "item_2003", "Seed", 50, "item_2003_desc", "SL_Item_c2_r3"],       # 糯米种子
        ["", 2004, "item_2004", "Seed", 50, "item_2004_desc", "SL_Item_c3_r3"],       # 菊花种子
        ["", 2005, "item_2005", "Seed", 50, "item_2005_desc", "SL_Item_c4_r3"],       # 辣椒种子
        # Fresh produce (3001-3005)
        ["", 3001, "item_3001", "Product", 50, "item_3001_desc", "SL_Item_c0_r6"],    # 白菜
        ["", 3002, "item_3002", "Product", 50, "item_3002_desc", "SL_Item_c1_r6"],    # 萝卜
        ["", 3003, "item_3003", "Product", 50, "item_3003_desc", "SL_Item_c2_r6"],    # 糯米
        ["", 3004, "item_3004", "Product", 50, "item_3004_desc", "SL_Item_c3_r6"],    # 菊花
        ["", 3005, "item_3005", "Product", 50, "item_3005_desc", "SL_Item_c4_r6"],    # 辣椒
        ["", 3101, "item_3101", "Product", 50, "item_3101_desc", "SL_Item_c5_r6"],    # 鸡蛋
        # Tree produce
        ["", 3006, "item_3006", "Product", 50, "item_3006_desc", "SL_Item_c6_r6"],    # 桂花
        ["", 3007, "item_3007", "Product", 50, "item_3007_desc", "SL_Item_c7_r6"],    # 柿子
        # Intermediate materials (4001-4004)
        ["", 4001, "item_4001", "Material", 50, "item_4001_desc", "SL_Item_c0_r9"],   # 桂花干
        ["", 4002, "item_4002", "Material", 50, "item_4002_desc", "SL_Item_c1_r9"],   # 糯米粉
        ["", 4003, "item_4003", "Material", 50, "item_4003_desc", "SL_Item_c2_r9"],   # 萝卜干
        ["", 4004, "item_4004", "Material", 50, "item_4004_desc", "SL_Item_c3_r9"],   # 菊花干
        # Final products (5001-5005)
        ["", 5001, "item_5001", "Product", 20, "item_5001_desc", "SL_Item_c0_r12"],   # 桂花糕
        ["", 5002, "item_5002", "Product", 20, "item_5002_desc", "SL_Item_c1_r12"],   # 辣炒蛋
        ["", 5003, "item_5003", "Product", 20, "item_5003_desc", "SL_Item_c2_r12"],   # 清炒白菜
        ["", 5004, "item_5004", "Product", 20, "item_5004_desc", "SL_Item_c3_r12"],   # 菊花茶
        ["", 5005, "item_5005", "Product", 20, "item_5005_desc", "SL_Item_c4_r12"],   # 柿饼
        # Junk
        ["", 9001, "item_9001", "Material", 10, "item_9001_desc", "SL_Item_c7_r14"],  # 黑暗料理
    ]

    write_sheet(ws, headers, types, comments, rows)
    path = os.path.join(DATAS_DIR, "物品_Item.xlsx")
    wb.save(path)
    print(f"  -> {path}")
```

- [ ] **Step 2: 运行 Python 脚本重新生成 Excel**

```bash
cd "Tools/Luban"
python gen_cozyyard_tables.py
```

Expected: `-> .../物品_Item.xlsx` 输出成功

- [ ] **Step 3: 运行 Luban 生成 C# + JSON**

```bash
cd "Tools/Luban"
# 使用项目现有的 Luban 生成命令
./gen.sh   # 或 gen.bat (Windows)
```

Expected: 成功生成，`Item.cs` 中新增 `IconSprite` 字段，`tbitem.json` 中每条记录新增 `"iconSprite": "SL_Item_c*_r*"`

- [ ] **Step 4: 验证生成的 Item.cs 包含 IconSprite**

打开 `Assets/Game/Scripts/Generated/Configs/Item.cs`，确认包含：

```csharp
public readonly string IconSprite;
```

---

### Task 2: 创建 SpriteLoader 工具类

**Files:**
- Create: `Assets/Game/Scripts/Shared/SpriteLoader.cs`

- [ ] **Step 1: 创建 SpriteLoader.cs**

```csharp
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JulyCore;
using UnityEngine;

namespace CozyYard
{
    public static class SpriteLoader
    {
        private static readonly Dictionary<string, Sprite> _cache = new();

        public static async UniTask<Sprite> LoadAsync(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName)) return null;
            if (_cache.TryGetValue(spriteName, out var cached)) return cached;

            try
            {
                var sprite = await GF.Resource.LoadAsync<Sprite>(spriteName);
                if (sprite != null) _cache[spriteName] = sprite;
                return sprite;
            }
            catch
            {
                return null;
            }
        }

        public static void ClearCache() => _cache.Clear();
    }
}
```

- [ ] **Step 2: 确认编译通过**

在 Unity 中检查无编译错误。

---

### Task 3: InventoryWindow 接入物品图标

**Files:**
- Modify: `Assets/Game/Scripts/Views/Windows/InventoryWindow/InventoryWindow.cs`

- [ ] **Step 1: 修改 Refresh() 方法，异步加载图标**

将 `Refresh()` 改为 `RefreshAsync()` 并加载 sprite：

在 `InventoryWindow.cs` 中做以下修改：

1) 将 `OnViewEnable` 中的 `Refresh()` 替换为 `RefreshAsync().Forget()`：

```csharp
// 旧代码
Refresh();
HideDetail();

// 新代码
HideDetail();
RefreshAsync().Forget();
```

2) 将 `OnInventoryChanged` 和 `OnCategoryChanged` 中的 `Refresh()` 替换为 `RefreshAsync().Forget()`

3) 将 `Refresh()` 方法改为：

```csharp
private async UniTaskVoid RefreshAsync()
{
    var store = GetStore<InventoryStore>();
    var itemTable = GF.Config.GetTable<TbItem>();

    if (_capacityText) _capacityText.text = $"{store.UsedSlots}/{store.Capacity}";
    if (_coinsText) _coinsText.text = store.Coins.ToString();

    var filteredItems = GetFilteredItems(store, itemTable);

    EnsureSlotCount(store.Capacity);

    for (int i = 0; i < _slotInstances.Count; i++)
    {
        var slot = _slotInstances[i];
        if (i < filteredItems.Count)
        {
            var stack = filteredItems[i];
            var cfg = itemTable?.GetOrDefault(stack.ItemId);
            Sprite icon = null;
            if (cfg != null && !string.IsNullOrEmpty(cfg.IconSprite))
                icon = await SpriteLoader.LoadAsync(cfg.IconSprite);
            slot.Setup(stack.ItemId, stack.Quantity, icon, icon != null ? Color.white : GetItemColor(cfg?.Type));
            slot.SetSelected(i == _selectedSlotIndex);
        }
        else
        {
            slot.SetEmpty();
        }
    }
}
```

4) 修改 `ShowDetail()` 加载详情图标：

```csharp
private async void ShowDetail(int itemId)
{
    var itemTable = GF.Config.GetTable<TbItem>();
    var cfg = itemTable?.GetOrDefault(itemId);
    if (cfg == null)
    {
        HideDetail();
        return;
    }

    if (_detailPanel) _detailPanel.SetActive(true);
    if (_detailIcon)
    {
        Sprite icon = null;
        if (!string.IsNullOrEmpty(cfg.IconSprite))
            icon = await SpriteLoader.LoadAsync(cfg.IconSprite);
        if (icon != null)
        {
            _detailIcon.sprite = icon;
            _detailIcon.color = Color.white;
        }
        else
        {
            _detailIcon.color = GetItemColor(cfg.Type);
        }
        _detailIcon.enabled = true;
    }
    if (_detailName) _detailName.text = GF.Localization.Get(cfg.NameKey);
    if (_detailDesc) _detailDesc.text = GF.Localization.Get(cfg.DescKey);
}
```

5) 需要在文件顶部添加 `using Cysharp.Threading.Tasks;`

- [ ] **Step 2: 确认编译通过并运行测试**

在 Unity 中 Play，打开背包，确认物品格子显示 sprite 而非色块。

---

### Task 4: ShopWindow 添加物品图标

**Files:**
- Modify: `Assets/Game/Scripts/Views/Windows/ShopWindow/ShopWindow.cs`
- Modify: `Assets/Game/Scripts/Views/Windows/ShopWindow/ShopEntry.cs` (如果在独立文件中)

- [ ] **Step 1: 给 ShopEntry 添加 icon 字段和异步设置方法**

在 `ShopEntry.cs` 中：

```csharp
// 添加字段
[SerializeField] private Image _icon;

// 修改 Setup 签名
public void Setup(string name, int price, bool canAfford, Action onBuy, Sprite icon)
{
    if (_nameText) _nameText.text = name;
    if (_priceText) _priceText.text = $"{price}";
    if (_icon)
    {
        if (icon != null)
        {
            _icon.sprite = icon;
            _icon.color = Color.white;
        }
        _icon.enabled = icon != null;
    }
    if (_buyBtn)
    {
        _buyBtn.SetInteractable(canAfford);
        _buyBtn.onClick.AddListener(() => onBuy?.Invoke());
    }
}
```

需要在文件顶部添加 `using UnityEngine.UI;`

- [ ] **Step 2: ShopWindow.Refresh() 改为异步并加载图标**

在 `ShopWindow.cs` 中，将 `Refresh()` 改为 `RefreshAsync()`：

```csharp
private async UniTaskVoid RefreshAsync()
{
    ClearEntries();

    var invStore = GetStore<InventoryStore>();
    if (_coinsText) _coinsText.text = invStore.Coins.ToString();

    if (_entryPrefab == null || _listContainer == null) return;

    var tbShop = GF.Config.GetTable<TbShop>();
    var tbItem = GF.Config.GetTable<TbItem>();
    if (tbShop == null) return;

    var shopSystem = GetSystem<ShopSystem>();
    int playerCoins = invStore.Coins;

    foreach (var shopItem in tbShop.DataList)
    {
        var entry = Object.Instantiate(_entryPrefab, _listContainer);
        entry.gameObject.SetActive(true);

        var itemCfg = tbItem?.GetOrDefault(shopItem.ItemId);
        string itemName = itemCfg != null ? GF.Localization.Get(itemCfg.NameKey) : $"#{shopItem.ItemId}";
        bool canAfford = playerCoins >= shopItem.Price;

        Sprite icon = null;
        if (itemCfg != null && !string.IsNullOrEmpty(itemCfg.IconSprite))
            icon = await SpriteLoader.LoadAsync(itemCfg.IconSprite);

        int shopId = shopItem.Id;
        entry.Setup(itemName, shopItem.Price, canAfford, () => shopSystem.TryPurchase(shopId), icon);
        _entries.Add(entry);
    }
}
```

所有调用 `Refresh()` 的地方改为 `RefreshAsync().Forget()`。

需要添加 `using Cysharp.Threading.Tasks;`

- [ ] **Step 3: 更新 UIPrefabGenerator 中 ShopEntry 预制体生成代码**

在 `UIPrefabGenerator.cs` 中找到 `ShopEntry` 生成逻辑，添加一个 `Image` 组件（32×32，位于条目左侧），并将其绑定到 `_icon` 字段。

具体修改取决于现有代码结构，在生成 ShopEntry 时添加：

```csharp
var iconGo = CreateChild(entryGo, "Icon");
var iconRt = iconGo.AddComponent<RectTransform>();
iconRt.sizeDelta = new Vector2(32, 32);
// 定位在条目左侧
var iconImg = iconGo.AddComponent<Image>();
iconImg.color = new Color(0.6f, 0.6f, 0.6f);
iconImg.enabled = false;
```

然后在 `ShopEntry` 组件绑定时将 `_icon` 字段指向这个 Image。

- [ ] **Step 4: 重新生成 UI 预制体**

在 Unity 菜单中执行 `CozyYard/生成所有 UI 预制体`，然后 Play 测试商店窗口显示图标。

---

### Task 5: CraftWindow 添加产出图标

**Files:**
- Modify: `Assets/Game/Scripts/Views/Windows/CraftWindow/CraftWindow.cs`
- Modify: `Assets/Game/Scripts/Views/Windows/CraftWindow/CraftEntry.cs`

- [ ] **Step 1: CraftEntry 添加 icon 字段**

与 ShopEntry 同理，添加 `[SerializeField] private Image _icon;`，修改 `Setup()` 签名接收 `Sprite icon` 参数。

```csharp
[SerializeField] private Image _icon;

public void Setup(string name, bool canCraft, Action onCraft, Sprite icon)
{
    if (_nameText) _nameText.text = name;
    if (_icon)
    {
        if (icon != null)
        {
            _icon.sprite = icon;
            _icon.color = Color.white;
        }
        _icon.enabled = icon != null;
    }
    if (_craftBtn)
    {
        _craftBtn.SetInteractable(canCraft);
        _craftBtn.onClick.AddListener(() => onCraft?.Invoke());
    }
}
```

- [ ] **Step 2: CraftWindow.Refresh() 改为异步**

```csharp
private async UniTaskVoid RefreshAsync()
{
    ClearEntries();
    if (_entryPrefab == null || _listContainer == null) return;

    var craftStore = GetStore<CraftStore>();
    var craftSystem = GetSystem<CraftSystem>();
    var tbItem = GF.Config.GetTable<TbItem>();

    foreach (int recipeId in craftStore.UnlockedRecipeIds)
    {
        var entry = Object.Instantiate(_entryPrefab, _listContainer);
        entry.gameObject.SetActive(true);

        var recipe = GF.Config.GetTable<TbRecipe>()?.GetOrDefault(recipeId);
        string nameKey = recipe?.NameKey ?? $"#{recipeId}";
        bool canCraft = craftSystem.CanCraft(recipeId);
        int id = recipeId;

        Sprite icon = null;
        if (recipe != null)
        {
            var outputItem = tbItem?.GetOrDefault(recipe.OutputItemId);
            if (outputItem != null && !string.IsNullOrEmpty(outputItem.IconSprite))
                icon = await SpriteLoader.LoadAsync(outputItem.IconSprite);
        }

        entry.Setup(
            GF.Localization.Get(nameKey),
            canCraft,
            () => OnCraft(craftSystem, id),
            icon
        );

        _entries.Add(entry);
    }
}
```

- [ ] **Step 3: 更新 UIPrefabGenerator 中 CraftEntry 预制体**

同 ShopEntry，添加 32×32 的 Icon Image，绑定到 `_icon`。

- [ ] **Step 4: 重新生成预制体并测试**

---

### Task 6: BuildWindow 添加建筑图标

**Files:**
- Modify: `Tools/Luban/gen_cozyyard_tables.py` — `create_building_xlsx()` 函数
- Modify: `Assets/Game/Scripts/Views/Windows/BuildWindow/BuildWindow.cs`
- Modify: `Assets/Game/Scripts/Views/Windows/BuildWindow/BuildEntry.cs`

- [ ] **Step 1: Building 表添加 iconSprite 字段**

在 `gen_cozyyard_tables.py` 的 `create_building_xlsx()` 中：

```python
def create_building_xlsx():
    wb = Workbook()
    ws = wb.active
    ws.title = "building"

    headers  = ["##var", "id", "nameKey", "category", "sizeX", "sizeY", "materials", "materialQtys", "buildTime", "prerequisiteId", "level", "iconSprite"]
    types    = ["##type", "int", "string", "string", "int", "int", "(list#sep=,),int", "(list#sep=,),int", "int", "int", "int", "string"]
    comments = ["##",    "ID", "名称key",  "类别",      "宽",    "高",    "材料ID列表", "材料数量列表",   "建造时间(分钟)", "前置建筑ID", "等级", "图标sprite名"]

    rows = [
        ["", 1,  "building_1",   "House",      2, 2, "1003",      "20",     120, 0,  1, "SL_Item_c0_r1"],
        ["", 2,  "building_2",   "House",      3, 3, "1003,1002", "30,20",  180, 1,  2, "SL_Item_c1_r1"],
        ["", 10, "building_10",  "Production", 1, 1, "1003,1002", "5,3",    30,  0,  1, "SL_Item_c2_r1"],
        ["", 11, "building_11",  "Production", 1, 1, "1002,1003", "10,8",   60,  1,  2, "SL_Item_c3_r1"],
        ["", 20, "building_20",  "Production", 1, 1, "1003",      "8",      30,  0,  1, "SL_Item_c4_r1"],
        ["", 30, "building_30",  "Production", 1, 1, "1002",      "15",     60,  0,  1, "SL_Item_c5_r1"],
        ["", 40, "building_40",  "Livestock",  2, 2, "1003",      "12",     45,  0,  1, "SL_Item_c6_r1"],
        ["", 50, "building_50",  "Decoration", 1, 1, "1003",      "3",      10,  0,  1, "SL_Item_c7_r1"],
        ["", 60, "building_60",  "Functional", 1, 1, "1003,1002", "5,3",    20,  0,  1, "SL_Item_c0_r2"],
        ["", 70, "building_70",  "Functional", 2, 2, "1003,1002", "15,10",  90,  0,  1, "SL_Item_c1_r2"],
    ]

    write_sheet(ws, headers, types, comments, rows)
    path = os.path.join(DATAS_DIR, "建筑_Building.xlsx")
    wb.save(path)
    print(f"  -> {path}")
```

- [ ] **Step 2: BuildEntry 添加 icon 字段**

同 ShopEntry/CraftEntry，添加 `_icon` Image 字段和修改 `Setup()` 签名。

- [ ] **Step 3: BuildWindow.Refresh() 改为异步加载图标**

```csharp
private async UniTaskVoid RefreshAsync()
{
    ClearEntries();
    if (_entryPrefab == null || _listContainer == null) return;

    var buildSystem = GetSystem<BuildSystem>();
    var tbBuilding = GF.Config.GetTable<TbBuilding>();
    if (tbBuilding == null) return;

    foreach (var (id, cfg) in tbBuilding.DataMap)
    {
        var entry = Object.Instantiate(_entryPrefab, _listContainer);
        entry.gameObject.SetActive(true);

        bool canAfford = buildSystem.CanAfford(id);
        int buildingId = id;
        string displayName = $"{GF.Localization.Get(cfg.NameKey)} ({cfg.SizeX}×{cfg.SizeY})";

        Sprite icon = null;
        if (!string.IsNullOrEmpty(cfg.IconSprite))
            icon = await SpriteLoader.LoadAsync(cfg.IconSprite);

        entry.Setup(displayName, canAfford, () => OnBuild(buildingId), icon);
        _entries.Add(entry);
    }
}
```

- [ ] **Step 4: 更新 UIPrefabGenerator 中 BuildEntry 预制体**

- [ ] **Step 5: 重新生成 Excel → Luban → 预制体，全面测试 UI 图标**

```bash
cd "Tools/Luban"
python gen_cozyyard_tables.py && ./gen.sh
```

然后 Unity 中执行 `CozyYard/生成所有 UI 预制体`，Play 测试所有窗口。

- [ ] **Step 6: 提交 Phase A**

```bash
git add -A
git commit -m "feat: 物品图标接入 UI - Item/Building 表添加 iconSprite，背包/商店/配方/建造窗口显示 SL_Item sprite"
```

---

## Phase B: 世界装饰丰富化

> 60 个装饰只用了 2 个，84 个树只用了 1 个。扩充后地图更有层次感。

### Task 7: Obstacle 表添加 iconSprite 并扩充种类

**Files:**
- Modify: `Tools/Luban/gen_cozyyard_tables.py` — `create_obstacle_xlsx()` 函数
- Modify: `Tools/Luban/gen_cozyyard_tables.py` — `create_gameconfig_xlsx()` 函数（更新 maxObstacleId）

- [ ] **Step 1: 扩充障碍物种类到 6 种**

在 `gen_cozyyard_tables.py` 的 `create_obstacle_xlsx()` 中：

```python
def create_obstacle_xlsx():
    wb = Workbook()
    ws = wb.active
    ws.title = "obstacle"

    headers  = ["##var", "id", "nameKey",   "clearTime", "dropItemId", "dropQuantity", "iconSprite"]
    types    = ["##type", "int", "string", "int",       "int",        "int",           "string"]
    comments = ["##",    "ID", "名称key",    "清除耗时(分钟)", "掉落物品ID", "掉落数量",     "世界sprite名"]

    rows = [
        ["", 1, "obstacle_1",  15, 1001, 2, "SL_Deco_c2_r0"],   # 杂草
        ["", 2, "obstacle_2",  30, 1002, 3, "SL_Deco_c6_r3"],   # 石头
        ["", 3, "obstacle_3",  60, 1003, 5, "SL_Tree_c4_r5"],   # 树桩
        ["", 4, "obstacle_4",  20, 1001, 1, "SL_Deco_c4_r0"],   # 蘑菇
        ["", 5, "obstacle_5",  25, 1002, 2, "SL_Deco_c8_r3"],   # 大石头
        ["", 6, "obstacle_6",  45, 1003, 4, "SL_Tree_c6_r5"],   # 大树桩
    ]

    write_sheet(ws, headers, types, comments, rows)
    path = os.path.join(DATAS_DIR, "障碍物_Obstacle.xlsx")
    wb.save(path)
    print(f"  -> {path}")
```

- [ ] **Step 2: 更新 GameConfig 中 maxObstacleId**

在 `create_gameconfig_xlsx()` 中，找到 `maxObstacleId` 行，将值从 `3` 改为 `6`：

```python
("maxObstacleId",     "int",             "最大障碍物ID",              6),
```

- [ ] **Step 3: 重新生成 Excel + Luban**

```bash
cd "Tools/Luban"
python gen_cozyyard_tables.py && ./gen.sh
```

---

### Task 8: GridView 从配置加载障碍物 sprite（去除硬编码）

**Files:**
- Modify: `Assets/Game/Scripts/Views/GridView.cs` — `LoadObstacleSprites()` 方法

- [ ] **Step 1: 用配置表驱动障碍物 sprite 加载**

替换 `LoadObstacleSprites()` 方法：

```csharp
private async UniTask LoadObstacleSprites()
{
    var tbObstacle = GF.Config.GetTable<TbObstacle>();
    if (tbObstacle == null) return;

    foreach (var obstacle in tbObstacle.DataList)
    {
        if (!string.IsNullOrEmpty(obstacle.IconSprite))
        {
            var s = await SpriteLoader.LoadAsync(obstacle.IconSprite);
            if (s != null) _obstacleSprites[obstacle.Id] = s;
        }
    }
}
```

- [ ] **Step 2: 确认编译通过并测试**

在 Unity Play 模式中确认新增的 3 种障碍物（蘑菇、大石头、大树桩）正确显示。

---

### Task 9: 添加世界装饰散布层

**Files:**
- Modify: `Assets/Game/Scripts/Views/GridView.cs`

- [ ] **Step 1: 添加装饰 sprite 列表和散布逻辑**

在 `GridView.cs` 的 `#region Sprite Assets` 中添加：

```csharp
private Sprite[] _decoSprites;
```

在 `LoadSpritesAsync()` 方法末尾添加装饰 sprite 加载：

```csharp
await LoadDecoSprites();
```

新增方法：

```csharp
private async UniTask LoadDecoSprites()
{
    var decoNames = new[]
    {
        "SL_Deco_c0_r0", "SL_Deco_c1_r0", "SL_Deco_c3_r0",
        "SL_Deco_c5_r0", "SL_Deco_c0_r1", "SL_Deco_c1_r1",
        "SL_Deco_c2_r1", "SL_Deco_c3_r1"
    };
    var loaded = new List<Sprite>();
    foreach (var name in decoNames)
    {
        var s = await SpriteLoader.LoadAsync(name);
        if (s != null) loaded.Add(s);
    }
    _decoSprites = loaded.ToArray();
}
```

- [ ] **Step 2: 在 Empty 草地上随机添加装饰覆盖层**

修改 `ApplyCellVisual()` 方法，在 `CellState.Empty` 分支中添加：

```csharp
case CellState.Empty:
    sr.sprite = PickGrassVariant(x, y);
    sr.color = Color.white;
    TryAddDecoOverlay(sr.gameObject, x, y);
    break;
```

新增方法：

```csharp
private void TryAddDecoOverlay(GameObject tileGo, int x, int y)
{
    if (_decoSprites == null || _decoSprites.Length == 0) return;

    int hash = x * 31 + y * 17;
    if (hash % 8 != 0) return;

    var existing = tileGo.transform.Find("DecoOverlay");
    if (existing != null) return;

    var decoSprite = _decoSprites[Mathf.Abs(hash / 8) % _decoSprites.Length];
    var overlayGo = new GameObject("DecoOverlay");
    overlayGo.transform.SetParent(tileGo.transform);
    overlayGo.transform.localPosition = Vector3.zero;
    var sr = overlayGo.AddComponent<SpriteRenderer>();
    sr.sprite = decoSprite;
    sr.sortingOrder = GridUtils.GetSortingOrder(x, y) + 1;
}
```

- [ ] **Step 3: OnCellChanged 中清除装饰覆盖层**

修改 `OnCellChanged()` 方法，在清除障碍物覆盖层的地方，同时清除装饰覆盖层：

```csharp
private void OnCellChanged(GridCellChangedEvent evt)
{
    if (_tileRenderers == null) return;
    int x = evt.GridX, y = evt.GridY;
    if (x < 0 || x >= _tileRenderers.GetLength(0) || y < 0 || y >= _tileRenderers.GetLength(1)) return;

    var sr = _tileRenderers[x, y];
    var cell = _gridSystem.GetCell(x, y);
    if (cell == null) return;

    var overlay = sr.transform.Find("ObstacleOverlay");
    if (overlay != null) Destroy(overlay.gameObject);
    var deco = sr.transform.Find("DecoOverlay");
    if (deco != null) Destroy(deco.gameObject);

    ApplyCellVisual(sr, cell, x, y);
}
```

- [ ] **Step 4: 确认编译通过并测试**

Play 模式下确认空地上偶尔出现小花、蘑菇等装饰。

- [ ] **Step 5: 提交 Phase B**

```bash
git add -A
git commit -m "feat: 世界装饰丰富化 - 障碍物扩充至6种(配置驱动)，空地散布装饰花草"
```

---

## Phase C: 建筑可视化

> 目前建筑是彩色方块+文字标签。用 Sprout Lands 的围栏、房屋部件替换。

### Task 10: 提取建筑 sprite

**Files:**
- 手动或脚本提取

- [ ] **Step 1: 用 Python 从 Building Parts sprite sheet 提取建筑 sprite**

编写并运行 Python 脚本，从 Sprout Lands 建筑部件中提取关键 sprite：

```python
from PIL import Image
import os

arts_dir = "Assets/Game/Arts/SproutLands/Sprout Lands - Sprites - premium pack"
out_dir = "Assets/Game/Res/Sprites/World"
os.makedirs(out_dir, exist_ok=True)

extracts = {
    # (source_file, x, y, w, h, output_name)
    "Fences.png": [
        (0, 0, 16, 16, "SL_Fence_Horizontal"),
        (16, 0, 16, 16, "SL_Fence_Vertical"),
        (32, 0, 16, 16, "SL_Fence_Corner"),
    ],
    "Chest.png": [
        (0, 0, 16, 16, "SL_Chest"),
    ],
    "Paths.png": [
        (16, 16, 16, 16, "SL_Path_Center"),
    ],
}

for filename, regions in extracts.items():
    src_path = os.path.join(arts_dir, "Tilesets", "Building parts", filename)
    if not os.path.exists(src_path):
        print(f"  skip: {src_path}")
        continue
    img = Image.open(src_path)
    for x, y, w, h, name in regions:
        tile = img.crop((x, y, x + w, y + h))
        out_path = os.path.join(out_dir, f"{name}.png")
        tile.save(out_path)
        print(f"  -> {out_path}")
```

注意：具体裁剪坐标需要在查看 sprite sheet 后调整。上述坐标是估算值。

- [ ] **Step 2: 运行 SpriteImportTool 设置导入参数**

在 Unity 菜单中执行 `CozyYard/配置 Sprite 导入设置`，确保新提取的 sprite 使用 PPU=16、Point filter。

---

### Task 11: Building 表添加 worldSprite 字段

**Files:**
- Modify: `Tools/Luban/gen_cozyyard_tables.py` — `create_building_xlsx()` 函数

- [ ] **Step 1: 添加 worldSprite 字段**

在 Task 6 已添加 `iconSprite` 的基础上，再添加 `worldSprite` 字段：

```python
headers  = ["##var", "id", "nameKey", "category", "sizeX", "sizeY", "materials", "materialQtys", "buildTime", "prerequisiteId", "level", "iconSprite", "worldSprite"]
types    = ["##type", "int", "string", "string", "int", "int", "(list#sep=,),int", "(list#sep=,),int", "int", "int", "int", "string", "string"]
comments = ["##",    "ID", "名称key",  "类别",      "宽",    "高",    "材料ID列表", "材料数量列表",   "建造时间(分钟)", "前置建筑ID", "等级", "UI图标sprite", "世界sprite名"]

rows = [
    ["", 1,  "building_1",   "House",      2, 2, "1003",      "20",     120, 0,  1, "SL_Item_c0_r1", ""],
    ["", 2,  "building_2",   "House",      3, 3, "1003,1002", "30,20",  180, 1,  2, "SL_Item_c1_r1", ""],
    ["", 10, "building_10",  "Production", 1, 1, "1003,1002", "5,3",    30,  0,  1, "SL_Item_c2_r1", "SL_WorkStation"],
    ["", 11, "building_11",  "Production", 1, 1, "1002,1003", "10,8",   60,  1,  2, "SL_Item_c3_r1", "SL_WorkStation"],
    ["", 20, "building_20",  "Production", 1, 1, "1003",      "8",      30,  0,  1, "SL_Item_c4_r1", ""],
    ["", 30, "building_30",  "Production", 1, 1, "1002",      "15",     60,  0,  1, "SL_Item_c5_r1", ""],
    ["", 40, "building_40",  "Livestock",  2, 2, "1003",      "12",     45,  0,  1, "SL_Item_c6_r1", ""],
    ["", 50, "building_50",  "Decoration", 1, 1, "1003",      "3",      10,  0,  1, "SL_Item_c7_r1", "SL_Fence_Horizontal"],
    ["", 60, "building_60",  "Functional", 1, 1, "1003,1002", "5,3",    20,  0,  1, "SL_Item_c0_r2", "SL_Chest"],
    ["", 70, "building_70",  "Functional", 2, 2, "1003,1002", "15,10",  90,  0,  1, "SL_Item_c1_r2", ""],
]
```

空字符串 `""` 表示该建筑暂无世界 sprite，将保持原有的类别颜色方块渲染。

- [ ] **Step 2: 重新生成 Excel + Luban**

```bash
cd "Tools/Luban"
python gen_cozyyard_tables.py && ./gen.sh
```

---

### Task 12: GridView 用 worldSprite 渲染建筑

**Files:**
- Modify: `Assets/Game/Scripts/Views/GridView.cs` — `LoadAndRenderAsync()`、`CreateBuildingVisual()` 方法

- [ ] **Step 1: 添加建筑 sprite 缓存**

在 `#region Sprite Assets` 中添加：

```csharp
private readonly Dictionary<int, Sprite> _buildingSprites = new();
```

- [ ] **Step 2: 在 LoadSpritesAsync 末尾预加载建筑 sprite**

```csharp
await LoadBuildingSprites();
```

新增方法：

```csharp
private async UniTask LoadBuildingSprites()
{
    var tbBuilding = GF.Config.GetTable<TbBuilding>();
    if (tbBuilding == null) return;

    foreach (var building in tbBuilding.DataList)
    {
        if (!string.IsNullOrEmpty(building.WorldSprite))
        {
            var s = await SpriteLoader.LoadAsync(building.WorldSprite);
            if (s != null) _buildingSprites[building.Id] = s;
        }
    }
}
```

- [ ] **Step 3: 修改 CreateBuildingVisual 使用 worldSprite**

替换 `CreateBuildingVisual()` 中的建筑渲染逻辑：

```csharp
private void CreateBuildingVisual(BuildingInstance building)
{
    if (_buildingObjects.ContainsKey(building.UniqueId)) return;

    var parent = new GameObject($"Building_{building.UniqueId}");
    parent.transform.SetParent(_tilesParent != null ? _tilesParent : transform);
    parent.transform.localPosition = Vector3.zero;

    var cfg = GF.Config.GetTable<TbBuilding>()?.GetOrDefault(building.BuildingId);
    int baseSortOrder = GridUtils.GetSortingOrder(building.GridX, building.GridY) + 10;

    bool hasWorldSprite = _buildingSprites.TryGetValue(building.BuildingId, out var worldSprite);

    if (hasWorldSprite && building.SizeX == 1 && building.SizeY == 1)
    {
        var wp = GridUtils.GridToWorld(building.GridX, building.GridY);
        var tileGo = new GameObject("Sprite");
        tileGo.transform.SetParent(parent.transform);
        tileGo.transform.localPosition = new Vector3(wp.x, wp.y, 0);
        var sr = tileGo.AddComponent<SpriteRenderer>();
        sr.sprite = worldSprite;
        sr.sortingOrder = baseSortOrder;
        sr.color = Color.white;
    }
    else
    {
        Color buildingColor = GetBuildingColor(cfg?.Category ?? "");
        for (int dx = 0; dx < building.SizeX; dx++)
        {
            for (int dy = 0; dy < building.SizeY; dy++)
            {
                var wp = GridUtils.GridToWorld(building.GridX + dx, building.GridY + dy);
                var tileGo = new GameObject($"Tile_{dx}_{dy}");
                tileGo.transform.SetParent(parent.transform);
                tileGo.transform.localPosition = new Vector3(wp.x, wp.y, 0);
                var sr = tileGo.AddComponent<SpriteRenderer>();
                sr.sprite = hasWorldSprite ? worldSprite : _grassSprite;
                sr.sortingOrder = baseSortOrder;
                sr.color = hasWorldSprite ? Color.white : buildingColor;
            }
        }

        if (!hasWorldSprite)
        {
            float centerX = building.GridX + (building.SizeX - 1) * 0.5f;
            float centerY = building.GridY + (building.SizeY - 1) * 0.5f;
            var labelWorldPos = new Vector2(
                centerX * GridUtils.TileSize,
                -centerY * GridUtils.TileSize
            );

            float labelWidth = Mathf.Max(building.SizeX, building.SizeY) * GridUtils.TileSize * 0.9f;
            float labelHeight = GridUtils.TileSize * 0.4f;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(parent.transform);
            labelGo.transform.localPosition = new Vector3(labelWorldPos.x, labelWorldPos.y, 0);
            var rt = labelGo.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(labelWidth, labelHeight);

            var tmp = labelGo.AddComponent<TextMeshPro>();
            tmp.text = cfg != null ? GF.Localization.Get(cfg.NameKey) : $"#{building.BuildingId}";
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 0.5f;
            tmp.fontSizeMax = 3f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.sortingOrder = baseSortOrder + 1;
        }
    }

    _buildingObjects[building.UniqueId] = parent;
}
```

- [ ] **Step 4: 确认编译通过并测试**

Play 模式下放置 1×1 建筑（围栏 id=50、饲料槽 id=60、野外篝火 id=10），确认使用 worldSprite 渲染。多格建筑保持颜色方块+标签。

- [ ] **Step 5: 提交 Phase C**

```bash
git add -A
git commit -m "feat: 建筑可视化 - 配置驱动 worldSprite 渲染，1x1建筑使用实际 sprite"
```

---

## 验证清单

执行完所有任务后，验证以下功能：

- [ ] 打开**背包**：所有物品格子显示 sprite 图标而非色块
- [ ] 打开**商店**：每个商品条目左侧有图标
- [ ] 打开**配方**：每个配方条目左侧显示产出物品图标
- [ ] 打开**建造**：每个建筑条目左侧有图标
- [ ] 世界地图上有**6种障碍物**（杂草、石头、树桩、蘑菇、大石头、大树桩）
- [ ] 空地上偶尔出现**装饰花草**
- [ ] 放置 1×1 建筑（围栏、工作台）显示实际 sprite
- [ ] 多格建筑保持颜色方块渲染（graceful fallback）
- [ ] 无编译错误、无运行时报错

---

## 后续可扩展方向（本次不实施）

| 方向 | 说明 |
|------|------|
| **UI 皮肤替换** | 用 Sprout Lands UI 包的对话框/按钮/图标替换程序化 UI |
| **玩家角色** | 提取角色 spritesheet，添加移动动画 |
| **动物可视化** | 鸡/牛 sprite + 动画，配合 AnimalSystem |
| **水域动画** | 用 Water_1~4.png 做水面波纹动画 |
| **多格建筑 sprite** | 用 House/Barn 部件拼接多格建筑视觉 |
| **树木系统** | 实现 TbTree 对应的种树/生长/采集系统 |
