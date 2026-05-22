using System.Collections.Generic;
using cfg;
using JulyArch;
using JulyCore;
using JulyGame;
using TMPro;
using UnityEngine;

namespace CozyYard
{
    public class GridView : GameView
    {
        [SerializeField] private Sprite _emptyTileSprite;
        [SerializeField] private Sprite _obstacleTileSprite;
        [SerializeField] private Sprite _soilTileSprite;
        [SerializeField] private Sprite _buildingTileSprite;
        [SerializeField] private Sprite _highlightSprite;
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
        private readonly Dictionary<int, GameObject> _buildingObjects = new();

        private bool _inPlacementMode;
        private int _placementBuildingId;

        public override IArchContext GetArchitecture() => GameArch.Context;

        protected override void OnViewEnable()
        {
            EnsurePlaceholderSprites();
            _gridSystem = this.GetSystem<GridSystem>();
            _farmSystem = this.GetSystem<FarmSystem>();
            _buildSystem = this.GetSystem<BuildSystem>();
            this.Subscribe<GridCellChangedEvent>(OnCellChanged);
            this.Subscribe<BuildingPlacedEvent>(OnBuildingPlaced);
            this.Subscribe<BuildingRemovedEvent>(OnBuildingRemoved);
            this.Subscribe<EnterPlacementModeEvent>(OnEnterPlacementMode);
            RenderGrid();
            RenderExistingBuildings();
            CreateHighlight();
        }

        private void EnsurePlaceholderSprites()
        {
            if (_emptyTileSprite == null)
                _emptyTileSprite = CreateColoredSprite(new Color(0.6f, 0.85f, 0.45f));
            if (_obstacleTileSprite == null)
                _obstacleTileSprite = CreateColoredSprite(new Color(0.4f, 0.3f, 0.25f));
            if (_soilTileSprite == null)
                _soilTileSprite = CreateColoredSprite(new Color(0.55f, 0.35f, 0.2f));
            if (_buildingTileSprite == null)
                _buildingTileSprite = CreateColoredSprite(new Color(0.85f, 0.65f, 0.3f));
            if (_highlightSprite == null)
                _highlightSprite = CreateColoredSprite(Color.white);
        }

        private static Sprite CreateColoredSprite(Color color)
        {
            const int width = 64;
            const int height = 32;
            var tex = new Texture2D(width, height);
            var pixels = new Color[width * height];
            
            float halfW = width * 0.5f;
            float halfH = height * 0.5f;
            for (int py = 0; py < height; py++)
            {
                for (int px = 0; px < width; px++)
                {
                    float nx = Mathf.Abs(px - halfW + 0.5f) / halfW;
                    float ny = Mathf.Abs(py - halfH + 0.5f) / halfH;
                    pixels[py * width + px] = (nx + ny <= 1f) ? color : Color.clear;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            tex.filterMode = FilterMode.Point;
            float ppu = width / IsometricUtils.TileWidth;
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), ppu);
        }

        private void Update()
        {
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
        }

        private void CancelPlacement()
        {
            _inPlacementMode = false;
            _placementBuildingId = 0;
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

            var mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var gridPos = IsometricUtils.WorldToGrid(new Vector2(mouseWorld.x, mouseWorld.y));

            if (_gridSystem.IsInBounds(gridPos.x, gridPos.y))
            {
                _highlightObj.SetActive(true);
                var worldPos = IsometricUtils.GridToWorld(gridPos.x, gridPos.y);
                _highlightObj.transform.localPosition = new Vector3(worldPos.x, worldPos.y, 0);

                bool canPlace = _buildSystem.CanBuild(_placementBuildingId, gridPos.x, gridPos.y);
                _highlightRenderer.color = canPlace ? ValidPlacementColor : InvalidPlacementColor;

                if (Input.GetMouseButtonDown(0) && canPlace)
                {
                    _buildSystem.Build(_placementBuildingId, gridPos.x, gridPos.y);
                    _inPlacementMode = false;
                    _placementBuildingId = 0;
                    _highlightRenderer.color = NormalHighlightColor;
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

            var mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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

            float centerX = building.GridX + (building.SizeX - 1) * 0.5f;
            float centerY = building.GridY + (building.SizeY - 1) * 0.5f;
            var worldPos = IsometricUtils.GridToWorld((int)centerX, (int)centerY);

            var go = new GameObject($"Building_{building.UniqueId}");
            go.transform.SetParent(_tilesParent != null ? _tilesParent : transform);
            go.transform.localPosition = new Vector3(worldPos.x, worldPos.y, 0);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _buildingTileSprite != null ? _buildingTileSprite : _emptyTileSprite;
            sr.sortingOrder = IsometricUtils.GetSortingOrder(building.GridX, building.GridY) + 10;
            sr.color = new Color(0.8f, 0.6f, 0.3f);

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform);
            labelGo.transform.localPosition = new Vector3(0, 0.3f, 0);
            var tmp = labelGo.AddComponent<TextMeshPro>();
            var cfg = GF.Config.GetTable<TbBuilding>()?.GetOrDefault(building.BuildingId);
            tmp.text = cfg != null ? GF.Localization.Get(cfg.NameKey) : $"#{building.BuildingId}";
            tmp.fontSize = 3;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.sortingOrder = sr.sortingOrder + 1;

            _buildingObjects[building.UniqueId] = go;
        }

        #endregion
    }
}
