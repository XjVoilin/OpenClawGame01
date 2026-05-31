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

        public void Setup(string name, bool canCraft, Action onCraft, string iconName)
        {
            if (_nameText) _nameText.text = name;
            _icon.LoadSprite(iconName);
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
