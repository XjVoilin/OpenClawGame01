# Sprout Lands 视觉集成 实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 将 Sprout Lands Basic Pack 的 16x16 像素美术资源集成到 CozyYard 项目，替换所有占位视觉，实现可见的游戏场景效果。

**Architecture:** 将坐标系从等距菱形（IsometricUtils）转为正交俯视（GridUtils），调整 GridView 的加载和渲染逻辑以使用新 sprite，添加简单摄像机控制器，启用已编写好的昼夜光照系统。

**Tech Stack:** Unity 2022 URP 2D, C#, YooAsset (sprite 加载), Luban (配置表), Cysharp/UniTask

---

## 前置：用户操作

1. 从 https://cupnooble.itch.io/sprout-lands-asset-pack 下载 **Sprout Lands - Sprites - Basic pack.zip**
2. 解压后将内容放入 `Assets/Game/Arts/SproutLands/`
3. 通知我已导入完成

> 后续所有 Task 在用户导入完成后执行。

---

### Task 1: Sprite 导入配置 Editor 工具

**Files:**
- Create: `Assets/Game/Scripts/Editor/SpriteImportTool.cs`

- [ ] **Step 1: 创建批量导入工具**

编写 Editor 脚本，扫描 `Assets/Game/Arts/SproutLands/` 下所有 PNG，设置统一导入参数：

```csharp
[MenuItem("CozyYard/配置 SproutLands 导入设置", false, 220)]
public static void ConfigureImportSettings()
{
    var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Game/Arts/SproutLands" });
    foreach (var guid in guids)
    {
        var path = AssetDatabase.GUIDToAssetPath(guid);
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) continue;

        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 16;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;

        // 如果是多图 sprite sheet，设置为 Multiple 模式
        if (importer.spriteImportMode == SpriteImportMode.Single)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null && (tex.width > 16 || tex.height > 16))
            {
                importer.spriteImportMode = SpriteImportMode.Multiple;
                // 自动按 16x16 网格切片
                var factory = new SpriteDataProviderFactories();
                factory.Init();
                // ... 使用 SpriteEditorExtension 切片
            }
        }

        importer.SaveAndReimport();
    }
    Debug.Log($"[SproutLands] 已配置 {guids.Length} 个纹理的导入设置");
}
```

> 注意：Sprout Lands 的 sprite sheet 可能需要手动在 Sprite Editor 中确认切片。此工具设置基本导入参数，切片细节可能需要微调。

- [ ] **Step 2: 将可用的单体 sprite 复制到 YooAsset 可加载路径**

在 `Assets/Game/Res/Sprites/` 下建立子目录结构：

```
Assets/Game/Res/Sprites/
├── Tiles/          ← 保留旧文件（兼容），新增 SproutLands 地块
├── World/          ← 作物、障碍物、建筑等世界 sprite
└── Items/          ← 物品图标（如果素材包中有）
```

编写一个编辑器方法，从 `Arts/SproutLands/` 中识别并复制/链接关键 sprite 到 `Res/Sprites/World/` 目录，使 YooAsset 的 `AddressByFileName` 规则可以按文件名加载。

---

### Task 2: 坐标系转换 — 等距 → 正交俯视

**Files:**
- Modify: `Assets/Game/Scripts/Modules/Grid/IsometricUtils.cs` → rename to `GridUtils.cs`
- Modify: `Assets/Game/Scripts/Views/GridView.cs`（所有 `IsometricUtils` 引用）

- [ ] **Step 1: 重写 IsometricUtils → GridUtils**

```csharp
using UnityEngine;

namespace CozyYard
{
    public static class GridUtils
    {
        public const float TileSize = 1f; // 16px / 16PPU = 1 world unit

        public static Vector2 GridToWorld(int gridX, int gridY)
        {
            return new Vector2(gridX * TileSize, -gridY * TileSize);
        }

        public static Vector2Int WorldToGrid(Vector2 worldPos)
        {
            return new Vector2Int(
                Mathf.RoundToInt(worldPos.x / TileSize),
                Mathf.RoundToInt(-worldPos.y / TileSize)
            );
        }

        public static int GetSortingOrder(int gridX, int gridY, int heightOffset = 0)
        {
            return -gridY * 100 - heightOffset;
        }
    }
}
```

- [ ] **Step 2: 全局替换引用**

