namespace CozyYard
{
    /// <summary>四季循环</summary>
    public enum Season
    {
        Spring = 0,  // 春
        Summer = 1,  // 夏
        Autumn = 2,  // 秋
        Winter = 3   // 冬
    }

    /// <summary>一天中的时段划分</summary>
    public enum TimePhase
    {
        Dawn,       // 黎明
        Morning,    // 上午
        Noon,       // 正午
        Afternoon,  // 下午
        Evening,    // 傍晚
        Night       // 夜晚
    }

    /// <summary>网格单元格状态</summary>
    public enum CellState
    {
        Unexplored, // 未探索（迷雾区域）
        Obstacle,   // 障碍物占据
        Empty,      // 空地，可放置
        Soil,       // 耕地，可种植
        Water,      // 水域
        Paved       // 铺设道路/地板
    }

    /// <summary>建筑分类</summary>
    public enum BuildingCategory
    {
        House,      // 住宅
        Production, // 生产设施
        Livestock,  // 畜牧设施
        Functional, // 功能性建筑
        Decoration  // 装饰物
    }

    /// <summary>动物类型</summary>
    public enum AnimalType
    {
        Poultry, // 家禽
        Aquatic, // 水产
        Pet      // 宠物
    }

    /// <summary>物品类型</summary>
    public enum ItemType
    {
        Material, // 原材料
        Seed,     // 种子
        Product,  // 加工产品
        Tool      // 工具
    }

    /// <summary>天气类型</summary>
    public enum WeatherType
    {
        Sunny,     // 晴天
        Cloudy,    // 多云
        LightRain, // 小雨
        HeavyRain, // 大雨
        Windy      // 大风
    }
}
