using System.Collections.Generic;
using JulyArch;

namespace SpiritHealer
{
    public enum DiagnosisMethod { Wang, Wen, Wen2, Qie }

    public class DiagnosisData
    {
        public Dictionary<DiagnosisMethod, int> MethodLevels = new()
        {
            { DiagnosisMethod.Wang, 1 },
            { DiagnosisMethod.Wen, 1 },
            { DiagnosisMethod.Wen2, 1 },
            { DiagnosisMethod.Qie, 1 },
        };

        public Dictionary<DiagnosisMethod, float> MethodExp = new()
        {
            { DiagnosisMethod.Wang, 0f },
            { DiagnosisMethod.Wen, 0f },
            { DiagnosisMethod.Wen2, 0f },
            { DiagnosisMethod.Qie, 0f },
        };
    }

    public interface IDiagnosisQueries : IStoreQueries
    {
        int GetMethodLevel(DiagnosisMethod method);
        float GetMethodExp(DiagnosisMethod method);
    }

    public class DiagnosisStore : StoreBase<DiagnosisData>, IDiagnosisQueries
    {
        public int GetMethodLevel(DiagnosisMethod method) => Data.MethodLevels[method];
        public float GetMethodExp(DiagnosisMethod method) => Data.MethodExp[method];

        public void AddExp(DiagnosisMethod method, float amount) => Data.MethodExp[method] += amount;

        public void SetLevel(DiagnosisMethod method, int level) => Data.MethodLevels[method] = level;
    }
}
