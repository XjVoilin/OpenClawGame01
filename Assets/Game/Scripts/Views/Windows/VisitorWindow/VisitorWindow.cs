using System.Collections.Generic;
using cfg;
using JulyArch;
using JulyCore;
using TMPro;
using UnityEngine;

namespace SpiritHealer
{
    /// <summary>
    /// 来客面板：显示来客信息、四诊操作、已揭示症状，并提供开方入口。
    /// 接诊后由 GameHUD 打开，开方/送走后关闭。
    /// </summary>
    public class VisitorWindow : GameUIView
    {
        [Header("来客信息")]
        [SerializeField] private TextMeshProUGUI _visitorName;
        [SerializeField] private TextMeshProUGUI _visitorType;
        [SerializeField] private TextMeshProUGUI _causeHint;

        [Header("四诊按钮")]
        [SerializeField] private UISmartButton _wangBtn;
        [SerializeField] private UISmartButton _wenBtn;
        [SerializeField] private UISmartButton _wen2Btn;
        [SerializeField] private UISmartButton _qieBtn;

        [Header("四诊等级")]
        [SerializeField] private TextMeshProUGUI _wangLevel;
        [SerializeField] private TextMeshProUGUI _wenLevel;
        [SerializeField] private TextMeshProUGUI _wen2Level;
        [SerializeField] private TextMeshProUGUI _qieLevel;

        [Header("诊断结果")]
        [SerializeField] private TextMeshProUGUI _diagnosisResult;

        [Header("症状列表")]
        [SerializeField] private Transform _symptomListRoot;
        [SerializeField] private GameObject _symptomItemPrefab;

        [Header("操作")]
        [SerializeField] private UISmartButton _prescribeBtn;
        [SerializeField] private UISmartButton _dismissBtn;

        private GameLoopSystem _gameLoop;
        private DiagnosisSystem _diagnosisSystem;
        private VisitorStore _visitorStore;
        private DiagnosisStore _diagnosisStore;

        private readonly List<GameObject> _symptomItems = new();

        protected override void OnBeforeOpen()
        {
            _gameLoop = this.GetSystem<GameLoopSystem>();
            _diagnosisSystem = this.GetSystem<DiagnosisSystem>();
            _visitorStore = this.GetStore<VisitorStore>();
            _diagnosisStore = this.GetStore<DiagnosisStore>();

            _wangBtn.onClick.AddListener(() => OnDiagnose(DiagnosisMethod.Wang));
            _wenBtn.onClick.AddListener(() => OnDiagnose(DiagnosisMethod.Wen));
            _wen2Btn.onClick.AddListener(() => OnDiagnose(DiagnosisMethod.Wen2));
            _qieBtn.onClick.AddListener(() => OnDiagnose(DiagnosisMethod.Qie));
            _prescribeBtn.onClick.AddListener(OnPrescribe);
            _dismissBtn.onClick.AddListener(OnDismiss);

            RefreshVisitorInfo();
            RefreshDiagnosisLevels();
            RefreshSymptomList();
            ClearDiagnosisResult();
        }

        protected override void OnClose()
        {
            _wangBtn.onClick.RemoveAllListeners();
            _wenBtn.onClick.RemoveAllListeners();
            _wen2Btn.onClick.RemoveAllListeners();
            _qieBtn.onClick.RemoveAllListeners();
            _prescribeBtn.onClick.RemoveAllListeners();
            _dismissBtn.onClick.RemoveAllListeners();
            ClearSymptomItems();
        }

        private void RefreshVisitorInfo()
        {
            var visitor = _visitorStore.CurrentVisitor;
            if (visitor == null) return;

            if (_visitorName) _visitorName.text = visitor.Name;
            if (_visitorType) _visitorType.text = GetTypeName(visitor.Type);

            var cause = CfgTable.Cause.GetOrDefault(visitor.CauseId);
            if (_causeHint && cause != null) _causeHint.text = cause.Description;
        }

        private void RefreshDiagnosisLevels()
        {
            SetLevelText(_wangLevel, DiagnosisMethod.Wang, "望");
            SetLevelText(_wenLevel, DiagnosisMethod.Wen, "闻");
            SetLevelText(_wen2Level, DiagnosisMethod.Wen2, "问");
            SetLevelText(_qieLevel, DiagnosisMethod.Qie, "切");
        }

        private void SetLevelText(TextMeshProUGUI text, DiagnosisMethod method, string label)
        {
            if (!text) return;
            var level = _diagnosisStore.GetMethodLevel(method);
            text.text = $"{label} Lv.{level}";
        }

        private void OnDiagnose(DiagnosisMethod method)
        {
            var result = _gameLoop.Diagnose(method);

            if (_diagnosisResult)
            {
                if (result.Success && result.RevealedSymptoms.Count > 0)
                {
                    _diagnosisResult.text = $"<color=#4CAF50>诊断成功！揭示了 {result.RevealedSymptoms.Count} 条信息</color>";
                }
                else if (result.Success)
                {
                    _diagnosisResult.text = "<color=#FF9800>已无更多可揭示的症状</color>";
                }
                else
                {
                    _diagnosisResult.text = "<color=#F44336>诊断未能获取有效信息……</color>";
                }
            }

            RefreshDiagnosisLevels();
            RefreshSymptomList();
        }

        private void RefreshSymptomList()
        {
            ClearSymptomItems();

            var visitor = _visitorStore.CurrentVisitor;
            if (visitor == null) return;

            var symptoms = _diagnosisSystem.GetRevealedSymptoms(visitor);
            foreach (var symptom in symptoms)
            {
                if (!_symptomItemPrefab || !_symptomListRoot) continue;

                var go = Instantiate(_symptomItemPrefab, _symptomListRoot);
                go.SetActive(true);
                var text = go.GetComponentInChildren<TextMeshProUGUI>();
                if (text) text.text = $"[{GetMethodLabel(symptom.Method)}] {symptom.Content}";
                _symptomItems.Add(go);
            }

            _prescribeBtn.SetInteractable(symptoms.Count > 0);
        }

        private void ClearSymptomItems()
        {
            foreach (var go in _symptomItems) Destroy(go);
            _symptomItems.Clear();
        }

        private void ClearDiagnosisResult()
        {
            if (_diagnosisResult) _diagnosisResult.text = "";
        }

        private void OnPrescribe()
        {
            GF.UI.Open(UIWindowId.PrescriptionWindow);
        }

        private void OnDismiss()
        {
            _gameLoop.DismissCurrentVisitor();
            CloseWindow();
        }

        private static string GetTypeName(VisitorType t) => t switch
        {
            VisitorType.Commoner => "凡人",
            VisitorType.Wanderer => "散修",
            VisitorType.SectDisciple => "宗门弟子",
            VisitorType.Elder => "长老",
            VisitorType.Mysterious => "神秘来客",
            _ => "来客"
        };

        private static string GetMethodLabel(int method) => method switch
        {
            0 => "望",
            1 => "闻",
            2 => "问",
            3 => "切",
            _ => "?"
        };
    }
}
