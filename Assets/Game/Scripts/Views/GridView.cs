using System;
using System.Collections.Generic;
using cfg;
using Cysharp.Threading.Tasks;
using JulyArch;
using JulyCore;
using JulyGame;
using TMPro;
using UnityEngine;

namespace CozyYard
{
    public class GridView : GameView
    {
        [SerializeField] private Transform _tilesParent;

        private static readonly Color ValidPlacementColor = new(0.2f, 0.9f, 0.2f, 0.6f);
        private static readonly Color InvalidPlacementColor = new(0.9f, 0.2f, 0.2f, 0.6f);
        private static readonly Color NormalHighlightColor = new(1f, 1f, 1f, 0.5f);
        private static readonly Color PlantableColor = new(0.3f, 0.9f, 0.5f, 0.6f);
        private static readonly Color UnexploredTint = new(0.35f, 0.35f, 0.35f);

        private GridSystem _gridSystem;
        private FarmSystem _farmSystem;
        private BuildSystem _buildSystem;
        private SpriteRenderer[,] _tileRenderers;
        private GameObject _highlightObj;
        private SpriteRenderer _highlightRenderer;
        private readonly List<GameObject> _placementHighlights = new();
        private readonly Dictionary<int, GameObject> _buildingObjects = new();
        private readonly Dictionary<Vector2Int, SpriteRenderer> _cropRenderers = new();

        private bool _inPlantingMode;
        private int _plantingSeedItemId;
        private int _plantingCropId;

        private bool _inPlacementMode;
        private int _placementBuildingId;
        private int _placementSizeX;
        private int _placementSizeY;

        #region Sprite Assets

        private Sprite _grassSprite;
        private Sprite[] _grassVariants;
        private Sprite _soilSprite;
        private Sprite _highlightSprite;
        private readonly List<IDisposable> _baseSpriteHandles = new();

        private static readonly string[] GrassVariantNames =
        {
            "SL_Grass", "SL_GrassFlowers1", "SL_GrassFlowers2",
            "SL_GrassDetail1", "SL_GrassDetail2"
        };

        private static readonly string[] DecoSpriteNames =
        {
            "SL_Deco_c0_r0", "SL_Deco_c1_r0", "SL_Deco_c3_r0",
            "SL_Deco_c5_r0", "SL_Deco_c0_r1", "SL_Deco_c1_r1",
            "SL_Deco_c2_r1", "SL_Deco_c3_r1"
        };

        #endregion

        public override IArchContext GetArchitecture() => GameArch.Context;

        protected override void OnViewEnable()
        {
            _gridSystem = GetSystem<GridSystem>();
            _farmSystem = GetSystem<FarmSystem>();
            _buildSystem = GetSystem<BuildSystem>();
            Subscribe<GridCellChangedEvent>(OnCellChanged);
            Subscribe<BuildingPlacedEvent>(OnBuildingPlaced);
            Subscribe<BuildingRemovedEvent>(OnBuildingRemoved);
            Subscribe<EnterPlacementModeEvent>(OnEnterPlacementMode);
            Subscribe<CropPlantedEvent>(OnCropPlanted);
            Subscribe<CropGrowthEvent>(OnCropGrowth);
            Subscribe<CropWateredEvent>(OnCropWatered);
            Subscribe<CropReadyEvent>(OnCropReady);
            Subscribe<CropWitheredEvent>(OnCropWithered);
            Subscribe<CropHarvestedEvent>(OnCropHarvested);
            Subscribe<EnterPlantingModeEvent>(OnEnterPlantingMode);
            LoadAndRenderAsync().Forget();
        }

        private async UniTaskVoid LoadAndRenderAsync()
        {
            await LoadSpritesAsync();
            RenderGrid();
            RenderExistingBuildings();
            RenderExistingCrops();
            CreateHighlight();
        }

        #region Sprite Loading

