using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CozyYard
{
    public class CraftEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Image _icon;
        [SerializeField] private UISmartButtonGray _craftBtn;

        public void Setup(string name, bool canCraft, Action onCraft, Sprite icon)
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
            if (_craftBtn)
            {
                _craftBtn.SetInteractable(canCraft);
                _craftBtn.onClick.AddListener(() => onCraft?.Invoke());
            }
        }

        public void Cleanup()
        {
            if (_craftBtn) _craftBtn.onClick.RemoveAllListeners();
        }
    }
}
