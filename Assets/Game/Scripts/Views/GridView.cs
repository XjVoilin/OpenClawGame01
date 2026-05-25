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
        private Sprite _emptyTileSprite;
        private Sprite _obstacleTileSprite;
        private Sprite _soilTileSprite;
        private Sprite _buildingTileSprite;
        private Sprite _highlightSprite;
        [SerializeField] private Transform _tilesParent;

        private static readonly Color ValidPlacementColor = new(0.2f, 0.9f, 0.2f, 0.6f);
        private static readonly Color InvalidPlacementColor = new(0.9f, 0.2f, 0.2f, 0.6f);
        private static readonly Color NormalHighlightColor = new(1f, 1f, 1f, 0.5f);

        private GridSystem _gridSystem;
        private FarmSystem _farmSystem;
        private BuildSystem _buildSystem;
        private SpriteRenderer[,] _tileRenderers;
        private GameObject _highlightObj;
        private SpriteRenderer _highlightRenderer;
        private readonly List<GameObject> _placementHighlights = new();
        private readonly Dictionary<int, GameObject> _buildingObjects = new();

        private readonly Dictionary<Vector2Int, SpriteRenderer> _cropRenderers = new();

        private static readonly Color CropSeedColor = new(0.4f, 0.55f, 0.3f, 0.8f);
        private static readonly Color CropSproutColor = new(0.3f, 0.7f, 0.3f, 0.9f);
        private static readonly Color CropGrowingColor = new(0.2f, 0.8f, 0.2f, 1f);
        private static readonly Color CropMatureColor = new(1f, 0.85f, 0.1f, 1f);
        private static readonly Color CropWitheredColor = new(0.4f, 0.3f, 0.2f, 0.8f);

        private bool _inPlantingMode;
        private int _plantingSeedItemId;
        private int _plantingCropId;

        private bool _inPlacementMode;
        private int _placementBuildingId;
        private int _placementSizeX;
        private int _placementSizeY;

        public override IArchContext GetArchitecture() => GameArch.Context;

        protected override void OnViewEnable()
        {
            _gridSystem = this.GetSystem<GridSystem>();
            _farmSystem = this.GetSystem<FarmSystem>();
            _buildSystem = this.GetSystem<BuildSystem>();
            this.Subscribe<GridCellChangedEvent>(OnCellChanged);
            this.Subscribe<BuildingPlacedEvent>(OnBuildingPlaced);
            this.Subscribe<BuildingRemovedEvent>(OnBuildingRemoved);
            this.Subscribe<EnterPlacementModeEvent>(OnEnterPlacementMode);
            this.Subscribe<CropPlantedEvent>(OnCropPlanted);
            this.Subscribe<CropGrowthEvent>(OnCropGrowth);
            this.Subscribe<CropWateredEvent>(OnCropWatered);
            this.Subscribe<CropReadyEvent>(OnCropReady);
            this.Subscribe<CropWitheredEvent>(OnCropWithered);
            this.Subscribe<CropHarvestedEvent>(OnCropHarvested);
            this.Subscribe<EnterPlantingModeEvent>(OnEnterPlantingMode);
            LoadAndRenderAsync().Forget();
        }

        private async UniTaskVoid LoadAndRenderAsync()
        {
            await LoadTileSpritesAsync();
            RenderGrid();
            RenderExistingBuildings();
            RenderExistingCrops();
            CreateHighlight();
        }

        private async UniTask LoadTileSpritesAsync()
        {
            _emptyTileSprite = await GF.Resource.LoadAsync<Sprite>("Tile_Empty");
            _obstacleTileSprite = await GF.Resource.LoadAsync<Sprite>("Tile_Obstacle");
            _soilTileSprite = await GF.Resource.LoadAsync<Sprite>("Tile_Soil");
            _buildingTileSprite = await GF.Resource.LoadAsync<Sprite>("Tile_Building");
            _highlightSprite = await GF.Resource.LoadAsync<Sprite>("Tile_Highlight");
        }

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
            this.GetArchitecture().Event.Publish(new PlacementCancelledEvent());
        }

        private void UpdatePlacementMode()
        {
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
                return;
            }

            if (Camera.main == null) return;

            var mouseScreen = Input.mousePosition;
            mouseScreen.z = -Camera.main.transform.position.z;
            var mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
            var gridPos = IsometricUtils.WorldToGrid(new Vector2(mouseWorld.x, mouseWorld.y));

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
                                var wp = IsometricUtils.GridToWorld(cx, cy);
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

        private static readonly Color PlantableColor = new(0.3f, 0.9f, 0.5f, 0.6f);

        private void UpdatePlantingMode()
        {
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                _inPlantingMode = false;
                _highlightObj.SetActive(false);
                return;
            }

            if (Camera.main == null) return;

            var mouseScreen = Input.mousePosition;
            mouseScreen.z = -Camera.main.transform.position.z;
            var mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
            var gridPos = IsometricUtils.WorldToGrid(new Vector2(mouseWorld.x, mouseWorld.y));

            if (_gridSystem.IsInBounds(gridPos.x, gridPos.y))
            {
                var cell = _gridSystem.GetCell(gridPos.x, gridPos.y);
                bool isSoilReady = cell != null && cell.State == CellState.Soil && _farmSystem.GetCropAt(gridPos.x, gridPos.y) == null;
                bool canTillAndPlant = cell != null && cell.State == CellState.Empty;
                bool canPlant = isSoilReady || canTillAndPlant;

                _highlightObj.SetActive(true);
                var worldPos = IsometricUtils.GridToWorld(gridPos.x, gridPos.y);
                _highlightObj.transform.localPosition = new Vector3(worldPos.x, worldPos.y, 0);
                _highlightRenderer.color = canPlant ? PlantableColor : InvalidPlacementColor;

                if (Input.GetMouseButtonDown(0) && canPlant)
                {
                    var invSystem = this.GetSystem<InventorySystem>();
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
            if (Camera.main == null) return;

            var mouseScreen = Input.mousePosition;
            mouseScreen.z = -Camera.main.transform.position.z;
            var mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
            var gridPos = IsometricUtils.WorldToGrid(new Vector2(mouseWorld.x, mouseWorld.y));

            if (_gridSystem.IsInBounds(gridPos.x, gridPos.y))
            {
                _highlightObj.SetActive(true);
                var worldPos = IsometricUtils.GridToWorld(gridPos.x, gridPos.y);
                _highlightObj.transform.localPosition = new Vector3(worldPos.x, worldPos.y, 0);
                _highlightRenderer.color = NormalHighlightColor;
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
                    if (cell.State == CellState.Unexplored)
                        sr.color = new Color(0.3f, 0.3f, 0.3f);

                    _tileRenderers[x, y] = sr;
                }
            }
        }

        private void CreateHighlight()
        {
            _highlightObj = new GameObject("Highlight");
            _highlightObj.transform.SetParent(transform);
            _highlightRenderer = _highlightObj.AddComponent<SpriteRenderer>();
            _highlightRenderer.sprite = _highlightSprite;
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
                sr.sprite = _highlightSprite;
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
            if (_tileRenderers != null 
                && evt.GridX >= 0 && evt.GridX < _tileRenderers.GetLength(0) 
                && evt.GridY >= 0 && evt.GridY < _tileRenderers.GetLength(1))
            {
                _tileRenderers[evt.GridX, evt.GridY].sprite = GetSpriteForState(evt.NewState);
            }
        }

        private Sprite GetSpriteForState(CellState state)
        {
            return state switch
            {
                CellState.Empty => _emptyTileSprite,
                CellState.Soil => _soilTileSprite != null ? _soilTileSprite : _emptyTileSprite,
                CellState.Obstacle => _obstacleTileSprite != null ? _obstacleTileSprite : _emptyTileSprite,
                CellState.Unexplored => _obstacleTileSprite != null ? _obstacleTileSprite : _emptyTileSprite,
                _ => _emptyTileSprite
            };
        }

        #endregion

        #region Crop Visuals

        private void RenderExistingCrops()
        {
            var farmStore = this.GetStore<FarmStore>();
            foreach (var crop in farmStore.Crops)
                CreateOrUpdateCropVisual(crop.GridX, crop.GridY, crop.Stage);
        }

        private void CreateOrUpdateCropVisual(int x, int y, CropGrowthStage stage)
        {
            var key = new Vector2Int(x, y);
            if (!_cropRenderers.TryGetValue(key, out var sr))
            {
                var worldPos = IsometricUtils.GridToWorld(x, y);
                var go = new GameObject($"Crop_{x}_{y}");
                go.transform.SetParent(_tilesParent != null ? _tilesParent : transform);
                go.transform.localPosition = new Vector3(worldPos.x, worldPos.y, 0);

                sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _soilTileSprite;
                sr.sortingOrder = IsometricUtils.GetSortingOrder(x, y) + 5;
                _cropRenderers[key] = sr;
            }

            float scale = stage switch
            {
                CropGrowthStage.Seed => 0.3f,
                CropGrowthStage.Sprout => 0.5f,
                CropGrowthStage.Growing => 0.7f,
                CropGrowthStage.Mature => 0.9f,
                CropGrowthStage.Withered => 0.6f,
                _ => 0.3f
            };

            sr.color = GetCropColor(stage);
            sr.transform.localScale = new Vector3(scale, scale, 1f);
            sr.gameObject.SetActive(true);
        }

        private void RemoveCropVisual(int x, int y)
        {
            var key = new Vector2Int(x, y);
            if (_cropRenderers.TryGetValue(key, out var sr))
            {
                if (sr != null && sr.gameObject != null)
                    Destroy(sr.gameObject);
                _cropRenderers.Remove(key);
            }
        }

        private static Color GetCropColor(CropGrowthStage stage)
        {
            return stage switch
            {
                CropGrowthStage.Seed => CropSeedColor,
                CropGrowthStage.Sprout => CropSproutColor,
                CropGrowthStage.Growing => CropGrowingColor,
                CropGrowthStage.Mature => CropMatureColor,
                CropGrowthStage.Withered => CropWitheredColor,
                _ => CropSeedColor
            };
        }

        private void OnCropPlanted(CropPlantedEvent evt)
        {
            CreateOrUpdateCropVisual(evt.GridX, evt.GridY, CropGrowthStage.Seed);
        }

        private void OnCropGrowth(CropGrowthEvent evt)
        {
            CreateOrUpdateCropVisual(evt.GridX, evt.GridY, evt.NewStage);
        }

        private void OnCropWatered(CropWateredEvent evt)
        {
            var key = new Vector2Int(evt.GridX, evt.GridY);
            if (_cropRenderers.TryGetValue(key, out var sr) && sr != null)
            {
                var c = sr.color;
                sr.color = new Color(c.r * 0.7f, c.g * 0.85f, c.b * 1.2f, c.a);
            }
        }

        private void OnCropReady(CropReadyEvent evt)
        {
            CreateOrUpdateCropVisual(evt.GridX, evt.GridY, CropGrowthStage.Mature);
        }

        private void OnCropWithered(CropWitheredEvent evt)
        {
            CreateOrUpdateCropVisual(evt.GridX, evt.GridY, CropGrowthStage.Withered);
        }

        private void OnCropHarvested(CropHarvestedEvent evt)
        {
            RemoveCropVisual(evt.GridX, evt.GridY);
        }

        #endregion

        #region Building Visuals

        private void OnBuildingPlaced(BuildingPlacedEvent evt)
        {
            var buildStore = this.GetStore<BuildStore>();
            var building = buildStore.GetBuildingAt(evt.GridX, evt.GridY);
            if (building != null)
                CreateBuildingVisual(building);
        }

        private void OnBuildingRemoved(BuildingRemovedEvent evt)
        {
            var toRemove = new List<int>();
            var buildStore = this.GetStore<BuildStore>();

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

            foreach (int uid in toRemove)
                _buildingObjects.Remove(uid);
        }

        private void RenderExistingBuildings()
        {
            var buildStore = this.GetStore<BuildStore>();
            foreach (var building in buildStore.Buildings)
                CreateBuildingVisual(building);
        }

        private void CreateBuildingVisual(BuildingInstance building)
        {
            if (_buildingObjects.ContainsKey(building.UniqueId)) return;

            var parent = new GameObject($"Building_{building.UniqueId}");
            parent.transform.SetParent(_tilesParent != null ? _tilesParent : transform);
            parent.transform.localPosition = Vector3.zero;

            int baseSortOrder = IsometricUtils.GetSortingOrder(building.GridX, building.GridY) + 10;
            Sprite tileSprite = _buildingTileSprite != null ? _buildingTileSprite : _emptyTileSprite;

            for (int dx = 0; dx < building.SizeX; dx++)
            {
                for (int dy = 0; dy < building.SizeY; dy++)
                {
                    var wp = IsometricUtils.GridToWorld(building.GridX + dx, building.GridY + dy);
                    var tileGo = new GameObject($"Tile_{dx}_{dy}");
                    tileGo.transform.SetParent(parent.transform);
                    tileGo.transform.localPosition = new Vector3(wp.x, wp.y, 0);

                    var sr = tileGo.AddComponent<SpriteRenderer>();
                    sr.sprite = tileSprite;
                    sr.sortingOrder = baseSortOrder;
                    sr.color = new Color(0.8f, 0.6f, 0.3f);
                }
            }

            var cfg = GF.Config.GetTable<TbBuilding>()?.GetOrDefault(building.BuildingId);
            float labelCenterX = building.GridX + (building.SizeX - 1) * 0.5f;
            float labelCenterY = building.GridY + (building.SizeY - 1) * 0.5f;
            var labelWorldPos = new Vector2(
                (labelCenterX - labelCenterY) * IsometricUtils.TileWidth * 0.5f,
                -(labelCenterX + labelCenterY) * IsometricUtils.TileHeight * 0.5f
            );

            float labelWidth = Mathf.Max(building.SizeX, building.SizeY) * IsometricUtils.TileWidth * 0.45f;
            float labelHeight = IsometricUtils.TileHeight * 0.4f;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(parent.transform);
            labelGo.transform.localPosition = new Vector3(labelWorldPos.x, labelWorldPos.y, 0);
            var rt = labelGo.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(labelWidth, labelHeight);

            var tmp = labelGo.AddComponent<TextMeshPro>();
            tmp.text = cfg != null ? GF.Localization.Get(cfg.NameKey) : $"#{building.BuildingId}";
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 1f;
            tmp.fontSizeMax = 4f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.sortingOrder = baseSortOrder + 1;

            _buildingObjects[building.UniqueId] = parent;
        }

        #endregion
    }
}
