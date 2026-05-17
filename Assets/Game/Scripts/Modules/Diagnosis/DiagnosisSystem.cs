using JulyArch;

namespace SpiritHealer
{
    /// <summary>
    /// 望闻问切系统 —— 游戏的统一交互语言。
    /// 对病人、药草、矿石、环境等一切对象执行四诊（望/闻/问/切），
    /// 根据诊法等级和成功率掷骰揭示信息，误判会产生后果。
    /// </summary>
    public class DiagnosisSystem : GameSystemBase
    {
        protected override void OnInitialize()
        {
        }

        /// <summary>
        /// 对目标执行一次诊断。
        /// 根据诊法等级 + 成功率掷骰 → 成功则揭示对应层级信息，失败则返回模糊/错误信息。
        /// 无论成败都积累该诊法经验。
        /// </summary>
        public void PerformDiagnosis(DiagnosisMethod method, int targetId)
        {
        }
    }
}
