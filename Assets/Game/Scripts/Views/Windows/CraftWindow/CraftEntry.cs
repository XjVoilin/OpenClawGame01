using System;
using TMPro;
using UnityEngine;

namespace CozyYard
{
    public class CraftEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private UISmartButtonGray _craftBtn;

        public void Setup(string name, bool canCraft, Action onCraft)
        {
            if (_nameText) _nameText.text = name;
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
