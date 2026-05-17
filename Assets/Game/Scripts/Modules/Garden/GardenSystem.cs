using JulyArch;

namespace SpiritHealer
{
    /// <summary>
    /// 灵田系统 —— 管理药材种植的全生命周期。
    /// 植物生长在每日夜间结算时推进一个周期（受土质/灵气/五行/季节/相邻互作影响），
    /// 处理播种、浇灌、采收、灵田扩展与改造。
    /// </summary>
    public class GardenSystem : GameSystemBase
    {
        protected override void OnInitialize()
        {
            this.Subscribe<PhaseChangedEvent>(OnPhaseChanged);
        }

        private void OnPhaseChanged(PhaseChangedEvent e)
        {
            if (e.NewPhase == TimePhase.Night)
            {
                OnDayEnd();
            }
        }

        /// <summary>
        /// 夜间结算：推进所有地块上植物的生长进度，
        /// 计算灵气消耗、五行相生相克、虫害概率等。
        /// </summary>
        public void OnDayEnd()
        {
        }

        /// <summary>播种：将种子放入指定地块，校验土质和品阶要求。</summary>
        public void PlantSeed(int x, int y, int seedConfigId)
        {
        }

        /// <summary>采收：收获成熟药材，品质由生长条件综合决定（下品/中品/上品/极品）。</summary>
        public void Harvest(int x, int y)
        {
        }

        /// <summary>浇灌：调节地块湿度。</summary>
        public void Water(int x, int y)
        {
        }

        /// <summary>改造地块土质（凡土→灵土→沃灵土→仙壤），需满足里程碑条件。</summary>
        public void UpgradePlot(int x, int y, SoilType targetSoil)
        {
        }

        /// <summary>扩田：灵田网格从 3×3 扩展到 4×4、5×5……需声望里程碑。</summary>
        public void ExpandGarden()
        {
        }
    }
}
