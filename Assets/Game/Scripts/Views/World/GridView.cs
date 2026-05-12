using IsleWorks.Economy;
using IsleWorks.Grid;
using IsleWorks.Production;
using JulyArch;
using JulyCore;
using UnityEngine;

namespace IsleWorks.Views
{
    /// <summary>
    /// 网格世界视图 —— 渲染地块和建筑，响应建造/拆除事件刷新显示。
    /// 输入处理通过 System 执行命令，机器选择通过事件接收。
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
            this.Subscribe<MachineSelectedEvent>(OnMachineSelected);
        }

        protected override void OnDisable()
        {
            this.Unsubscribe<BuildingPlacedEvent>(OnBuildingPlaced);
            this.Unsubscribe<BuildingRemovedEvent>(OnBuildingRemoved);
            this.Unsubscribe<MachineSelectedEvent>(OnMachineSelected);
            base.OnDisable();
        }

        private void Start()
        {
            var grid = this.Query<IGridQueries>();
            _width = grid.Width;
            _height = grid.Height;
            _tileRenderers = new SpriteRenderer[_width, _height];
            _buildingRenderers = new SpriteRenderer[_width, _height];

            for (int x = 0; x < _width; x++)
            for (int y = 0; y < _height; y++)
                CreateTileObject(x, y);

            RefreshAllTiles();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                HandleLeftClick();
            else if (Input.GetMouseButtonDown(1))
                HandleRightClick();
        }

        private void HandleLeftClick()
        {
            if (!TryGetGridPosition(out int gx, out int gy)) return;

            var grid = this.Query<IGridQueries>();
            if (grid.GetTile(gx, gy) == TileType.Port)
            {
                this.GetSystem<EconomySystem>().SellAllPortProducts();
                return;
            }

            if (_selectedMachineType == 0) return;

            var buildSystem = this.GetSystem<BuildSystem>();
            var pos = new Vector2Int(gx, gy);

            if (_selectedMachineType == (int)MachineType.Conveyor)
                buildSystem.PlaceConveyor(pos, Direction.Right);
            else
                buildSystem.PlaceBuilding(pos, _selectedMachineType);
        }

        private void HandleRightClick()
        {
            if (!TryGetGridPosition(out int gx, out int gy)) return;
            this.GetSystem<BuildSystem>().RemoveBuilding(new Vector2Int(gx, gy));
        }

        private bool TryGetGridPosition(out int gx, out int gy)
        {
            var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            gx = Mathf.FloorToInt(worldPos.x);
            gy = Mathf.FloorToInt(worldPos.y);
            return this.Query<IGridQueries>().IsInBounds(gx, gy);
        }

        private void CreateTileObject(int x, int y)
        {
            var tileObj = new GameObject($"Tile_{x}_{y}");
            tileObj.transform.SetParent(transform);
            tileObj.transform.localPosition = new Vector3(x, y, 0);

            var sr = tileObj.AddComponent<SpriteRenderer>();
            sr.sprite = PlaceholderVisuals.GetSprite(Color.white);
            sr.sortingOrder = 0;
            _tileRenderers[x, y] = sr;

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
            for (int y = 0; y < _height; y++)
                RefreshTile(x, y, grid);
        }

        private void RefreshTile(int x, int y, IGridQueries grid)
        {
            Color tileColor = grid.GetTile(x, y) switch
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
                tileColor = PlaceholderVisuals.GetResourceColor(resource);

            _tileRenderers[x, y].color = tileColor;

            int buildingId = grid.GetBuilding(x, y);
            if (buildingId <= 0)
            {
                _buildingRenderers[x, y].enabled = false;
                return;
            }

            var machine = grid.GetMachine(buildingId);
            if (machine != null)
            {
                _buildingRenderers[x, y].enabled = true;
                _buildingRenderers[x, y].color = PlaceholderVisuals.GetMachineColor(machine.MachineTypeId);
                return;
            }

            var conv = grid.GetConveyor(buildingId);
            if (conv != null)
            {
                _buildingRenderers[x, y].enabled = true;
                _buildingRenderers[x, y].color = PlaceholderVisuals.ConveyorColor;
                return;
            }

            _buildingRenderers[x, y].enabled = false;
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

        private void OnMachineSelected(MachineSelectedEvent e)
        {
            _selectedMachineType = e.MachineTypeId;
        }
    }
}
