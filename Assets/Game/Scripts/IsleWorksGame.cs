using JulyArch;

namespace IsleWorks
{
    /// <summary>
    /// 全局游戏上下文入口，供 View 等外部节点获取 Architecture 引用。
    /// </summary>
    public static class IsleWorksGame
    {
        public static GameContext Context { get; } = new GameContext();
    }
}
