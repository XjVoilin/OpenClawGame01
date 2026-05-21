using JulyCore;
using TMPro;
using UnityEngine;

namespace CozyYard
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UILocalizedText : MonoBehaviour
    {
        [SerializeField] private string _key;

        private TextMeshProUGUI _text;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            Refresh();
        }

        public void SetKey(string key)
        {
            _key = key;
            Refresh();
        }

        public void Refresh()
        {
            if (_text != null && !string.IsNullOrEmpty(_key))
                _text.text = GF.Localization.Get(_key);
        }
    }
}
