using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpiritHealer
{
    /// <summary>
    /// 处方槽位 UI 组件（君/臣/佐/使之一）。
    /// 挂在 PrescriptionWindow 预制体的子物体上。
    /// </summary>
    public class PrescriptionSlotUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _roleLabel;
        [SerializeField] private TextMeshProUGUI _herbName;
        [SerializeField] private TextMeshProUGUI _qualityText;
        [SerializeField] private UISmartButton _slotBtn;
        [SerializeField] private UISmartButton _clearBtn;
        [SerializeField] private Image _highlight;

        private HerbRole _role;
        private Action<HerbRole> _onClicked;
        private Action<HerbRole> _onClear;

        public void Init(HerbRole role, string label, Action<HerbRole> onClicked, Action<HerbRole> onClear)
        {
            _role = role;
            _onClicked = onClicked;
            _onClear = onClear;

            if (_roleLabel) _roleLabel.text = label;
            if (_slotBtn) _slotBtn.onClick.AddListener(() => _onClicked?.Invoke(_role));
            if (_clearBtn) _clearBtn.onClick.AddListener(() => _onClear?.Invoke(_role));

            SetEmpty();
        }

        public void SetHerb(string herbName, int quality)
        {
            if (_herbName) _herbName.text = herbName;
            if (_qualityText) _qualityText.text = quality > 0 ? $"品质 {quality}" : "";
            if (_clearBtn) _clearBtn.gameObject.SetActive(true);
        }

        public void SetEmpty()
        {
            if (_herbName) _herbName.text = "空";
            if (_qualityText) _qualityText.text = "";
            if (_clearBtn) _clearBtn.gameObject.SetActive(false);
        }

        public void SetSelected(bool selected)
        {
            if (_highlight) _highlight.enabled = selected;
        }

        private void OnDestroy()
        {
            if (_slotBtn) _slotBtn.onClick.RemoveAllListeners();
            if (_clearBtn) _clearBtn.onClick.RemoveAllListeners();
        }
    }
}
