using JulyCore;
using TMPro;
using UnityEngine;

namespace SpiritHealer
{
    /// <summary>
    /// 治疗结算数据，由 PrescriptionWindow 传入。
    /// </summary>
    public class TreatmentResultData
    {
        public float Score;
        public int ReputationGained;
        public int CoinsGained;
    }

    /// <summary>
    /// 治疗结算弹窗：显示疗效分数、结果评级和奖励信息。
    /// 由 PrescriptionWindow 确认开方后打开，data 参数为 TreatmentResultData。
    /// </summary>
    public class TreatmentResultWindow : GameUIView
    {
        [Header("结果展示")]
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _gradeText;
        [SerializeField] private TextMeshProUGUI _descriptionText;

        [Header("奖励信息")]
        [SerializeField] private TextMeshProUGUI _rewardText;

        [Header("操作")]
        [SerializeField] private UISmartButton _confirmBtn;

        protected override void OnBeforeOpen()
        {
            _confirmBtn.onClick.AddListener(OnConfirm);

            var data = GetData<TreatmentResultData>();
            if (data != null) ShowResult(data);
        }

        protected override void OnClose()
        {
            _confirmBtn.onClick.RemoveAllListeners();
        }

        private void ShowResult(TreatmentResultData data)
        {
            if (_scoreText) _scoreText.text = $"{data.Score:F0}";

            var (grade, color, desc) = GetGradeInfo(data.Score);
            if (_gradeText)
            {
                _gradeText.text = grade;
                _gradeText.color = color;
            }
            if (_descriptionText) _descriptionText.text = desc;

            if (_rewardText)
            {
                var parts = new System.Collections.Generic.List<string>();
                if (data.ReputationGained > 0) parts.Add($"声望 +{data.ReputationGained}");
                if (data.CoinsGained > 0) parts.Add($"碎银 +{data.CoinsGained}");
                _rewardText.text = parts.Count > 0 ? string.Join("  ", parts) : "无奖励";
            }
        }

        private void OnConfirm()
        {
            GF.UI.Close(UIWindowId.VisitorWindow, true);
            CloseWindow();
        }

        private static (string grade, Color color, string desc) GetGradeInfo(float score) => score switch
        {
            >= 90f => ("药到病除", new Color(0.3f, 0.8f, 0.3f), "药方精准，一剂见效！来客感激涕零。"),
            >= 70f => ("见效", new Color(0.6f, 0.8f, 0.3f), "症状有所缓解，或需复诊调方。"),
            >= 50f => ("微效", new Color(1f, 0.75f, 0f), "略有改善但效果不佳，需要重新辨证。"),
            >= 30f => ("无效", new Color(0.8f, 0.4f, 0.2f), "药方未能奏效，白费了药材。"),
            _ => ("反效", new Color(0.9f, 0.2f, 0.2f), "用药失当，病情反而加重了……")
        };
    }
}
