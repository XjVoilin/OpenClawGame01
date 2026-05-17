using JulyArch;

namespace SpiritHealer
{
    /// <summary>
    /// 时间系统 —— 轻量混合时间模型。
    /// 时间缓慢自然流逝，打开交互面板时自动暂停，玩家可随时手动跳过当前时段。
    /// 每 7 天轮转季节（春→夏→秋→冬），季节影响种植、来客、奇遇概率。
    /// </summary>
    public class TimeSystem : GameSystemBase, IUpdatableSystem
    {
        private const float DefaultPhaseDuration = 180f;
        private const int DaysPerSeason = 7;

        private bool _paused;
        private float _phaseDuration = DefaultPhaseDuration;

        public bool IsPaused => _paused;
        public float PhaseDuration => _phaseDuration;

        /// <summary>
        /// 流逝的时间
        /// </summary>
        private float _phaseElapsed;

        protected override void OnInitialize()
        {
        }

        /// <summary>暂停时间流逝（UI 面板打开时调用）。</summary>
        public void Pause() => _paused = true;

        /// <summary>恢复时间流逝（UI 面板关闭时调用）。</summary>
        public void Resume() => _paused = false;

        /// <summary>设置每个时段的持续秒数（调试/配置用）。</summary>
        public void SetPhaseDuration(float seconds) => _phaseDuration = seconds;

        public void OnUpdate(float deltaTime)
        {
            if (_paused) return;

            _phaseElapsed += deltaTime;

            if (_phaseElapsed >= _phaseDuration)
            {
                AdvancePhase();
            }
        }

        /// <summary>手动跳过当前时段，立即推进到下一时段。</summary>
        public void SkipPhase()
        {
            AdvancePhase();
        }

        private void AdvancePhase()
        {
            var time = GetStore<TimeStore>();
            var oldPhase = time.CurrentPhase;
            var newPhase = NextPhase(oldPhase);

            time.SetPhase(newPhase);
            _phaseElapsed = 0f;

            this.Publish(new PhaseChangedEvent { OldPhase = oldPhase, NewPhase = newPhase });

            if (oldPhase == TimePhase.Night && newPhase == TimePhase.Morning)
            {
                AdvanceToNextDay();
            }
        }

        private void AdvanceToNextDay()
        {
            GetStore<TimeStore>().AdvanceDay();

            CheckSeasonTransition();

            var time = GetStore<TimeStore>();
            this.Publish(new DayChangedEvent
            {
                NewDay = time.Day,
                CurrentSeason = time.CurrentSeason
            });
        }

        private void CheckSeasonTransition()
        {
            var time = GetStore<TimeStore>();
            if ((time.Day - 1) % DaysPerSeason == 0 && time.Day > 1)
            {
                var next = (Season)(((int)time.CurrentSeason + 1) % 4);
                time.SetSeason(next);
            }
        }

        private static TimePhase NextPhase(TimePhase current) => current switch
        {
            TimePhase.Morning => TimePhase.Daytime,
            TimePhase.Daytime => TimePhase.Evening,
            TimePhase.Evening => TimePhase.Night,
            TimePhase.Night => TimePhase.Morning,
            _ => TimePhase.Morning
        };
    }
}
