using JulyArch;

namespace SpiritHealer
{
    /// <summary>
    /// 来客系统 —— 驱动游戏节奏的核心来源。
    /// 每日根据声望和季节生成来客队列（凡人/散修/宗门弟子/长老/神秘人），
    /// 管理接诊流程和治疗结果统计。
    /// 营业时间结束（进入傍晚）时，未接诊的访客自动离开（无惩罚，仅错过收益）。
    /// </summary>
    public class VisitorSystem : GameSystemBase
    {
        protected override void OnInitialize()
        {
            this.Subscribe<PhaseChangedEvent>(OnPhaseChanged);
        }

        private void OnPhaseChanged(PhaseChangedEvent e)
        {
            if (e.NewPhase == TimePhase.Morning)
            {
                GenerateDailyVisitors();
            }
            else if (e.NewPhase == TimePhase.Evening)
            {
                DismissRemainingVisitors();
            }
        }

        /// <summary>
        /// 每日清晨生成当天来客队列。
        /// 来客类型和病症由声望等级、季节、随机事件共同决定。
        /// </summary>
        public void GenerateDailyVisitors()
        {
        }

        /// <summary>从等候队列中接入下一位来客到诊台。</summary>
        public void AcceptVisitor()
        {
        }

        /// <summary>送走当前来客（治疗完成或主动拒诊）。</summary>
        public void DismissVisitor()
        {
        }

        /// <summary>营业时间结束时清空未接诊的等候队列。</summary>
        private void DismissRemainingVisitors()
        {
        }
    }
}
