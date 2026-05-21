using TMPro;
using UnityEngine;

namespace CozyYard
{
    public class InventorySlotEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;

        public void Setup(string displayText)
        {
            if (_nameText) _nameText.text = displayText;
        }
    }
}
