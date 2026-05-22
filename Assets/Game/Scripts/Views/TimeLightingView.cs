using JulyArch;
using JulyGame;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CozyYard
{
    public class TimeLightingView : GameView
    {
        [SerializeField] private Light2D _globalLight;

        [Header("Light Config")]
        [SerializeField] private Color _dawnColor = new Color(1f, 0.83f, 0.63f);
        [SerializeField] private Color _dayColor = Color.white;
        [SerializeField] private Color _eveningColor = new Color(1f, 0.56f, 0.25f);
        [SerializeField] private Color _nightColor = new Color(0.25f, 0.38f, 0.63f);

        [SerializeField] private float _dawnIntensity = 0.4f;
        [SerializeField] private float _dayIntensity = 1.0f;
        [SerializeField] private float _eveningIntensity = 0.5f;
        [SerializeField] private float _nightIntensity = 0.2f;

        [SerializeField] private float _transitionSpeed = 2f;

        private Color _targetColor;
        private float _targetIntensity;

        public override IArchContext GetArchitecture() => GameArch.Context;

        protected override void OnViewEnable()
        {
            this.Subscribe<PhaseChangedEvent>(OnPhaseChanged);
            UpdateTargetFromCurrentPhase();
            ApplyImmediate();
        }

        private void Update()
        {
            if (_globalLight == null) return;
            _globalLight.color = Color.Lerp(_globalLight.color, _targetColor, Time.deltaTime * _transitionSpeed);
            _globalLight.intensity = Mathf.Lerp(_globalLight.intensity, _targetIntensity, Time.deltaTime * _transitionSpeed);
        }

        private void OnPhaseChanged(PhaseChangedEvent e)
        {
            UpdateTargetForPhase(e.NewPhase);
        }

        private void UpdateTargetFromCurrentPhase()
        {
            var q = this.GetStore<TimeStore>();
            UpdateTargetForPhase(q.CurrentPhase);
        }

        private void UpdateTargetForPhase(TimePhase phase)
        {
            switch (phase)
            {
                case TimePhase.Dawn:
                    _targetColor = _dawnColor;
                    _targetIntensity = _dawnIntensity;
                    break;
                case TimePhase.Morning:
                case TimePhase.Noon:
                case TimePhase.Afternoon:
                    _targetColor = _dayColor;
                    _targetIntensity = _dayIntensity;
                    break;
                case TimePhase.Evening:
                    _targetColor = _eveningColor;
                    _targetIntensity = _eveningIntensity;
                    break;
                case TimePhase.Night:
                    _targetColor = _nightColor;
                    _targetIntensity = _nightIntensity;
                    break;
            }
        }

        private void ApplyImmediate()
        {
            if (_globalLight == null) return;
            _globalLight.color = _targetColor;
            _globalLight.intensity = _targetIntensity;
        }
    }
}
