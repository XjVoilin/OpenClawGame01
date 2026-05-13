using JulyArch;
using JulyCore.Provider.UI;

namespace OffTrail
{
    /// <summary>
    /// UI 面板 View 基类（盒子层）
    /// </summary>
    public abstract class GameUIView : UIBase, IAppArch
    {
        public IGameContext GetArchitecture() => AppArch.Context;

        public sealed override void Close()
        {
            base.Close();
            this.UnsubscribeAll();
        }
    }
}
