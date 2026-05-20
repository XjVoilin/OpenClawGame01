# Plan 3: 背包系统

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 实现完整的背包/仓库系统——物品增删查、容量管理、堆叠逻辑、背包UI。

**Architecture:** InventoryStore (SavableStoreBase) 持有物品列表和容量，InventorySystem 处理增删逻辑和容量检查，InventoryWindow (UIWindow) 展示背包界面。物品定义通过 Luban TbItem 表驱动。

**Tech Stack:** Unity 2022.3, JulyArch (Store-System-View), JulyCore, Luban, UniTask

---

### Task 1: 实现 InventoryData 和 InventoryStore

**Files:**
- Create: `Assets/Game/Scripts/Modules/Inventory/InventoryData.cs`
- Create: `Assets/Game/Scripts/Modules/Inventory/IInventoryQueries.cs`
- Create: `Assets/Game/Scripts/Modules/Inventory/InventoryStore.cs`

- [ ] **Step 1: 创建 InventoryData.cs**

```csharp
using System;
using System.Collections.Generic;
using JulyCore.Data.Save;

namespace CozyYard
{
    [Serializable]
    public class ItemStack
    {
        public int ItemId;
        public int Quantity;
    }

    [Serializable]
    public class InventoryData : ISaveData
    {
        public int Capacity = 30;
        public List<ItemStack> Items = new();
        public int Coins;

        public SaveImportance Importance => SaveImportance.Normal;
    }
}
```

- [ ] **Step 2: 创建 IInventoryQueries.cs**

```csharp
using System.Collections.Generic;
using JulyArch;

namespace CozyYard
{
    public interface IInventoryQueries : IStoreQueries
    {
        int Capacity { get; }
        int UsedSlots { get; }
        int FreeSlots { get; }
        int Coins { get; }
        IReadOnlyList<ItemStack> Items { get; }
        int GetItemCount(int itemId);
        bool HasItem(int itemId, int quantity = 1);
        bool HasSpace(int itemId, int quantity = 1);
    }
}
```

- [ ] **Step 3: 创建 InventoryStore.cs**

```csharp
using System.Collections.Generic;

namespace CozyYard
{
    public class InventoryStore : SavableStoreBase<InventoryData>, IInventoryQueries
    {
        protected override string SaveKey => SaveKeys.InventoryData;

        public int Capacity => Data.Capacity;
        public int UsedSlots => Data.Items.Count;
        public int FreeSlots => Data.Capacity - Data.Items.Count;
        public int Coins => Data.Coins;
        public IReadOnlyList<ItemStack> Items => Data.Items;

        public int GetItemCount(int itemId)
        {
            var stack = FindStack(itemId);
            return stack?.Quantity ?? 0;
        }

        public bool HasItem(int itemId, int quantity = 1)
        {
            return GetItemCount(itemId) >= quantity;
        }

        public bool HasSpace(int itemId, int quantity = 1)
        {
            var existing = FindStack(itemId);
            if (existing != null) return true;
            return FreeSlots > 0;
        }

        public bool AddItem(int itemId, int quantity)
        {
            if (quantity <= 0) return false;

            var existing = FindStack(itemId);
            if (existing != null)
            {
                existing.Quantity += quantity;
                MarkDirty();
                return true;
            }

            if (FreeSlots <= 0) return false;

            Data.Items.Add(new ItemStack { ItemId = itemId, Quantity = quantity });
            MarkDirty();
            return true;
        }

        public bool RemoveItem(int itemId, int quantity)
        {
            if (quantity <= 0) return false;

            var existing = FindStack(itemId);
            if (existing == null || existing.Quantity < quantity) return false;

            existing.Quantity -= quantity;
            if (existing.Quantity <= 0)
            {
                Data.Items.Remove(existing);
            }
            MarkDirty();
            return true;
        }

        public void AddCoins(int amount)
        {
            Data.Coins += amount;
            MarkDirty();
        }

        public bool SpendCoins(int amount)
        {
            if (Data.Coins < amount) return false;
            Data.Coins -= amount;
            MarkDirty();
            return true;
        }

        public void SetCapacity(int capacity)
        {
            Data.Capacity = capacity;
            MarkDirty();
        }

        private ItemStack FindStack(int itemId)
        {
            for (int i = 0; i < Data.Items.Count; i++)
            {
                if (Data.Items[i].ItemId == itemId) return Data.Items[i];
            }
            return null;
        }
    }
}
```