在所有 C# 文件中替换：
- `IsometricUtils.GridToWorld` → `GridUtils.GridToWorld`
- `IsometricUtils.WorldToGrid` → `GridUtils.WorldToGrid`
- `IsometricUtils.GetSortingOrder` → `GridUtils.GetSortingOrder`
- `IsometricUtils.TileWidth` → `GridUtils.TileSize`
- `IsometricUtils.TileHeight` → `GridUtils.TileSize`

涉及文件：
- `GridView.cs` — 大量引用（~15处）
- 其他引用 `IsometricUtils` 的文件

- [ ] **Step 3: 删除旧文件**

删除 `IsometricUtils.cs` 和对应的 `.meta`（如果文件已重命名则不需要）。

---

### Task 3: 更新 GridView — 地块渲染

**Files:**
- Modify: `Assets/Game/Scripts/Views/GridView.cs`

- [ ] **Step 1: 更新 LoadTileSpritesAsync**

替换占位菱形 sprite 为 Sprout Lands 地块：

```csharp
private Sprite _grassSprite;    // 替代 _emptyTileSprite
private Sprite _soilSprite;     // 替代 _soilTileSprite
private Sprite _obstacleSprite; // 替代 _obstacleTileSprite（fallback）
private Sprite _highlightSprite;

private async UniTask LoadTileSpritesAsync()
{
    _grassSprite = await GF.Resource.LoadAsync<Sprite>("grass_tile");
    _soilSprite = await GF.Resource.LoadAsync<Sprite>("soil_tile");
    _obstacleSprite = await GF.Resource.LoadAsync<Sprite>("rock_tile");
    _highlightSprite = await GF.Resource.LoadAsync<Sprite>("highlight_tile");
}
```

> `grass_tile` / `soil_tile` 等名称取决于实际素材包中的文件名，需在导入后确认并映射。

- [ ] **Step 2: 更新 GetSpriteForState**

```csharp
private Sprite GetSpriteForState(CellState state)
{
    return state switch
    {
        CellState.Empty => _grassSprite,
        CellState.Soil => _soilSprite ?? _grassSprite,
        CellState.Obstacle => _obstacleSprite ?? _grassSprite,
        CellState.Unexplored => _grassSprite,  // 未探索区域用灰色草地
        _ => _grassSprite
    };
}
```

- [ ] **Step 3: 更新 RenderGrid 中的 Unexplored 着色**

`Unexplored` 格子改用半透明黑色覆盖代替纯色 tint，保持新 sprite 的视觉可辨识度：

```csharp
if (cell.State == CellState.Unexplored)
    sr.color = new Color(0.4f, 0.4f, 0.4f);
```

- [ ] **Step 4: 生成高亮 tile**

如果 Sprout Lands 中没有合适的高亮 tile，用 TileSpriteGenerator 生成一个 16×16 的白色方形半透明 sprite（替代菱形）：

```csharp
// TileSpriteGenerator 新增方形模式
private static Texture2D CreateSquareTexture(Color color, int size = 16)
{
    var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
    var pixels = new Color[size * size];
    for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
    tex.SetPixels(pixels);
    tex.Apply();
    return tex;
}
```

---

### Task 4: 更新 GridView — 作物视觉

**Files:**
- Modify: `Assets/Game/Scripts/Views/GridView.cs`

- [ ] **Step 1: 加载作物 sprite**

Sprout Lands Basic Pack 包含一些作物生长阶段 sprite。根据素材包内容，建立映射：

```csharp
private readonly Dictionary<int, Sprite[]> _cropStageSprites = new();

private async UniTask LoadCropSpritesAsync()
{
    // 每种作物 4 个阶段 sprite (Seed/Sprout/Growing/Mature)
    // cropId → sprite 名映射，根据素材包实际内容填充
    var cropSpriteMap = new Dictionary<int, string>
    {
        { 1, "cabbage" },   // 白菜
        { 2, "radish" },    // 萝卜
        { 3, "rice" },      // 糯米（如无，用通用作物）
        { 4, "flower" },    // 菊花
        { 5, "pepper" },    // 辣椒（如无，用通用作物）
    };

    foreach (var (cropId, spriteName) in cropSpriteMap)
    {
        var stages = new Sprite[4];
        for (int i = 0; i < 4; i++)
        {
            stages[i] = await GF.Resource.LoadAsync<Sprite>($"{spriteName}_stage_{i}");
        }
        _cropStageSprites[cropId] = stages;
    }
}
```

- [ ] **Step 2: 重写 CreateOrUpdateCropVisual**

用实际 sprite 替代颜色/缩放占位：

