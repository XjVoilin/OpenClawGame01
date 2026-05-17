using JulyArch;

namespace SpiritHealer
{
    /// <summary>
    /// 处方系统 —— 游戏的核心产出环节。
    /// 玩家根据诊断结果选药组方（君臣佐使），
    /// 疗效 = 辨证准确度 × 用药匹配度 × 药材品质系数。
    /// 管理处方记录和药理笔记的积累。
    /// </summary>
    public class PrescriptionSystem : GameSystemBase
    {
        protected override void OnInitialize()
        {
        }

        /// <summary>设置处方中某一位置（君/臣/佐/使）的药材。</summary>
        public void SetSlot(HerbRole role, int herbId, int quality)
        {
        }

        /// <summary>清空当前处方栏，重新组方。</summary>
        public void ClearSlots()
        {
        }

        /// <summary>
        /// 向来客开出处方，计算疗效分数。
        /// 90+ 药到病除，70-89 见效需复诊，50-69 微效，30-49 无效，&lt;30 反效。
        /// 消耗药材、记入药理笔记、影响声望。
        /// </summary>
        public float Prescribe(int visitorId)
        {
            return 0f;
        }

        /// <summary>将某条处方记录标记为"验方"，后续可快速调用。</summary>
        public void SaveAsVerified(int recordId)
        {
        }
    }
}