- [ ] **Step 4: 提交**

```bash
git add -A
git commit -m "feat(inventory): add InventoryData, InventoryStore, IInventoryQueries"
```

---

### Task 2: 实现 InventorySystem

**Files:**
- Create: `Assets/Game/Scripts/Modules/Inventory/InventorySystem.cs`

- [ ] **Step 1: 创建 InventorySystem.cs**

```csharp
using JulyArch;

namespace CozyYard
{
    public class InventorySystem : GameSystemBase
    {
        private InventoryStore _store;

        protected override void OnInitialize()
        {
            _store = GetStore<InventoryStore>();
        }

        public bool AddItem(int itemId, int quantity = 1)
        {
            if (!_store.HasSpace(itemId, quantity)) return false;

            bool success = _store.AddItem(itemId, quantity);
            if (success)
            {
                Publish(new InventoryChangedEvent());
            }
            return success;
        }

        public bool RemoveItem(int itemId, int quantity = 1)
        {
            bool success = _store.RemoveItem(itemId, quantity);
            if (success)
            {
                Publish(new InventoryChangedEvent());
            }
            return success;
        }

        public bool HasItem(int itemId, int quantity = 1)
        {
            return _store.HasItem(itemId, quantity);
        }

        public int GetItemCount(int itemId)
        {
            return _store.GetItemCount(itemId);
        }

        public void AddCoins(int amount)
        {
            _store.AddCoins(amount);
            Publish(new InventoryChangedEvent());
        }

        public bool SpendCoins(int amount)
        {
            bool success = _store.SpendCoins(amount);
            if (success)
            {
                Publish(new InventoryChangedEvent());
            }
            return success;
        }

        public void ExpandCapacity(int additionalSlots)
        {
            _store.SetCapacity(_store.Capacity + additionalSlots);
            Publish(new InventoryChangedEvent());
        }

        /// <summary>
        /// 批量消耗多种物品（用于制作等）。全部满足才消耗，原子操作。
        /// </summary>
        public bool ConsumeItems(int[] itemIds, int[] quantities)
        {
            if (itemIds.Length != quantities.Length) return false;

            for (int i = 0; i < itemIds.Length; i++)
            {
                if (!_store.HasItem(itemIds[i], quantities[i])) return false;
            }

            for (int i = 0; i < itemIds.Length; i++)
            {
                _store.RemoveItem(itemIds[i], quantities[i]);
            }

            Publish(new InventoryChangedEvent());
            return true;
        }
    }
}
```

- [ ] **Step 2: 提交**

```bash
git add -A
git commit -m "feat(inventory): add InventorySystem with add/remove/consume logic"
```

---

### Task 3: 注册 Inventory 模块

**Files:**
- Modify: `Assets/Game/Scripts/HotUpdateRegistrar.cs`

- [ ] **Step 1: 在 RegisterStores 中添加**

```csharp
ctx.RegisterStore(new InventoryStore());
```

- [ ] **Step 2: 在 RegisterSystems 中添加**

```csharp
ctx.RegisterSystem(new InventorySystem());
```

- [ ] **Step 3: 提交**

```bash
git add -A
git commit -m "feat(inventory): register InventoryStore and InventorySystem"
```

---

### Task 4: 连接 Grid 开荒到 Inventory（清除障碍物获得材料）

**Files:**
- Modify: `Assets/Game/Scripts/Modules/Grid/GridSystem.cs`

- [ ] **Step 1: 修改 ClearObstacle 方法，添加物品掉落**

在 GridSystem 中添加对 InventorySystem 的引用和掉落逻辑：

```csharp
// 在 OnInitialize 中获取 InventorySystem
private InventorySystem _inventorySystem;

protected override void OnInitialize()
{
    _store = GetStore<GridStore>();
    _inventorySystem = GetSystem<InventorySystem>();
}

public bool ClearObstacle(int x, int y)
{
    var cell = _store.GetCell(x, y);
    if (cell == null || cell.State != CellState.Obstacle) return false;

    int obstacleId = cell.ObstacleId;
    cell.ObstacleId = 0;
    _store.SetCellState(x, y, CellState.Empty);

    // 根据障碍物类型给予材料 (对应 TbObstacle 表)
    switch (obstacleId)
    {
        case 1: // 杂草 -> 杂草纤维 x2
            _inventorySystem.AddItem(1001, 2);
            break;
        case 2: // 石头 -> 石头 x3
            _inventorySystem.AddItem(1002, 3);
            break;
        case 3: // 树桩 -> 木材 x5
            _inventorySystem.AddItem(1003, 5);
            break;
    }

    Publish(new GridCellChangedEvent { GridX = x, GridY = y, NewState = CellState.Empty });
    return true;
}
```

