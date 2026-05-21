using System;
using TMPro;
using UnityEngine;

namespace CozyYard
{
    public class VisitorEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _infoText;
        [SerializeField] private UISmartButton _fulfillBtn;
        [SerializeField] private UISmartButton _dismissBtn;

        public void Setup(string info, Action onFulfill, Action onDismiss)
        {
            if (_infoText) _infoText.text = info;
            if (_fulfillBtn)
                _fulfillBtn.onClick.AddListener(() => onFulfill?.Invoke());
            if (_dismissBtn)
                _dismissBtn.onClick.AddListener(() => onDismiss?.Invoke());
        }

        public void Cleanup()
        {
            if (_fulfillBtn) _fulfillBtn.onClick.RemoveAllListeners();
            if (_dismissBtn) _dismissBtn.onClick.RemoveAllListeners();
        }
    }
}
