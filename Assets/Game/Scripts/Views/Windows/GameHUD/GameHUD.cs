using cfg;
using JulyArch;
using JulyCore;
using TMPro;
using UnityEngine;
namespace SpiritHealer
{
    /// <summary>
    /// 主界面 HUD：显示时间/资源/来客队列，提供核心操作入口。
    /// 游戏启动后常驻显示，不参与 UI 栈管理。
    /// </summary>
    public class GameHUD : GameUIView
    {
        [Header("时间信息")]
        [SerializeField] private TextMeshProUGUI _dayText;
        [SerializeField] private TextMeshProUGUI _seasonText;
        [SerializeField] private TextMeshProUGUI _phaseText;
        [SerializeField] private TextMeshProUGUI _timeText;

        [Header("玩家资源")]
        [SerializeField] private TextMeshProUGUI _coinsText;
        [SerializeField] private TextMeshProUGUI _reputationText;

        [Header("来客队列")]
        [SerializeField] private TextMeshProUGUI _queueText;

        [Header("操作按钮")]
        [SerializeField] private UISmartButton _acceptVisitorBtn;
        [SerializeField] private UISmartButton _endDayBtn;

        private TimeStore _timeStore;
        private PlayerStore _playerStore;
        private VisitorStore _visitorStore;
        private GameLoopSystem _gameLoop;

        protected override void OnBeforeOpen()
        {
            _timeStore = this.GetStore<TimeStore>();
            _playerStore = this.GetStore<PlayerStore>();
            _visitorStore = this.GetStore<VisitorStore>();
            _gameLoop = this.GetSystem<GameLoopSystem>();

            _acceptVisitorBtn.onClick.AddListener(OnAcceptVisitor);
            _endDayBtn.onClick.AddListener(OnEndDay);

            this.Subscribe<PhaseChangedEvent>(OnPhaseChanged);
            this.Subscribe<DayChangedEvent>(OnDayChanged);
            this.Subscribe<TreatmentCompletedEvent>(OnTreatmentCompleted);
            this.Subscribe<VisitorChangedEvent>(OnVisitorChanged);

            Refresh();
        }

        protected override void OnClose()
        {
            _acceptVisitorBtn.onClick.RemoveListener(OnAcceptVisitor);
            _endDayBtn.onClick.RemoveListener(OnEndDay);
            this.UnsubscribeAll();
        }

        private void OnPhaseChanged(PhaseChangedEvent e) => Refresh();
        private void OnDayChanged(DayChangedEvent e) => Refresh();
        private void OnTreatmentCompleted(TreatmentCompletedEvent e) => Refresh();
        private void OnVisitorChanged(VisitorChangedEvent e) => Refresh();

        private void Refresh()
        {
            RefreshTime();
            RefreshResources();
            RefreshQueue();
            RefreshButtons();
        }

        private void RefreshTime()
        {
            if (_dayText) _dayText.text = $"第 {_timeStore.Day} 天";
            if (_seasonText) _seasonText.text = GetSeasonName(_timeStore.CurrentSeason);
            if (_phaseText) _phaseText.text = GetPhaseName(_timeStore.CurrentPhase);
            if (_timeText) _timeText.text = $"{_timeStore.Hour:D2}:{_timeStore.Minute:D2}";
        }

        private void RefreshResources()
        {
            if (_coinsText) _coinsText.text = _playerStore.Coins.ToString();
            if (_reputationText) _reputationText.text = _playerStore.Reputation.ToString();
        }

        private void RefreshQueue()
        {
            var count = _visitorStore.WaitingQueue.Count;
            if (_queueText) _queueText.text = count > 0 ? $"等候: {count}人" : "无来客";
        }

        private void RefreshButtons()
        {
            bool hasQueue = _visitorStore.WaitingQueue.Count > 0;
            bool noCurrentVisitor = _visitorStore.CurrentVisitor == null;
            bool isOpen = _timeStore.IsOpen;

            _acceptVisitorBtn.SetInteractable(hasQueue && noCurrentVisitor && isOpen);
            _endDayBtn.SetInteractable(noCurrentVisitor);
        }

        private void OnAcceptVisitor()
        {
            if (!_gameLoop.AcceptNextVisitor()) return;
            Refresh();
            GF.UI.Open(UIWindowId.VisitorWindow);
        }

        private void OnEndDay()
        {
            _gameLoop.EndDay();
        }

        private static string GetSeasonName(ESeason s) => s switch
        {
            ESeason.Spring => "春",
            ESeason.Summer => "夏",
            ESeason.Autumn => "秋",
            ESeason.Winter => "冬",
            _ => "?"
        };

        private static string GetPhaseName(ETimePhase p) => p switch
        {
            ETimePhase.Morning => "早晨",
            ETimePhase.Noon => "正午",
            ETimePhase.Afternoon => "下午",
            ETimePhase.Evening => "傍晚",
            ETimePhase.Night => "夜晚",
            _ => "?"
        };
    }
}
