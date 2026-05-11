using IsleWorks.Economy;
using IsleWorks.Grid;
using IsleWorks.Production;
using JulyArch;
using JulyCore;
using UnityEngine;

namespace IsleWorks.Views
{
    /// <summary>
    /// 网格视图 —— 显示 8x8 彩色方块网格，处理建造/拆除交互。
    /// </summary>
    public class GridView : GameView
    {
        public override IGameContext GetArchitecture() => AppArch.Context;

        private SpriteRenderer[,] _tileRenderers;
        private SpriteRenderer[,] _buildingRenderers;
        private int _width;
        private int _height;
        private int _selectedMachineType;

        protected override void OnEnable()
        {
            base.OnEnable();
            this.Subscribe<BuildingPlacedEvent>(OnBuildingPlaced);
            this.Subscribe<BuildingRemovedEvent>(OnBuildingRemoved);
        }

        protected override void OnDisable()
        {
            this.Unsubscribe<BuildingPlacedEvent>(OnBuildingPlaced);
            this.Unsubscribe<BuildingRemovedEvent>(OnBuildingRemoved);
            base.OnDisable();
        }

        public void Initialize()
        {
            var grid = this.Query<IGridQueries>();
            _width = grid.Width;
            _height = grid.Height;
            _tileRenderers = new SpriteRenderer[_width, _height];
            _buildingRenderers = new SpriteRenderer[_width, _height];

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    CreateTileObject(x, y);
                }
            }

            RefreshAllTiles();
            GF.Log("GridView initialized.");
        }

        public void SetSelectedMachineType(int machineTypeId)
        {
            _selectedMachineType = machineTypeId;
            GF.Log($"Selected machine type: {machineTypeId}");
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleLeftClick();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                HandleRightClick();
            }
        }

        private void HandleLeftClick()
        {
            var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int gx = Mathf.FloorToInt(worldPos.x);
            int gy = Mathf.FloorToInt(worldPos.y);

            var grid = this.Query<IGridQueries>();
            if (!grid.IsInBounds(gx, gy)) return;

            // Click on port tile to sell products
            if (grid.GetTile(gx, gy) == TileType.Port)
            {
                SellAtPort();
                return;
            }

            if (_selectedMachineType == 0) return;

            if (_selectedMachineType == (int)MachineType.Conveyor)
            {
                this.GetSystem<BuildSystem>().PlaceConveyor(new Vector2Int(gx, gy), Direction.Right);
            }
            else
            {
                this.GetSystem<BuildSystem>().PlaceBuilding(new Vector2Int(gx, gy), _selectedMachineType);
            }
        }

        private void HandleRightClick()
        {
            var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int gx = Mathf.FloorToInt(worldPos.x);
            int gy = Mathf.FloorToInt(worldPos.y);

            var grid = this.Query<IGridQueries>();
            if (!grid.IsInBounds(gx, gy)) return;

            this.GetSystem<BuildSystem>().RemoveBuilding(new Vector2Int(gx, gy));
        }

        private void CreateTileObject(int x, int y)
        {
            // Tile background
            var tileObj = new GameObject($"Tile_{x}_{y}");
            tileObj.transform.SetParent(transform);
            tileObj.transform.localPosition = new Vector3(x, y, 0);
            var sr = tileObj.AddComponent<SpriteRenderer>();
            sr.sprite = PlaceholderVisuals.GetSprite(Color.white);
            sr.sortingOrder = 0;
            _tileRenderers[x, y] = sr;

            // Building overlay
            var buildObj = new GameObject($"Building_{x}_{y}");
            buildObj.transform.SetParent(tileObj.transform);
            buildObj.transform.localPosition = Vector3.zero;
            var bsr = buildObj.AddComponent<SpriteRenderer>();
            bsr.sprite = PlaceholderVisuals.GetSprite(Color.white);
            bsr.sortingOrder = 1;
            bsr.enabled = false;
            _buildingRenderers[x, y] = bsr;
        }

        private void RefreshAllTiles()
        {
            var grid = this.Query<IGridQueries>();
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    RefreshTile(x, y, grid);
                }
            }
        }

        private void RefreshTile(int x, int y, IGridQueries grid)
        {
            var tileType = grid.GetTile(x, y);
            Color tileColor = tileType switch
            {
                TileType.Normal => PlaceholderVisuals.NormalTile,
                TileType.Port => PlaceholderVisuals.PortTile,
                TileType.Locked => PlaceholderVisuals.LockedTile,
                TileType.Water => PlaceholderVisuals.WaterTile,
                TileType.Mountain => PlaceholderVisuals.MountainTile,
                _ => PlaceholderVisuals.NormalTile
            };

            var resource = grid.GetResourceNode(x, y);
            if (resource != ResourceType.None)
            {
                tileColor = PlaceholderVisuals.GetResourceColor(resource);
            }

            _tileRenderers[x, y].color = tileColor;

            int buildingId = grid.GetBuilding(x, y);
            if (buildingId > 0)
            {
                var machine = grid.GetMachine(buildingId);
                if (machine != null)
                {
                    _buildingRenderers[x, y].enabled = true;
                    _buildingRenderers[x, y].color = PlaceholderVisuals.GetMachineColor(machine.MachineTypeId);
                }
                else
                {
                    var conv = grid.GetConveyor(buildingId);
                    if (conv != null)
                    {
                        _buildingRenderers[x, y].enabled = true;
                        _buildingRenderers[x, y].color = PlaceholderVisuals.ConveyorColor;
                    }
                    else
                    {
                        _buildingRenderers[x, y].enabled = false;
                    }
                }
            }
            else
            {
                _buildingRenderers[x, y].enabled = false;
            }
        }

        private void SellAtPort()
        {
            var inv = this.Query<IInventoryQueries>();
            if (inv.PortProducts.Count == 0)
            {
                GF.Log("Port has no products to sell.");
                return;
            }

            var products = new ResourceType[inv.PortProducts.Count];
            for (int i = 0; i < inv.PortProducts.Count; i++)
                products[i] = inv.PortProducts[i];

            this.GetSystem<EconomySystem>().SellAtPort(products);

            this.Mutate<InventoryStore>(store => store.ClearPortProducts());
        }

        private void OnBuildingPlaced(BuildingPlacedEvent e)
        {
            var grid = this.Query<IGridQueries>();
            RefreshTile(e.Position.x, e.Position.y, grid);
        }

        private void OnBuildingRemoved(BuildingRemovedEvent e)
        {
            var grid = this.Query<IGridQueries>();
            RefreshTile(e.Position.x, e.Position.y, grid);
        }
    }
}