```csharp
private void CreateOrUpdateCropVisual(int x, int y, CropGrowthStage stage)
{
    var key = new Vector2Int(x, y);
    if (!_cropRenderers.TryGetValue(key, out var sr))
    {
        var worldPos = GridUtils.GridToWorld(x, y);
        var go = new GameObject($"Crop_{x}_{y}");
        go.transform.SetParent(_tilesParent != null ? _tilesParent : transform);
        go.transform.localPosition = new Vector3(worldPos.x, worldPos.y, 0);
        sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = GridUtils.GetSortingOrder(x, y) + 5;
        _cropRenderers[key] = sr;
    }

    int cropId = GetCropIdAt(x, y);
    int stageIndex = stage switch
    {
        CropGrowthStage.Seed => 0,
        CropGrowthStage.Sprout => 1,
        CropGrowthStage.Growing => 2,
        CropGrowthStage.Mature => 3,
        _ => 0
    };

    if (_cropStageSprites.TryGetValue(cropId, out var stages) && stageIndex < stages.Length && stages[stageIndex] != null)
    {
        sr.sprite = stages[stageIndex];
        sr.color = stage == CropGrowthStage.Withered ? new Color(0.5f, 0.4f, 0.3f) : Color.white;
    }
    else
    {
        // fallback: 使用旧的颜色方案
        sr.sprite = _soilSprite;
        sr.color = GetCropColor(stage);
    }

    sr.transform.localScale = Vector3.one;
    sr.gameObject.SetActive(true);
}

private int GetCropIdAt(int x, int y)
{
    var crop = _farmSystem.GetCropAt(x, y);
    return crop?.CropId ?? 0;
}
```

---

### Task 5: 更新 GridView — 障碍物视觉

**Files:**
- Modify: `Assets/Game/Scripts/Views/GridView.cs`

- [ ] **Step 1: 为不同障碍物类型加载不同 sprite**

```csharp
private readonly Dictionary<int, Sprite> _obstacleSprites = new();

private async UniTask LoadObstacleSpritesAsync()
{
    // ObstacleId 1=杂草, 2=石头, 3=树桩
    var map = new Dictionary<int, string>
    {
        { 1, "weed" },     // 对应素材包中的杂草/小植物
        { 2, "rock" },     // 对应素材包中的石头
        { 3, "stump" },    // 对应素材包中的树桩
    };

    foreach (var (id, name) in map)
    {
        var sprite = await GF.Resource.LoadAsync<Sprite>(name);
        if (sprite != null) _obstacleSprites[id] = sprite;
    }
}
```

- [ ] **Step 2: 在 RenderGrid 中按障碍物类型显示不同 sprite**

在 `RenderGrid` 中，当 `cell.State == CellState.Obstacle` 时，查找 `cell.ObstacleId` 对应的 sprite：

```csharp
if (cell.State == CellState.Obstacle && _obstacleSprites.TryGetValue(cell.ObstacleId, out var obSprite))
    sr.sprite = obSprite;
```

同时在 `OnCellChanged` 中也做相应更新。

---

### Task 6: 改进 GridView — 建筑视觉

**Files:**
- Modify: `Assets/Game/Scripts/Views/GridView.cs`

- [ ] **Step 1: 为建筑使用统一的彩色方块（暂无精确建筑 sprite）**

Sprout Lands Basic Pack 中可能不包含完整的建筑 sprite。改进策略：
- 用 16×16 方形 sprite + 按建筑类别着色（替代菱形）
- 保持 TMP 标签显示建筑名称
- 调整标签位置为正交坐标

