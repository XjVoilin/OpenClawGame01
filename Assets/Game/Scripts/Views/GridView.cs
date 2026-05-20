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
        private FarmSystem _farmSystem;
        private SpriteRenderer[,] _tileRenderers;
        private GameObject _highlightObj;

        public override IGameContext GetArchitecture() => AppArch.Context;

        protected override void OnViewEnable()
        {
            _gridSystem = this.GetSystem<GridSystem>();
            _farmSystem = this.GetSystem<FarmSystem>();
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
            if (Camera.main == null) return;

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
    }
}
