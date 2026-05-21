using TMPro;
using UnityEngine;

namespace CozyYard
{
    public class MilestoneEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _infoText;

        public void Setup(string info)
        {
            if (_infoText) _infoText.text = info;
        }
    }
}
