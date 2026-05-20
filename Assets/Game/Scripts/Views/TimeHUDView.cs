using JulyArch;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CozyYard
{
    public class TimeHUDView : GameView
    {
        [SerializeField] private TextMeshProUGUI _dayText;
        [SerializeField] private TextMeshProUGUI _seasonText;
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private TextMeshProUGUI _phaseText;

        [Header("Speed Controls")]
        [SerializeField] private Button _speed1Btn;
        [SerializeField] private Button _speed2Btn;
        [SerializeField] private Button _speed3Btn;
        [SerializeField] private Button _endDayBtn;

        private TimeSystem _timeSystem;

        public override IGameContext GetArchitecture() => AppArch.Context;

        protected override void OnViewEnable()
        {
            _timeSystem = this.GetSystem<TimeSystem>();

            this.Subscribe<PhaseChangedEvent>(OnPhaseChanged);
            this.Subscribe<DayChangedEvent>(OnDayChanged);
            this.Subscribe<SeasonChangedEvent>(OnSeasonChanged);

            if (_speed1Btn) _speed1Btn.onClick.AddListener(() => _timeSystem.SetSpeed(1));
            if (_speed2Btn) _speed2Btn.onClick.AddListener(() => _timeSystem.SetSpeed(2));
            if (_speed3Btn) _speed3Btn.onClick.AddListener(() => _timeSystem.SetSpeed(3));
            if (_endDayBtn) _endDayBtn.onClick.AddListener(() => _timeSystem.EndDay());

            Refresh();
        }

        protected override void OnViewDisable()
        {
            if (_speed1Btn) _speed1Btn.onClick.RemoveAllListeners();
            if (_speed2Btn) _speed2Btn.onClick.RemoveAllListeners();
            if (_speed3Btn) _speed3Btn.onClick.RemoveAllListeners();
            if (_endDayBtn) _endDayBtn.onClick.RemoveAllListeners();
        }

        private void Update()
        {
            RefreshTime();
        }

        private void OnPhaseChanged(PhaseChangedEvent e) => Refresh();
        private void OnDayChanged(DayChangedEvent e) => Refresh();
        private void OnSeasonChanged(SeasonChangedEvent e) => Refresh();

        private void Refresh()
        {
            RefreshTime();
            RefreshDay();
        }

        private void RefreshTime()
        {
            var q = this.Query<ITimeQueries>();
            if (_timeText) _timeText.text = $"{q.Hour:D2}:{q.Minute:D2}";
            if (_phaseText) _phaseText.text = GetPhaseName(q.CurrentPhase);
        }

        private void RefreshDay()
        {
            var q = this.Query<ITimeQueries>();
            if (_dayText) _dayText.text = $"第 {q.Day} 天";
            if (_seasonText) _seasonText.text = GetSeasonName(q.CurrentSeason);
        }

        private static string GetSeasonName(Season s) => s switch
        {
            Season.Spring => "春",
            Season.Summer => "夏",
            Season.Autumn => "秋",
            Season.Winter => "冬",
            _ => "?"
        };

        private static string GetPhaseName(TimePhase p) => p switch
        {
            TimePhase.Dawn => "清晨",
            TimePhase.Morning => "上午",
            TimePhase.Noon => "正午",
            TimePhase.Afternoon => "下午",
            TimePhase.Evening => "傍晚",
            TimePhase.Night => "夜晚",
            _ => "?"
        };
    }
}
