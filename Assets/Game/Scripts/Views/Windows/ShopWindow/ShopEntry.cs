using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CozyYard
{
    public class ShopEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private Image _icon;
        [SerializeField] private UISmartButtonGray _buyBtn;

        public void Setup(string name, int price, bool canAfford, Action onBuy, Sprite icon)
        {
            if (_nameText) _nameText.text = name;
            if (_priceText) _priceText.text = $"{price}";
            if (_icon)
            {
                if (icon != null)
                {
                    _icon.sprite = icon;
                    _icon.color = Color.white;
                }
                _icon.enabled = icon != null;
            }
            if (_buyBtn)
            {
                _buyBtn.SetInteractable(canAfford);
                _buyBtn.onClick.AddListener(() => onBuy?.Invoke());
            }
        }

        public void Cleanup()
        {
            if (_buyBtn) _buyBtn.onClick.RemoveAllListeners();
        }
    }
}
