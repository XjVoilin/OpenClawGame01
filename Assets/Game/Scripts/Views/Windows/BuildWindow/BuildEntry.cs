using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CozyYard
{
    public class BuildEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Image _icon;
        [SerializeField] private UISmartButtonGray _buildBtn;

        public void Setup(string name, bool canAfford, Action onBuild, Sprite icon)
        {
            if (_nameText) _nameText.text = name;
            if (_icon)
            {
                if (icon != null)
                {
                    _icon.sprite = icon;
                    _icon.color = Color.white;
                }
                _icon.enabled = icon != null;
            }
            if (_buildBtn)
            {
                _buildBtn.SetInteractable(canAfford);
                _buildBtn.onClick.AddListener(() => onBuild?.Invoke());
            }
        }

        public void Cleanup()
        {
            if (_buildBtn) _buildBtn.onClick.RemoveAllListeners();
        }
    }
}
