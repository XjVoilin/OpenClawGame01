using JulyArch;
using JulyCore.Provider.UI;

namespace SpiritHealer
{
    /// <summary>
    /// UI 面板 View 基类（盒子层）
    /// </summary>
    public abstract class GameUIView : UIBase, ICanEvent, ICanGetSystem, ICanGetStore
    {
        public IGameContext GetArchitecture() => AppArch.Context;

        public sealed override void Close()
        {
            base.Close();
            this.UnsubscribeAll();
        }
    }

}