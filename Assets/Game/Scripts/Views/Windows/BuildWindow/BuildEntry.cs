using System;
using TMPro;
using UnityEngine;

namespace CozyYard
{
    public class BuildEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private UISmartButtonGray _buildBtn;

        public void Setup(string name, bool canBuild, Action onBuild)
        {
            if (_nameText) _nameText.text = name;
            if (_buildBtn)
            {
                _buildBtn.SetInteractable(canBuild);
                _buildBtn.onClick.AddListener(() => onBuild?.Invoke());
            }
        }

        public void Cleanup()
        {
            if (_buildBtn) _buildBtn.onClick.RemoveAllListeners();
        }
    }
}
