using UnityEngine;
using UnityEngine.UI;
using IsleWorks.Grid;
using IsleWorks.Island;
using JulyArch;

namespace IsleWorks.Views
{
    /// <summary>
    /// 岛屿地图视图，负责显示地块并处理玩家交互。
    /// </summary>
    public class IslandMapView : GameView
    {
        public override IGameContext GetArchitecture() => IsleWorksGame.Context;

        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private Transform mapContainer;

        public void Initialize(int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    CreateTile(x, y);
                }
            }

            Debug.Log("Island map initialized.");
        }

        private void CreateTile(int x, int y)
        {
            var tileObject = Instantiate(tilePrefab, mapContainer);
            tileObject.transform.localPosition = new Vector3(x, y, 0);

            var button = tileObject.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnTileClicked(x, y));
            }
        }

        private void OnTileClicked(int x, int y)
        {
            var grid = this.Query<IGridQueries>();
            var tileType = grid.GetTile(x, y);

            if (tileType == TileType.Locked)
            {
                int unlockedCount = CountUnlockedTiles();
                int cost = IslandPriceCalculator.GetTilePrice(unlockedCount);
                this.GetSystem<IslandSystem>().UnlockTile(new Vector2Int(x, y), cost);
            }
        }

        private int CountUnlockedTiles()
        {
            var grid = this.Query<IGridQueries>();
            int count = 0;
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    if (grid.GetTile(x, y) != TileType.Locked)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
    }
}
