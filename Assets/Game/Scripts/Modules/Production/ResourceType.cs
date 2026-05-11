namespace IsleWorks.Production
{
    public enum ResourceType
    {
        None = 0,
        // 基础资源
        Wood = 101,
        Ore = 102,
        Coal = 103,
        Water = 104,
        Oil = 105,
        // 初加工产品
        Plank = 201,
        Ingot = 202,
        Plastic = 203,
        // 中间件
        Tool = 301,
        CircuitBoard = 302,
        // 终端产品
        Automaton = 401,
    }
}