```csharp
private void CreateBuildingVisual(BuildingInstance building)
{
    if (_buildingObjects.ContainsKey(building.UniqueId)) return;

    var parent = new GameObject($"Building_{building.UniqueId}");
    parent.transform.SetParent(_tilesParent != null ? _tilesParent : transform);
    parent.transform.localPosition = Vector3.zero;

    var cfg = GF.Config.GetTable<TbBuilding>()?.GetOrDefault(building.BuildingId);
    Color buildingColor = GetBuildingColor(cfg?.Category ?? "");

    int baseSortOrder = GridUtils.GetSortingOrder(building.GridX, building.GridY) + 10;

    for (int dx = 0; dx < building.SizeX; dx++)
    {
        for (int dy = 0; dy < building.SizeY; dy++)
        {
            var wp = GridUtils.GridToWorld(building.GridX + dx, building.GridY + dy);
            var tileGo = new GameObject($"Tile_{dx}_{dy}");
            tileGo.transform.SetParent(parent.transform);
            tileGo.transform.localPosition = new Vector3(wp.x, wp.y, 0);
            var sr = tileGo.AddComponent<SpriteRenderer>();
            sr.sprite = _grassSprite; // 使用草地 tile 作为底色
            sr.sortingOrder = baseSortOrder;
            sr.color = buildingColor;
        }
    }

    // 标签居中于建筑占地中心
    float centerX = building.GridX + (building.SizeX - 1) * 0.5f;
    float centerY = building.GridY + (building.SizeY - 1) * 0.5f;
    var labelPos = GridUtils.GridToWorld((int)centerX, (int)centerY);
    // ... 创建 TMP 标签
}

private static Color GetBuildingColor(string category)
{
    return category switch
    {
        "House" => new Color(0.8f, 0.55f, 0.35f),
        "Production" => new Color(0.7f, 0.7f, 0.5f),
        "Livestock" => new Color(0.6f, 0.8f, 0.5f),
        "Decoration" => new Color(0.7f, 0.6f, 0.8f),
        "Functional" => new Color(0.5f, 0.7f, 0.8f),
        _ => new Color(0.7f, 0.7f, 0.7f)
    };
}
```

---

### Task 7: 摄像机设置 + 简单控制器

**Files:**
- Create: `Assets/Game/Scripts/Views/CameraController.cs`
- Modify: `Assets/Game/Scenes/Main.unity`（通过代码调整）
- Modify: `Assets/Game/Scripts/Modules/SceneFlow/MainSceneSetup.cs`

- [ ] **Step 1: 创建 CameraController**

```csharp
using UnityEngine;

namespace CozyYard
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float _panSpeed = 8f;
        [SerializeField] private float _zoomSpeed = 2f;
        [SerializeField] private float _minOrtho = 4f;
        [SerializeField] private float _maxOrtho = 14f;

        private Camera _cam;
        private Vector2 _minBounds;
        private Vector2 _maxBounds;

        public void Initialize(int gridWidth, int gridHeight)
        {
            _cam = Camera.main;
            _minBounds = GridUtils.GridToWorld(0, gridHeight);
            _maxBounds = GridUtils.GridToWorld(gridWidth, 0);

            // 初始位置：网格中心
            float cx = gridWidth * GridUtils.TileSize * 0.5f;
            float cy = -gridHeight * GridUtils.TileSize * 0.5f;
            transform.position = new Vector3(cx, cy, -10f);

            if (_cam != null) _cam.orthographicSize = 8f;
        }

        private void Update()
        {
            if (_cam == null) return;

            // WASD / 方向键平移
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            if (h != 0 || v != 0)
            {
                var delta = new Vector3(h, v, 0) * (_panSpeed * Time.deltaTime);
                transform.position += delta;
            }

            // 滚轮缩放
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                _cam.orthographicSize = Mathf.Clamp(
                    _cam.orthographicSize - scroll * _zoomSpeed,
                    _minOrtho, _maxOrtho);
            }

            // 限制在网格范围内
            ClampPosition();
        }

        private void ClampPosition()
        {
            var pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, _minBounds.x - 2f, _maxBounds.x + 2f);
            pos.y = Mathf.Clamp(pos.y, _minBounds.y - 2f, _maxBounds.y + 2f);
            transform.position = pos;
        }
    }
}
```

- [ ] **Step 2: 在 MainSceneSetup 中初始化摄像机**

```csharp
protected override void OnEnter()
{
    CreateSceneView<GridView>("[GridView]");
    SetupCamera();
    OpenWindow(UIWindowId.GameHUD);
    OpenWindow(UIWindowId.TimeHUD);
    OpenWindow(UIWindowId.WeatherHUD);
}

private void SetupCamera()
{
    var cam = Camera.main;
    if (cam == null) return;

    var controller = cam.gameObject.AddComponent<CameraController>();
    // GridSystem 通过 GF.Arch 获取 width/height
    var gridStore = GameArch.Context.GetStore<GridStore>();
    controller.Initialize(gridStore.Width, gridStore.Height);
}
```

---

### Task 8: 启用昼夜光照

**Files:**
- Modify: `Assets/Game/Scripts/Modules/SceneFlow/MainSceneSetup.cs`
- Modify: `Assets/Game/Scripts/Views/TimeLightingView.cs`（改为运行时创建 Light2D）

