using System;
using cfg;
using TMPro;
using UnityEngine;

namespace SpiritHealer
{
    /// <summary>
    /// 药材列表项 UI 组件。
    /// 挂在 PrescriptionWindow 的 HerbItem 预制体上，显示药材信息供选择。
    /// </summary>
    public class HerbItemUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _countText;
        [SerializeField] private TextMeshProUGUI _infoText;
        [SerializeField] private UISmartButton _selectBtn;

        private int _herbId;
        private int _quality;
        private Action<int, int> _onSelected;

        public void Init(Herb herb, int quality, int count, HerbKnowledge knowledge, Action<int, int> onSelected)
        {
            _herbId = herb.Id;
            _quality = quality;
            _onSelected = onSelected;

            if (_nameText) _nameText.text = herb.Name;
            if (_countText) _countText.text = $"x{count}";
            if (_infoText) _infoText.text = BuildInfoText(herb, knowledge);

            if (_selectBtn) _selectBtn.onClick.AddListener(OnClick);
        }

        private static string BuildInfoText(Herb herb, HerbKnowledge k)
        {
            if (k == null) return "未知";

            var parts = new System.Collections.Generic.List<string>();
            if (k.KnowsNature) parts.Add(herb.Nature.ToString());
            if (k.KnowsFlavor) parts.Add(herb.Flavor.ToString());
            if (k.KnowsMeridian) parts.Add(herb.Meridian);
            if (k.KnowsToxicity) parts.Add(herb.Toxicity > 0 ? $"毒性{herb.Toxicity}" : "无毒");

            return parts.Count > 0 ? string.Join(" / ", parts) : "未知";
        }

        private void OnClick()
        {
            _onSelected?.Invoke(_herbId, _quality);
        }

        private void OnDestroy()
        {
            if (_selectBtn) _selectBtn.onClick.RemoveAllListeners();
        }
    }
}
