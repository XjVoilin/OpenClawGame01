using System.Collections.Generic;
using System.Linq;
using cfg;
using JulyArch;
using JulyCore;

namespace SpiritHealer
{
    public struct DiagnosisResult
    {
        public bool Success;
        public List<Symptom> RevealedSymptoms;
        public DiagnosisMethod Method;
    }

    /// <summary>
    /// 望闻问切系统。
    /// 对当前来客执行四诊，根据诊法等级掷骰揭示症状信息。
    /// 无论成败都积累经验，经验满自动升级。
    /// </summary>
    public class DiagnosisSystem : GameSystemBase
    {
        private const float BaseSuccessRate = 0.5f;
        private const float BonusPerLevel = 0.15f;
        private const float ExpPerDiagnosis = 30f;
        private const float ExpPerLevel = 100f;
        private const int MaxLevel = 5;

        private DiagnosisStore _store;
        private VisitorStore _visitorStore;

        protected override void OnInitialize()
        {
            _store = GetStore<DiagnosisStore>();
            _visitorStore = GetStore<VisitorStore>();
        }

        /// <summary>
        /// 对当前来客执行一次诊断。
        /// 成功则揭示对应诊法 + 等级范围内的症状，失败则无新信息。
        /// 无论结果都积累经验。
        /// </summary>
        public DiagnosisResult PerformDiagnosis(DiagnosisMethod method)
        {
            var visitor = _visitorStore.CurrentVisitor;
            var result = new DiagnosisResult
            {
                Method = method,
                Success = false,
                RevealedSymptoms = new List<Symptom>()
            };

            if (visitor == null) return result;

            var level = _store.GetMethodLevel(method);
            var methodInt = (int)method;

            var candidateSymptoms = CfgTable.Symptom.DataList
                .Where(s => s.CauseId == visitor.CauseId
                         && s.Method == methodInt
                         && s.Level <= level
                         && !visitor.RevealedSymptomIds.Contains(s.Id))
                .ToList();

            var successRate = BaseSuccessRate + level * BonusPerLevel;
            var roll = UnityEngine.Random.value;
            if (roll <= successRate && candidateSymptoms.Count > 0)
            {
                result.Success = true;
                foreach (var symptom in candidateSymptoms)
                {
                    visitor.RevealedSymptomIds.Add(symptom.Id);
                    result.RevealedSymptoms.Add(symptom);
                }
            }

            GainExp(method);
            return result;
        }

        public int GetMethodLevel(DiagnosisMethod method) => _store.GetMethodLevel(method);

        /// <summary>
        /// 获取来客已揭示的全部症状。
        /// </summary>
        public List<Symptom> GetRevealedSymptoms(VisitorInstance visitor)
        {
            if (visitor == null) return new List<Symptom>();
            return CfgTable.Symptom.DataList
                .Where(s => visitor.RevealedSymptomIds.Contains(s.Id))
                .ToList();
        }

        private void GainExp(DiagnosisMethod method)
        {
            var level = _store.GetMethodLevel(method);
            if (level >= MaxLevel) return;

            _store.AddExp(method, ExpPerDiagnosis);
            var exp = _store.GetMethodExp(method);
            var needed = level * ExpPerLevel;

            if (exp >= needed)
            {
                _store.AddExp(method, -needed);
                _store.SetLevel(method, level + 1);
            }
        }
    }
}
