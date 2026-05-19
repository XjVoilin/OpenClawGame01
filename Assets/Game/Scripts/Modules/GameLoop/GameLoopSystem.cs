using cfg;
using JulyArch;
using JulyCore;

namespace SpiritHealer
{
    /// <summary>
    /// 核心循环编排系统。
    /// 串联 诊断→开方→结算 流程，管理游戏初始化和日循环。
    /// View 层通过调用此系统的公开方法驱动游戏进程。
    /// </summary>
    public class GameLoopSystem : GameSystemBase
    {
        private TimeSystem _timeSystem;
        private VisitorSystem _visitorSystem;
        private DiagnosisSystem _diagnosisSystem;
        private PrescriptionSystem _prescriptionSystem;
        private InventoryStore _inventoryStore;

        protected override void OnInitialize()
        {
            var arch = GetArchitecture();
            _timeSystem = arch.GetSystem<TimeSystem>();
            _visitorSystem = arch.GetSystem<VisitorSystem>();
            _diagnosisSystem = arch.GetSystem<DiagnosisSystem>();
            _prescriptionSystem = arch.GetSystem<PrescriptionSystem>();
            _inventoryStore = GetStore<InventoryStore>();

            SeedInitialInventory();
        }

        /// <summary>新游戏时给玩家基础药材库存。</summary>
        private void SeedInitialInventory()
        {
            if (_inventoryStore.Herbs.Count > 0) return;

            var herbs = CfgTable.Herb.DataList;
            foreach (var herb in herbs)
            {
                _inventoryStore.AddHerb(herb.Id, 1, 5);
            }
        }

        // --- 对外 API：View 层调用这些方法驱动流程 ---

        /// <summary>接诊下一位来客。</summary>
        public bool AcceptNextVisitor() => _visitorSystem.AcceptNextVisitor();

        /// <summary>对当前来客执行一次诊断。</summary>
        public DiagnosisResult Diagnose(DiagnosisMethod method) =>
            _diagnosisSystem.PerformDiagnosis(method);

        /// <summary>设置处方槽位。</summary>
        public void SetPrescriptionSlot(HerbRole role, int herbId, int quality) =>
            _prescriptionSystem.SetSlot(role, herbId, quality);

        /// <summary>清空处方。</summary>
        public void ClearPrescription() => _prescriptionSystem.ClearSlots();

        /// <summary>
        /// 开方并结算：计算疗效 → 发放奖励 → 送走来客。
        /// 返回疗效分数。
        /// </summary>
        public float PrescribeAndSettle()
        {
            var score = _prescriptionSystem.Prescribe();
            _visitorSystem.CompleteTreatment(score);
            return score;
        }

        /// <summary>送走当前来客（不治疗）。</summary>
        public void DismissCurrentVisitor() => _visitorSystem.DismissCurrentVisitor();

        /// <summary>结束当天，进入夜间结算并推进到次日。</summary>
        public void EndDay() => _timeSystem.EndDay();

        // --- 查询 API ---

        public bool IsOpen => _timeSystem.IsOpen;
        public int QueueCount => _visitorSystem is not null
            ? GetStore<VisitorStore>().WaitingQueue.Count : 0;
        public VisitorInstance CurrentVisitor => GetStore<VisitorStore>().CurrentVisitor;
    }
}
