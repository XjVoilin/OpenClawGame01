using System;
using JulyCore;
using TMPro;
using UnityEngine;

namespace CozyYard
{
    public class PhoneItemEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private UISmartButton _selectBtn;

        public void Setup(string displayText, Action onSelect)
        {
            if (_nameText) _nameText.text = displayText;
            if (_selectBtn)
                _selectBtn.onClick.AddListener(() => onSelect?.Invoke());
        }

        public void Cleanup()
        {
            if (_selectBtn) _selectBtn.onClick.RemoveAllListeners();
        }
    }
}
