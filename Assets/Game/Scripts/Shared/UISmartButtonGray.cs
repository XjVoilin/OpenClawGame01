using JulyCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CozyYard
{
    /// <summary>
    /// 带置灰效果的按钮组件。不可交互时自动将 Image 和文本变为灰色。
    /// 挂载在与 UISmartButton 同一 GameObject 上。
    /// </summary>
    [RequireComponent(typeof(UISmartButton))]
    public class UISmartButtonGray : MonoBehaviour
    {
        private static readonly Color GrayImageColor = new(0.35f, 0.35f, 0.35f, 1f);
        private static readonly Color GrayTextColor = new(0.6f, 0.6f, 0.6f, 1f);

        private UISmartButton _button;
        private Image _image;
        private TextMeshProUGUI _label;

        private Color _normalImageColor;
        private Color _normalTextColor;
        private bool _initialized;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_initialized) return;
            _button = GetComponent<UISmartButton>();
            _image = GetComponent<Image>();
            _label = GetComponentInChildren<TextMeshProUGUI>();
            if (_image) _normalImageColor = _image.color;
            if (_label) _normalTextColor = _label.color;
            _initialized = true;
        }

        public UnityEngine.Events.UnityEvent onClick => _button.onClick;

        public void SetInteractable(bool interactable)
        {
            Initialize();
            _button.SetInteractable(interactable);
            ApplyVisual(!interactable);
        }

        private void ApplyVisual(bool gray)
        {
            if (_image) _image.color = gray ? GrayImageColor : _normalImageColor;
            if (_label) _label.color = gray ? GrayTextColor : _normalTextColor;
        }
    }
}
