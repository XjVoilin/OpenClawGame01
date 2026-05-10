using System;

namespace IsleWorks.Island
{
    /// <summary>
    /// 扩岛价格计算器，基于指数增长。
    /// </summary>
    public static class IslandPriceCalculator
    {
        private const int BasePrice = 500; // 初始价格
        private const double GrowthRate = 1.2; // 增长率

        /// <summary>
        /// 根据解锁的地块数量计算下一块地的价格。
        /// </summary>
        public static int GetTilePrice(int unlockedTileCount)
        {
            return (int)Math.Round(BasePrice * Math.Pow(GrowthRate, unlockedTileCount));
        }
    }
}