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

        public void Setup(string name, int price, bool canAfford, Action onBuy, string iconName)
        {
            if (_nameText) _nameText.text = name;
            if (_priceText) _priceText.text = $"{price}";
            _icon?.LoadSprite(iconName);
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
