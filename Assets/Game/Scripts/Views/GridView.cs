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
            LoadAndRenderAsync().Forget();
        }

        private async UniTaskVoid LoadAndRenderAsync()
        {
            await LoadTileSpritesAsync();
            RenderGrid();
            RenderExistingBuildings();
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
            else
                UpdateNormalMode();
        }

        #region Placement Mode

        private void OnEnterPlacementMode(EnterPlacementModeEvent evt)
        {
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
