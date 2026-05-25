using System;
using TMPro;
using UnityEngine;

namespace CozyYard
{
    public class ShopEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private UISmartButtonGray _buyBtn;

        public void Setup(string name, int price, bool canAfford, Action onBuy)
        {
            if (_nameText) _nameText.text = name;
            if (_priceText) _priceText.text = $"{price}";
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
