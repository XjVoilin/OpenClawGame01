using JulyArch;
using OffTrail.Survival;
using OffTrail.World;
using TMPro;
using UnityEngine;

namespace OffTrail
{
    public class SurvivalHUD : GameUIView
    {
        [SerializeField] private TextMeshProUGUI _hungerText;
        [SerializeField] private TextMeshProUGUI _thirstText;
        [SerializeField] private TextMeshProUGUI _temperatureText;
        [SerializeField] private TextMeshProUGUI _staminaText;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private TextMeshProUGUI _dayText;

        protected override void OnBeforeOpen()
        {
            this.Subscribe<SurvivalStatChanged>(_ => RefreshStats());
            this.Subscribe<DayChanged>(_ => RefreshTime());

            RefreshStats();
            RefreshTime();
            base.OnBeforeOpen();
        }

        private void RefreshStats()
        {
            var stats = this.Query<ISurvivalQueries>();
            if (stats == null) return;

            if (_hungerText) _hungerText.text = $"饱食: {stats.Hunger:F0}";
            if (_thirstText) _thirstText.text = $"口渴: {stats.Thirst:F0}";
            if (_temperatureText) _temperatureText.text = $"体温: {stats.Temperature:F0}";
            if (_staminaText) _staminaText.text = $"体力: {stats.Stamina:F0}";
            if (_healthText) _healthText.text = $"生命: {stats.Health:F0}";
        }

        private void RefreshTime()
        {
            var time = this.Query<ITimeQueries>();
            if (time == null) return;

            if (_dayText) _dayText.text = $"第{time.Day}天 {time.GetSeasonName()}";
            if (_timeText) _timeText.text = time.GetTimeOfDayName();
        }
    }
}