- [ ] **Step 1: 修改 TimeLightingView，运行时创建 Light2D**

当前 `TimeLightingView` 需要序列化引用 `Light2D _globalLight`。改为在 `OnViewEnable` 中自动创建：

```csharp
protected override void OnViewEnable()
{
    if (_globalLight == null)
    {
        var lightGo = new GameObject("GlobalLight2D");
        lightGo.transform.SetParent(transform);
        _globalLight = lightGo.AddComponent<Light2D>();
        _globalLight.lightType = Light2D.LightType.Global;
        _globalLight.intensity = 1f;
        _globalLight.color = Color.white;
    }

    this.Subscribe<PhaseChangedEvent>(OnPhaseChanged);
    UpdateTargetFromCurrentPhase();
    ApplyImmediate();
}
```

- [ ] **Step 2: 在 MainSceneSetup 中启用 TimeLightingView**

```csharp
protected override void OnEnter()
{
    CreateSceneView<GridView>("[GridView]");
    CreateSceneView<TimeLightingView>("[Lighting]");
    SetupCamera();
    OpenWindow(UIWindowId.GameHUD);
    OpenWindow(UIWindowId.TimeHUD);
    OpenWindow(UIWindowId.WeatherHUD);
}
```

---

### Task 9: Sprite 名称映射（导入后执行）

**Files:**
- Create: `Assets/Game/Scripts/Modules/Grid/SpriteMapping.cs`

- [ ] **Step 1: 创建 sprite 名称映射静态类**

导入素材包后，根据实际文件名建立映射：

```csharp
namespace CozyYard
{
    public static class SpriteMapping
    {
        // 地块
        public const string Grass = "??";       // 填入实际文件名
        public const string Soil = "??";
        public const string Highlight = "Tile_Highlight_Square";

        // 障碍物 (ObstacleId → sprite name)
        public static readonly string[] Obstacles = { "", "??", "??", "??" };

        // 作物 (CropId → sprite 前缀，阶段后缀 _0 _1 _2 _3)
        public static readonly string[] CropPrefixes = { "", "??", "??", "??", "??", "??" };

        // 建筑类别颜色保持代码内定义
    }
}
```

> 所有 `??` 在导入素材包后根据实际文件名填充。GridView 中的 `LoadAsync<Sprite>()` 调用全部引用此映射。

---

### Task 10: 更新 TileSpriteGenerator

**Files:**
- Modify: `Assets/Game/Scripts/Editor/TileSpriteGenerator.cs`

- [ ] **Step 1: 新增方形 tile 生成**

保留原有菱形生成能力，新增 16×16 方形 sprite 生成（用于高亮和 fallback）：

```csharp
[MenuItem("CozyYard/生成方形高亮 Sprite", false, 211)]
public static void GenerateSquareHighlight()
{
    const int size = 16;
    var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
    var pixels = new Color[size * size];
    var borderColor = new Color(1f, 1f, 1f, 0.8f);
    var fillColor = new Color(1f, 1f, 1f, 0.3f);

    for (int y = 0; y < size; y++)
    {
        for (int x = 0; x < size; x++)
        {
            bool isBorder = x == 0 || x == size - 1 || y == 0 || y == size - 1;
            pixels[y * size + x] = isBorder ? borderColor : fillColor;
        }
    }

    tex.SetPixels(pixels);
    tex.Apply();
    // 保存到 Assets/Game/Res/Sprites/Tiles/Tile_Highlight_Square.png
    // 导入设置 PPU=16, Point filter
}
```

---

## 预期效果

完成所有 Task 后，运行游戏应看到：

1. **草地 tile** 覆盖整个 24×24 网格（替代彩色菱形）
2. **不同类型障碍物** 显示不同 sprite（杂草/石头/树桩）
3. **翻地后** 显示泥土 tile
4. **种植作物** 显示真实的生长阶段 sprite
5. **建筑** 显示按类别着色的方块 + 名称标签
6. **鼠标高亮** 使用方形半透明 sprite
7. **WASD/方向键** 可平移摄像机，**滚轮** 可缩放
8. **昼夜光照** 随游戏时间变化（晨昏/白天/夜晚颜色过渡）

## 后续扩展（不在本计划范围）

- 为每种建筑添加独立 sprite（需付费版或另寻素材）
- 物品图标接入 UI（需 Item 表添加 `iconSprite` 字段）
- 角色 sprite + 动画
- 动物 sprite
- 音效/BGM
- Pixel Perfect Camera 配置