注意：后续可改为从 Luban TbObstacle 表读取掉落配置，目前硬编码与配表数据一致。

- [ ] **Step 2: 让 ClearObstacle 消耗时间**

在 ClearObstacle 成功后调用 TimeSystem：

```csharp
private TimeSystem _timeSystem;

// OnInitialize 中获取
_timeSystem = GetSystem<TimeSystem>();

// ClearObstacle 中成功后
_timeSystem.ConsumeTime(obstacleId == 1 ? 15 : obstacleId == 2 ? 30 : 60);
```

- [ ] **Step 3: 提交**

```bash
git add -A
git commit -m "feat(grid): clearing obstacles drops items and consumes time"
```

---

### Task 5: 创建 InventoryWindow（背包UI）

**Files:**
- Create: `Assets/Game/Scripts/Views/Windows/InventoryWindow/InventoryWindow.cs`

- [ ] **Step 1: 创建 InventoryWindow.cs**

```csharp
using System.Collections.Generic;
using JulyArch;
using JulyCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CozyYard
{
    public class InventoryWindow : GameUIView
    {
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private GameObject _itemSlotPrefab;
        [SerializeField] private TextMeshProUGUI _capacityText;
        [SerializeField] private TextMeshProUGUI _coinsText;
        [SerializeField] private Button _closeBtn;

        private readonly List<GameObject> _slotInstances = new();

        public override IGameContext GetArchitecture() => AppArch.Context;

        protected override void OnBeforeOpen()
        {
            this.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            if (_closeBtn) _closeBtn.onClick.AddListener(Close);
            Refresh();
        }

        protected override void OnClose()
        {
            if (_closeBtn) _closeBtn.onClick.RemoveAllListeners();
            this.UnsubscribeAll();
            ClearSlots();
        }

        private void OnInventoryChanged(InventoryChangedEvent e) => Refresh();

        private void Refresh()
        {
            var q = this.Query<IInventoryQueries>();

            if (_capacityText) _capacityText.text = $"{q.UsedSlots}/{q.Capacity}";
            if (_coinsText) _coinsText.text = q.Coins.ToString();

            ClearSlots();

            foreach (var stack in q.Items)
            {
                if (_itemSlotPrefab == null || _itemsContainer == null) break;

                var go = Object.Instantiate(_itemSlotPrefab, _itemsContainer);
                go.SetActive(true);

                var nameText = go.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText) nameText.text = $"#{stack.ItemId} x{stack.Quantity}";

                _slotInstances.Add(go);
            }
        }

        private void ClearSlots()
        {
            foreach (var go in _slotInstances)
            {
                Object.Destroy(go);
            }
            _slotInstances.Clear();
        }

        private void Close()
        {
            GF.UI.Close(UIWindowId.InventoryWindow);
        }
    }
}
```

注意：`GameUIView` 是之前灵药师项目中的 UIWindow 基类。如果该文件已被删除，需要重新创建一个简单版本或直接继承 `GameView`。检查框架中是否有 UIWindow 基类可用。如果 `GameUIView` 不存在，改为继承 `GameView` 并自行处理 open/close 逻辑。

- [ ] **Step 2: 提交**

```bash
git add -A
git commit -m "feat(inventory): add InventoryWindow UI"
```

---

## 计划完成标志

当以上 5 个 Task 全部完成后，背包系统应处于以下状态：
- InventoryStore 持有物品列表、容量、铜板
- InventorySystem 提供 AddItem/RemoveItem/ConsumeItems/AddCoins/SpendCoins
- 清除地图障碍物时自动获得对应材料并消耗时间
- InventoryWindow 可展示当前背包内容
- 所有模块已注册到 HotUpdateRegistrar

接下来进入 **Plan 4: 种植系统**。