        private async UniTask LoadSpritesAsync()
        {
            _grassSprite = await LoadSpriteOrNull("SL_Grass");
            _soilSprite = await LoadSpriteOrNull("SL_TilledDirt");
            _highlightSprite = await LoadSpriteOrNull("Tile_Highlight");

            var variants = new List<Sprite>();
            foreach (var name in GrassVariantNames)
            {
                var s = await LoadSpriteOrNull(name);
                if (s != null) variants.Add(s);
            }
            _grassVariants = variants.Count > 0 ? variants.ToArray() : new[] { _grassSprite };
        }

        private async UniTask<Sprite> LoadSpriteOrNull(string name)
        {
            try
            {
                var handle = await GF.Resource.LoadWithHandleAsync<Sprite>(name);
                if (handle == null || !handle.IsValid) return null;
                _baseSpriteHandles.Add(handle);
                return handle.Asset;
            }
            catch { return null; }
        }

        #endregion

        #region Update / Input

        private void Update()
        {
            if (_highlightObj == null) return;

            if (_inPlacementMode)
                UpdatePlacementMode();
            else if (_inPlantingMode)
                UpdatePlantingMode();
            else
                UpdateNormalMode();
        }

        private Vector2Int GetGridPosUnderMouse()
        {
            if (Camera.main == null) return new Vector2Int(-1, -1);
            var mouseScreen = Input.mousePosition;
            mouseScreen.z = -Camera.main.transform.position.z;
            var mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
            return GridUtils.WorldToGrid(new Vector2(mouseWorld.x, mouseWorld.y));
        }

        #endregion

        #region Placement Mode

        private void OnEnterPlantingMode(EnterPlantingModeEvent evt)
        {
            _inPlantingMode = true;
            _plantingSeedItemId = evt.SeedItemId;
            _plantingCropId = evt.CropId;
            _inPlacementMode = false;
        }

        private void OnEnterPlacementMode(EnterPlacementModeEvent evt)
        {
            _inPlantingMode = false;
            _inPlacementMode = true;
            _placementBuildingId = evt.BuildingId;

            var cfg = GF.Config.GetTable<TbBuilding>()?.GetOrDefault(evt.BuildingId);
            _placementSizeX = cfg != null ? cfg.SizeX : 1;
            _placementSizeY = cfg != null ? cfg.SizeY : 1;
            EnsurePlacementHighlights(_placementSizeX * _placementSizeY);
        }

        private void CancelPlacement()
        {
            _inPlacementMode = false;
            _placementBuildingId = 0;
            HidePlacementHighlights();
            _highlightObj.SetActive(false);
            _highlightRenderer.color = NormalHighlightColor;
            GetArchitecture().Event.Publish(new PlacementCancelledEvent());
        }

        private void UpdatePlacementMode()
        {
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
                return;
            }

            var gridPos = GetGridPosUnderMouse();
            if (_gridSystem.IsInBounds(gridPos.x, gridPos.y))
            {
                bool canPlace = _buildSystem.CanBuild(_placementBuildingId, gridPos.x, gridPos.y);
                Color color = canPlace ? ValidPlacementColor : InvalidPlacementColor;

                int idx = 0;
                for (int dx = 0; dx < _placementSizeX; dx++)
                {
                    for (int dy = 0; dy < _placementSizeY; dy++)
                    {
                        if (idx < _placementHighlights.Count)
                        {
                            var h = _placementHighlights[idx];
                            int cx = gridPos.x + dx;
                            int cy = gridPos.y + dy;
                            if (_gridSystem.IsInBounds(cx, cy))
                            {
                                h.SetActive(true);
                                var wp = GridUtils.GridToWorld(cx, cy);
                                h.transform.localPosition = new Vector3(wp.x, wp.y, 0);
                                h.GetComponent<SpriteRenderer>().color = color;
                            }
                            else
                            {
                                h.SetActive(false);
                            }
                        }
                        idx++;
                    }
                }
                for (; idx < _placementHighlights.Count; idx++)
                    _placementHighlights[idx].SetActive(false);

                _highlightObj.SetActive(false);

                if (Input.GetMouseButtonDown(0) && canPlace)
                {
                    _buildSystem.Build(_placementBuildingId, gridPos.x, gridPos.y);
                    _inPlacementMode = false;
                    _placementBuildingId = 0;
                    HidePlacementHighlights();
                }
            }
            else
            {
                HidePlacementHighlights();
            }
        }

