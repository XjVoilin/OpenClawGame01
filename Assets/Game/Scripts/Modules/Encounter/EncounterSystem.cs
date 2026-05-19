using cfg;
using JulyArch;

namespace SpiritHealer
{
    /// <summary>
    /// 奇遇系统 —— 修仙世界的机缘与惊喜。
    /// 每日夜间结算时按条件概率判定是否触发奇遇事件（神秘来客、灵田异象、古方残页等），
    /// 奇遇可授予稀有种子、宝物、诊法加成或里程碑解锁。
    /// </summary>
    public class EncounterSystem : GameSystemBase
    {
        protected override void OnInitialize()
        {
            Subscribe<PhaseChangedEvent>(OnPhaseChanged);
        }

        private void OnPhaseChanged(PhaseChangedEvent e)
        {
            if (e.NewPhase == ETimePhase.Night)
            {
                CheckDailyEncounter();
            }
        }

        /// <summary>
        /// 夜间结算时调用，根据声望、季节、已完成里程碑等条件计算奇遇触发概率。
        /// 使用高品阶药材治病可额外提升当日触发概率。
        /// </summary>
        public void CheckDailyEncounter()
        {
        }

        /// <summary>触发指定奇遇事件，执行奖励发放和状态变更。</summary>
        public void TriggerEncounter(int encounterId)
        {
        }
    }
}