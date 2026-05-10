using UnityEngine;
using UnityEngine.UI;
using IsleWorks.Systems;
using IsleWorks.Stores;
using IsleWorks.Data;
using JulyArch;

namespace IsleWorks.Views
{
    /// <summary>
    /// 岛屿地图视图，负责显示地块并处理玩家交互。
    /// </summary>
    public class IslandMapView : GameView
    {
        [Inject] private GridStore _gridStore;
        [Inject] private IslandSystem _islandSystem;
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private Transform mapContainer;

        /// <summary>
        /// 初始化地图。
        /// </summary>
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

        /// <summary>
        /// 创建单个地块。
        /// </summary>
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

        /// <summary>
        /// 地块点击事件。
        /// </summary>
        private void OnTileClicked(int x, int y)
        {
            var tileType = _gridStore.GetTile(x, y);

            if (tileType == TileType.Locked)
            {
                int unlockedCount = CountUnlockedTiles();
                int cost = IslandPriceCalculator.GetTilePrice(unlockedCount);

                _islandSystem.UnlockTile(new Vector2Int(x, y), cost);
            }
        }

        /// <summary>
        /// 统计解锁的地块数量。
        /// </summary>
        private int CountUnlockedTiles()
        {
            int count = 0;
            for (int x = 0; x < _gridStore.Width; x++)
            {
                for (int y = 0; y < _gridStore.Height; y++)
                {
                    if (_gridStore.GetTile(x, y) != TileType.Locked)
                    {
                        count++;
                    }
                }
            }

            return count;
        }
    }
}