        #endregion

        #region Planting Mode

        private void UpdatePlantingMode()
        {
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                _inPlantingMode = false;
                _highlightObj.SetActive(false);
                return;
            }

            var gridPos = GetGridPosUnderMouse();
            if (_gridSystem.IsInBounds(gridPos.x, gridPos.y))
            {
                var cell = _gridSystem.GetCell(gridPos.x, gridPos.y);
                bool isSoilReady = cell != null && cell.State == CellState.Soil && _farmSystem.GetCropAt(gridPos.x, gridPos.y) == null;
                bool canTillAndPlant = cell != null && cell.State == CellState.Empty;
                bool canPlant = isSoilReady || canTillAndPlant;

                _highlightObj.SetActive(true);
                var worldPos = GridUtils.GridToWorld(gridPos.x, gridPos.y);
                _highlightObj.transform.localPosition = new Vector3(worldPos.x, worldPos.y, 0);
                _highlightRenderer.color = canPlant ? PlantableColor : InvalidPlacementColor;

                if (Input.GetMouseButtonDown(0) && canPlant)
                {
                    var invSystem = GetSystem<InventorySystem>();
                    if (invSystem.HasItem(_plantingSeedItemId, 1))
                    {
                        if (canTillAndPlant)
                            _farmSystem.TillSoil(gridPos.x, gridPos.y);
                        _farmSystem.PlantCrop(gridPos.x, gridPos.y, _plantingCropId, _plantingSeedItemId);
                        if (!invSystem.HasItem(_plantingSeedItemId, 1))
                        {
                            _inPlantingMode = false;
                            _highlightObj.SetActive(false);
                        }
                    }
                }
            }
            else
            {
                _highlightObj.SetActive(false);
            }
        }

        #endregion

        #region Normal Mode

        private void UpdateNormalMode()
        {
            var gridPos = GetGridPosUnderMouse();
            if (_gridSystem.IsInBounds(gridPos.x, gridPos.y))
            {
                _highlightObj.SetActive(true);
                var worldPos = GridUtils.GridToWorld(gridPos.x, gridPos.y);
                _highlightObj.transform.localPosition = new Vector3(worldPos.x, worldPos.y, 0);
                _highlightRenderer.color = NormalHighlightColor;
            }
            else
            {
                _highlightObj.SetActive(false);
            }

            if (Input.GetMouseButtonDown(0) && _gridSystem.IsInBounds(gridPos.x, gridPos.y))
                OnTileClicked(gridPos.x, gridPos.y);
        }

        private void OnTileClicked(int x, int y)
        {
            var cell = _gridSystem.GetCell(x, y);
            if (cell == null) return;

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
                        GF.UI.Open(UIWindowId.InventoryWindow);
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

        #endregion

        #region Grid Rendering

        private void RenderGrid()
        {
            var w = _gridSystem.Width;
            var h = _gridSystem.Height;
            _tileRenderers = new SpriteRenderer[w, h];

            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var cell = _gridSystem.GetCell(x, y);
                    var worldPos = GridUtils.GridToWorld(x, y);

                    var go = new GameObject($"Tile_{x}_{y}");
                    go.transform.SetParent(_tilesParent != null ? _tilesParent : transform);
                    go.transform.localPosition = new Vector3(worldPos.x, worldPos.y, 0);

                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sortingOrder = GridUtils.GetSortingOrder(x, y);

                    ApplyCellVisual(sr, cell, x, y);
                    _tileRenderers[x, y] = sr;
                }
            }
        }

        private void ApplyCellVisual(SpriteRenderer sr, GridCellData cell, int x, int y)
        {
            switch (cell.State)
            {
                case CellState.Empty:
                    sr.sprite = PickGrassVariant(x, y);
                    sr.color = Color.white;
                    TryAddDecoOverlay(sr.gameObject, x, y);
                    break;
                case CellState.Soil:
                    sr.sprite = _soilSprite != null ? _soilSprite : PickGrassVariant(x, y);
                    sr.color = Color.white;
                    break;
                case CellState.Obstacle:
                    sr.sprite = PickGrassVariant(x, y);
                    var obCfg = GF.Config.GetTable<TbObstacle>()?.GetOrDefault(cell.ObstacleId);
                    if (obCfg != null && !string.IsNullOrEmpty(obCfg.IconSprite))
                    {
                        sr.color = Color.white;
                        AddObstacleOverlay(sr.gameObject, obCfg.IconSprite, x, y);
                    }
                    else
                    {
                        sr.color = new Color(0.7f, 0.6f, 0.5f);
                    }
                    break;
                case CellState.Unexplored:
                    sr.sprite = PickGrassVariant(x, y);
                    sr.color = UnexploredTint;
                    break;
                default:
                    sr.sprite = _grassSprite;
                    sr.color = Color.white;
                    break;
            }
        }

        private Sprite PickGrassVariant(int x, int y)
        {
            if (_grassVariants == null || _grassVariants.Length == 0) return _grassSprite;
            int hash = x * 7 + y * 13;
            if (hash % 3 == 0 && _grassVariants.Length > 1)
                return _grassVariants[Mathf.Abs(hash) % _grassVariants.Length];
            return _grassVariants[0];
        }

        private void AddObstacleOverlay(GameObject tileGo, string spriteName, int x, int y)
        {
            var overlayGo = new GameObject("ObstacleOverlay");
            overlayGo.transform.SetParent(tileGo.transform);
            overlayGo.transform.localPosition = Vector3.zero;
            var sr = overlayGo.AddComponent<SpriteRenderer>();
            sr.sortingOrder = GridUtils.GetSortingOrder(x, y) + 1;
            sr.LoadSprite(spriteName);
        }

        private void TryAddDecoOverlay(GameObject tileGo, int x, int y)
        {
            int hash = x * 31 + y * 17;
            if (hash % 8 != 0) return;

            var existing = tileGo.transform.Find("DecoOverlay");
            if (existing != null) return;

            var decoName = DecoSpriteNames[Mathf.Abs(hash / 8) % DecoSpriteNames.Length];
            var overlayGo = new GameObject("DecoOverlay");
            overlayGo.transform.SetParent(tileGo.transform);
            overlayGo.transform.localPosition = Vector3.zero;
            var sr = overlayGo.AddComponent<SpriteRenderer>();
            sr.sortingOrder = GridUtils.GetSortingOrder(x, y) + 1;
            sr.LoadSprite(decoName);
        }

        private void CreateHighlight()
        {
            _highlightObj = new GameObject("Highlight");
            _highlightObj.transform.SetParent(transform);
            _highlightRenderer = _highlightObj.AddComponent<SpriteRenderer>();
            _highlightRenderer.sprite = _highlightSprite != null ? _highlightSprite : _grassSprite;
            _highlightRenderer.sortingOrder = 9999;
            _highlightRenderer.color = NormalHighlightColor;
            _highlightObj.SetActive(false);
        }

        private void EnsurePlacementHighlights(int count)
        {
            while (_placementHighlights.Count < count)
            {
                var go = new GameObject($"PlacementHighlight_{_placementHighlights.Count}");
                go.transform.SetParent(transform);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _highlightSprite != null ? _highlightSprite : _grassSprite;
                sr.sortingOrder = 9998;
                go.SetActive(false);
                _placementHighlights.Add(go);
            }
        }

        private void HidePlacementHighlights()
        {
            foreach (var h in _placementHighlights)
                h.SetActive(false);
        }

        private void OnCellChanged(GridCellChangedEvent evt)
        {
            if (_tileRenderers == null) return;
            int x = evt.GridX, y = evt.GridY;
            if (x < 0 || x >= _tileRenderers.GetLength(0) || y < 0 || y >= _tileRenderers.GetLength(1)) return;

            var sr = _tileRenderers[x, y];
            var cell = _gridSystem.GetCell(x, y);
            if (cell == null) return;

            // 清除旧的障碍物覆盖层
            var overlay = sr.transform.Find("ObstacleOverlay");
            if (overlay != null) Destroy(overlay.gameObject);
            var deco = sr.transform.Find("DecoOverlay");
            if (deco != null) Destroy(deco.gameObject);

            ApplyCellVisual(sr, cell, x, y);
        }

        #endregion

        #region Crop Visuals

        private void RenderExistingCrops()
        {
            var farmStore = GetStore<FarmStore>();
            foreach (var crop in farmStore.Crops)
                CreateOrUpdateCropVisual(crop.GridX, crop.GridY, crop.Stage);
        }

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
            var spriteName = GetCropSpriteName(cropId, stage);

            if (spriteName != null)
            {
                sr.LoadSprite(spriteName);
                sr.color = stage == CropGrowthStage.Withered ? new Color(0.5f, 0.4f, 0.3f) : Color.white;
            }
            else
            {
                sr.sprite = _soilSprite ?? _grassSprite;
                sr.color = GetFallbackCropColor(stage);
            }

            sr.transform.localScale = Vector3.one;
            sr.gameObject.SetActive(true);
        }

        private int GetCropIdAt(int x, int y)
        {
            var crop = _farmSystem.GetCropAt(x, y);
            return crop?.CropId ?? 0;
        }

        private static string GetCropSpriteName(int cropId, CropGrowthStage stage)
        {
            if (cropId < 1 || cropId > 5) return null;
            int col = cropId - 1;
            int baseRow = col * 3;
            int rowOffset = stage switch
            {
                CropGrowthStage.Seed => 0,
                CropGrowthStage.Sprout => 1,
                _ => 2
            };
            return $"SL_Crop_c{col}_r{baseRow + rowOffset}";
        }

        private void RemoveCropVisual(int x, int y)
        {
            var key = new Vector2Int(x, y);
            if (_cropRenderers.TryGetValue(key, out var sr))
            {
                if (sr != null && sr.gameObject != null) Destroy(sr.gameObject);
                _cropRenderers.Remove(key);
            }
        }

        private static Color GetFallbackCropColor(CropGrowthStage stage)
        {
            return stage switch
            {
                CropGrowthStage.Seed => new Color(0.4f, 0.55f, 0.3f, 0.8f),
                CropGrowthStage.Sprout => new Color(0.3f, 0.7f, 0.3f, 0.9f),
                CropGrowthStage.Growing => new Color(0.2f, 0.8f, 0.2f, 1f),
                CropGrowthStage.Mature => new Color(1f, 0.85f, 0.1f, 1f),
                CropGrowthStage.Withered => new Color(0.4f, 0.3f, 0.2f, 0.8f),
                _ => new Color(0.4f, 0.55f, 0.3f, 0.8f)
            };
        }

        private void OnCropPlanted(CropPlantedEvent evt) => CreateOrUpdateCropVisual(evt.GridX, evt.GridY, CropGrowthStage.Seed);
        private void OnCropGrowth(CropGrowthEvent evt) => CreateOrUpdateCropVisual(evt.GridX, evt.GridY, evt.NewStage);
        private void OnCropReady(CropReadyEvent evt) => CreateOrUpdateCropVisual(evt.GridX, evt.GridY, CropGrowthStage.Mature);
        private void OnCropWithered(CropWitheredEvent evt) => CreateOrUpdateCropVisual(evt.GridX, evt.GridY, CropGrowthStage.Withered);
        private void OnCropHarvested(CropHarvestedEvent evt) => RemoveCropVisual(evt.GridX, evt.GridY);

        private void OnCropWatered(CropWateredEvent evt)
        {
            var key = new Vector2Int(evt.GridX, evt.GridY);
            if (_cropRenderers.TryGetValue(key, out var sr) && sr != null)
            {
                var c = sr.color;
                sr.color = new Color(c.r * 0.8f, c.g * 0.9f, Mathf.Min(c.b * 1.2f, 1f), c.a);
            }
        }

        #endregion

        #region Building Visuals

        private void OnBuildingPlaced(BuildingPlacedEvent evt)
        {
            var buildStore = GetStore<BuildStore>();
            var building = buildStore.GetBuildingAt(evt.GridX, evt.GridY);
            if (building != null) CreateBuildingVisual(building);
        }

        private void OnBuildingRemoved(BuildingRemovedEvent evt)
        {
            var toRemove = new List<int>();
            var buildStore = GetStore<BuildStore>();

            foreach (var (uid, go) in _buildingObjects)
            {
                bool found = false;
                foreach (var b in buildStore.Buildings)
                {
                    if (b.UniqueId == uid) { found = true; break; }
                }
                if (!found || go == null)
                {
                    toRemove.Add(uid);
                    if (go != null) Destroy(go);
                }
            }
            foreach (int uid in toRemove) _buildingObjects.Remove(uid);
        }

        private void RenderExistingBuildings()
        {
            var buildStore = GetStore<BuildStore>();
            foreach (var building in buildStore.Buildings)
                CreateBuildingVisual(building);
        }

        private void CreateBuildingVisual(BuildingInstance building)
        {
            if (_buildingObjects.ContainsKey(building.UniqueId)) return;

            var parent = new GameObject($"Building_{building.UniqueId}");
            parent.transform.SetParent(_tilesParent != null ? _tilesParent : transform);
            parent.transform.localPosition = Vector3.zero;

            var cfg = GF.Config.GetTable<TbBuilding>()?.GetOrDefault(building.BuildingId);
            int baseSortOrder = GridUtils.GetSortingOrder(building.GridX, building.GridY) + 10;

            bool hasWorldSprite = cfg != null && !string.IsNullOrEmpty(cfg.WorldSprite);

            if (hasWorldSprite && building.SizeX == 1 && building.SizeY == 1)
            {
                var wp = GridUtils.GridToWorld(building.GridX, building.GridY);
                var tileGo = new GameObject("Sprite");
                tileGo.transform.SetParent(parent.transform);
                tileGo.transform.localPosition = new Vector3(wp.x, wp.y, 0);
                var sr = tileGo.AddComponent<SpriteRenderer>();
                sr.sortingOrder = baseSortOrder;
                sr.color = Color.white;
                sr.LoadSprite(cfg.WorldSprite);
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
                        sr.sortingOrder = baseSortOrder;
                        if (hasWorldSprite)
                        {
                            sr.color = Color.white;
                            sr.LoadSprite(cfg.WorldSprite);
                        }
                        else
                        {
                            sr.sprite = _grassSprite;
                            sr.color = buildingColor;
                        }
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

        private static Color GetBuildingColor(string category)
        {
            return category switch
            {
                "House" => new Color(0.8f, 0.55f, 0.35f),
                "Production" => new Color(0.75f, 0.7f, 0.5f),
                "Livestock" => new Color(0.6f, 0.8f, 0.5f),
                "Decoration" => new Color(0.75f, 0.6f, 0.8f),
                "Functional" => new Color(0.5f, 0.7f, 0.8f),
                _ => new Color(0.7f, 0.7f, 0.7f)
            };
        }

        #endregion

        private void OnDestroy()
        {
            foreach (var handle in _baseSpriteHandles)
                handle?.Dispose();
            _baseSpriteHandles.Clear();
        }
    }
}
