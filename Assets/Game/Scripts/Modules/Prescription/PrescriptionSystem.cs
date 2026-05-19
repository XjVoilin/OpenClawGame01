using System;
using System.Linq;
using cfg;
using JulyArch;

namespace SpiritHealer
{
    /// <summary>
    /// 处方系统。
    /// 玩家根据诊断结果选药组方（君臣佐使），
    /// 疗效 = 君药匹配 + 臣药匹配 + 佐使基础分 + 品质加成。
    /// 开方后消耗药材、记录处方、揭示药材知识。
    /// </summary>
    public class PrescriptionSystem : GameSystemBase
    {
        private PrescriptionStore _store;
        private InventoryStore _inventoryStore;
        private VisitorStore _visitorStore;

        protected override void OnInitialize()
        {
            _store = GetStore<PrescriptionStore>();
            _inventoryStore = GetStore<InventoryStore>();
            _visitorStore = GetStore<VisitorStore>();
        }

        public void SetSlot(HerbRole role, int herbId, int quality)
        {
            _store.SetSlot(role, herbId, quality);
        }

        public void ClearSlots()
        {
            _store.ClearSlots();
        }

        /// <summary>
        /// 对当前来客开方。计算疗效、消耗药材、记入处方记录、揭示药材知识。
        /// 返回疗效分数 0-100。
        /// </summary>
        public float Prescribe()
        {
            var visitor = _visitorStore.CurrentVisitor;
            if (visitor == null) return 0f;

            var cause = CfgTable.Cause.GetOrDefault(visitor.CauseId);
            if (cause == null) return 0f;

            var score = CalculateEfficacy(cause);

            ConsumeHerbs();
            RevealKnowledgeForUsedHerbs();

            var record = new PrescriptionRecord
            {
                Id = _store.AllocateRecordId(),
                CauseId = visitor.CauseId,
                Slots = _store.CurrentSlots.Select(s =>
                    new PrescriptionSlot { Role = s.Role, HerbId = s.HerbId, Quality = s.Quality }).ToList(),
                EfficacyScore = score
            };
            _store.AddRecord(record);
            _store.ClearSlots();

            return score;
        }

        public void SaveAsVerified(int recordId)
        {
            var record = _store.GetRecord(recordId);
            if (record != null) record.IsVerified = true;
        }

        private float CalculateEfficacy(Cause cause)
        {
            float score = 0f;

            var jun = _store.GetSlot(HerbRole.Jun);
            var chen = _store.GetSlot(HerbRole.Chen);
            var zuo = _store.GetSlot(HerbRole.Zuo);
            var shi = _store.GetSlot(HerbRole.Shi);

            if (jun != null)
                score += cause.JunHerbIds.Contains(jun.HerbId) ? 40f : 10f;

            if (chen != null)
                score += cause.ChenHerbIds.Contains(chen.HerbId) ? 25f : 5f;

            if (zuo != null)
                score += 15f;

            // 甘草做使药加满分
            if (shi != null)
                score += shi.HerbId == 1 ? 10f : 5f;

            float qualityBonus = 0f;
            foreach (var slot in _store.CurrentSlots)
            {
                qualityBonus += (slot.Quality - 1) * 3f;
            }
            score += qualityBonus;

            return Math.Clamp(score, 0f, 100f);
        }

        private void ConsumeHerbs()
        {
            foreach (var slot in _store.CurrentSlots)
            {
                if (slot.HerbId > 0)
                    _inventoryStore.RemoveHerb(slot.HerbId, slot.Quality, 1);
            }
        }

        private void RevealKnowledgeForUsedHerbs()
        {
            foreach (var slot in _store.CurrentSlots)
            {
                if (slot.HerbId <= 0) continue;
                var k = _store.GetOrCreateKnowledge(slot.HerbId);
                if (!k.KnowsNature) { k.KnowsNature = true; continue; }
                if (!k.KnowsFlavor) { k.KnowsFlavor = true; continue; }
                if (!k.KnowsMeridian) { k.KnowsMeridian = true; continue; }
                if (!k.KnowsToxicity) { k.KnowsToxicity = true; }
            }
        }
    }
}
