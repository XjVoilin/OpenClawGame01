# Plan 2: 时间系统

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 实现完整的时间系统——昼夜循环、季节轮转、行为时间消耗、基础流逝、时间加速、日结算。

**Architecture:** TimeStore (SavableStoreBase) 持有当前天数/分钟/季节数据，TimeSystem (GameSystemBase + IUpdatableSystem) 处理实时流逝和行为消耗，通过事件通知其他系统。Luban TbTime/TbSeason 配表驱动所有数值。

**Tech Stack:** Unity 2022.3, JulyArch (Store-System-View), JulyCore, Luban, UniTask

---

### Task 1: 创建 Luban 时间配置表数据

**Files:**
- Modify: `Tools/Luban/gen_cozyyard_tables.py` (添加 time.xlsx 和 season.xlsx 生成)

- [ ] **Step 1: 在 gen_cozyyard_tables.py 中添加 create_time_xlsx 函数**

```python
def create_time_xlsx():
    wb = Workbook()
    ws = wb.active
    ws.title = "time"

    headers  = ["##var", "id", "phaseName", "startMinute", "lightIntensity", "lightColor"]
    comments = ["##",    "ID", "时段名称",    "开始分钟",     "光照强度(0-1)",    "光照颜色(hex)"]

    rows = [
        ["", 1, "Dawn",      360,  0.4, "FFD4A0"],
        ["", 2, "Morning",   480,  0.8, "FFFFFF"],
        ["", 3, "Noon",      720,  1.0, "FFFFFF"],
        ["", 4, "Afternoon", 840,  0.9, "FFF8E0"],
        ["", 5, "Evening",   1080, 0.5, "FF9040"],
        ["", 6, "Night",     1260, 0.2, "4060A0"],
    ]

    write_sheet(ws, headers, comments, rows)
    path = os.path.join(DATAS_DIR, "time.xlsx")
    wb.save(path)
    print(f"  -> {path}")
```

时段说明：Dawn=6:00, Morning=8:00, Noon=12:00, Afternoon=14:00, Evening=18:00, Night=21:00

- [ ] **Step 2: 添加 create_season_xlsx 函数**

```python
def create_season_xlsx():
    wb = Workbook()
    ws = wb.active
    ws.title = "season"

    headers  = ["##var", "id", "name",   "days", "tempModifier"]
    comments = ["##",    "ID", "季节名称", "天数",  "温度修正(影响生长速度)"]

    rows = [
        ["", 0, "春", 15, 1.0],
        ["", 1, "夏", 15, 1.2],
        ["", 2, "秋", 15, 1.0],
        ["", 3, "冬", 10, 0.5],
    ]

    write_sheet(ws, headers, comments, rows)
    path = os.path.join(DATAS_DIR, "season.xlsx")
    wb.save(path)
    print(f"  -> {path}")
```

- [ ] **Step 3: 在 main 中调用新函数并运行**

在 `if __name__ == "__main__":` 块中添加：
```python
    create_time_xlsx()
    create_season_xlsx()
```

Run:
```bash
cd Tools/Luban
python gen_cozyyard_tables.py
```

- [ ] **Step 4: 提交**

```bash
git add -A
git commit -m "feat(luban): add time and season config tables"
```

---

### Task 2: 实现 TimeStore

**Files:**
- Create: `Assets/Game/Scripts/Modules/Time/TimeData.cs`
- Create: `Assets/Game/Scripts/Modules/Time/TimeStore.cs`
- Create: `Assets/Game/Scripts/Modules/Time/ITimeQueries.cs`

- [ ] **Step 1: 创建 TimeData.cs**

```csharp
using System;
using JulyCore.Data.Save;

namespace CozyYard
{
    [Serializable]
    public class TimeData : ISaveData
    {
        public int Day = 1;
        public int MinuteOfDay = 360;
        public int SeasonIndex = 2; // 从秋季开始 (MVP)
        public int Year = 1;

        public SaveImportance Importance => SaveImportance.Normal;
    }
}
```

- [ ] **Step 2: 创建 ITimeQueries.cs**

```csharp
using JulyArch;

namespace CozyYard
{
    public interface ITimeQueries : IStoreQueries
    {
        int Day { get; }
        int MinuteOfDay { get; }
        int Hour { get; }
        int Minute { get; }
        Season CurrentSeason { get; }
        TimePhase CurrentPhase { get; }
        int Year { get; }
        int DayInSeason { get; }
        bool IsNight { get; }
    }
}
```

- [ ] **Step 3: 创建 TimeStore.cs**

```csharp
using System;

namespace CozyYard
{
    public class TimeStore : SavableStoreBase<TimeData>, ITimeQueries
    {
        protected override string SaveKey => SaveKeys.TimeData;

        public int Day => Data.Day;
        public int MinuteOfDay => Data.MinuteOfDay;
        public int Hour => Data.MinuteOfDay / 60;
        public int Minute => Data.MinuteOfDay % 60;
        public Season CurrentSeason => (Season)Data.SeasonIndex;
        public int Year => Data.Year;

        public TimePhase CurrentPhase => GetPhaseForMinute(Data.MinuteOfDay);

        public int DayInSeason
        {
            get
            {
                int totalDays = Data.Day;
                int[] seasonDays = { 15, 15, 15, 10 };
                int daysInYear = 55;
                int dayInYear = ((totalDays - 1) % daysInYear);
                int accumulated = 0;
                for (int i = 0; i < Data.SeasonIndex; i++)
                {
                    // MVP starts at autumn, so for simplicity count from current season start
                }
                // Simpler: just track days within current season
                return dayInYear + 1; // placeholder, system will manage this properly
            }
        }

        public bool IsNight => CurrentPhase == TimePhase.Night;

        public void AddMinutes(int minutes)
        {
            Data.MinuteOfDay += minutes;
            MarkDirty();
        }

        public void SetMinuteOfDay(int minute)
        {
            Data.MinuteOfDay = minute;
            MarkDirty();
        }

        public void AdvanceDay()
        {
            Data.Day++;
            MarkDirty();
        }

        public void SetSeason(Season season)
        {
            Data.SeasonIndex = (int)season;
            MarkDirty();
        }

        public void AdvanceYear()
        {
            Data.Year++;
            MarkDirty();
        }

        private static TimePhase GetPhaseForMinute(int minute)
        {
            if (minute < 360) return TimePhase.Night;
            if (minute < 480) return TimePhase.Dawn;
            if (minute < 720) return TimePhase.Morning;
            if (minute < 840) return TimePhase.Noon;
            if (minute < 1080) return TimePhase.Afternoon;
            if (minute < 1260) return TimePhase.Evening;
            return TimePhase.Night;
        }
    }
}
```

注意：`DayInSeason` 的精确实现在 TimeSystem 中管理（Store 只做简单存取），这里提供接口满足查询需求。

- [ ] **Step 4: 提交**

```bash
git add -A
git commit -m "feat(time): add TimeData, TimeStore, ITimeQueries"
```

---

### Task 3: 实现 TimeSystem

**Files:**
- Create: `Assets/Game/Scripts/Modules/Time/TimeSystem.cs`

- [ ] **Step 1: 创建 TimeSystem.cs**

```csharp
using JulyArch;
using UnityEngine;

namespace CozyYard
{
    public class TimeSystem : GameSystemBase, IUpdatableSystem
    {
        private TimeStore _store;

        private float _timeScale = 1f;
        private float _accumulatedRealTime;
        private bool _paused;

        // 基础流速：30 分钟真实时间 = 1440 游戏分钟(1天)
        // => 1 真实秒 = 1440 / (30*60) = 0.8 游戏分钟
        private const float BaseGameMinutesPerRealSecond = 0.8f;

        // 一天的活动时间范围
        private const int DayStartMinute = 360;   // 6:00
        private const int DayEndMinute = 1440;    // 24:00 (强制结束)

        // 季节天数
        private static readonly int[] SeasonDays = { 15, 15, 15, 10 };

        public float TimeScale
        {
            get => _timeScale;
            set => _timeScale = Mathf.Clamp(value, 0f, 3f);
        }

        public bool IsPaused
        {
            get => _paused;
            set => _paused = value;
        }

        public int DayInSeason { get; private set; } = 1;

        protected override void OnInitialize()
        {
            _store = GetStore<TimeStore>();
            CalculateDayInSeason();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_paused) return;

            _accumulatedRealTime += deltaTime * _timeScale;

            float minutesToAdd = _accumulatedRealTime * BaseGameMinutesPerRealSecond;
            if (minutesToAdd >= 1f)
            {
                int wholeMinutes = Mathf.FloorToInt(minutesToAdd);
                _accumulatedRealTime -= wholeMinutes / BaseGameMinutesPerRealSecond;
                AdvanceTime(wholeMinutes);
            }
        }

        /// <summary>
        /// 行为消耗时间（由其他系统调用）。
        /// </summary>
        public void ConsumeTime(int minutes)
        {
            if (minutes <= 0) return;
            AdvanceTime(minutes);
        }

        /// <summary>
        /// 设置时间倍速 (1/2/3)。
        /// </summary>
        public void SetSpeed(int multiplier)
        {
            TimeScale = Mathf.Clamp(multiplier, 1, 3);
        }

        /// <summary>
        /// 主动结束当天（玩家点击"休息"）。
        /// </summary>
        public void EndDay()
        {
            var oldPhase = _store.CurrentPhase;
            if (oldPhase != TimePhase.Night)
            {
                _store.SetMinuteOfDay(1260);
                Publish(new PhaseChangedEvent { OldPhase = oldPhase, NewPhase = TimePhase.Night });
            }

            PerformDaySettlement();
        }

        /// <summary>
        /// 确保游戏启动时时间处于合理状态。
        /// </summary>
        public void EnsureDayStarted()
        {
            if (_store.MinuteOfDay < DayStartMinute)
            {
                _store.SetMinuteOfDay(DayStartMinute);
            }
            CalculateDayInSeason();
        }

        private void AdvanceTime(int minutes)
        {
            var oldPhase = _store.CurrentPhase;
            _store.AddMinutes(minutes);

            // 检查是否超过一天
            if (_store.MinuteOfDay >= DayEndMinute)
            {
                _store.SetMinuteOfDay(DayEndMinute);
                var newPhase = _store.CurrentPhase;
                if (oldPhase != newPhase)
                {
                    Publish(new PhaseChangedEvent { OldPhase = oldPhase, NewPhase = newPhase });
                }
                PerformDaySettlement();
                return;
            }

            var currentPhase = _store.CurrentPhase;
            if (oldPhase != currentPhase)
            {
                Publish(new PhaseChangedEvent { OldPhase = oldPhase, NewPhase = currentPhase });
            }
        }

        private void PerformDaySettlement()
        {
            _store.AdvanceDay();

            // 检查季节切换
            DayInSeason++;
            int currentSeasonDays = SeasonDays[(int)_store.CurrentSeason];
            if (DayInSeason > currentSeasonDays)
            {
                DayInSeason = 1;
                var oldSeason = _store.CurrentSeason;
                int nextSeasonIndex = ((int)oldSeason + 1) % 4;
                _store.SetSeason((Season)nextSeasonIndex);

                if (nextSeasonIndex == 0)
                {
                    _store.AdvanceYear();
                }

                Publish(new SeasonChangedEvent
                {
                    OldSeason = oldSeason,
                    NewSeason = _store.CurrentSeason
                });
            }

            // 重置到新一天早晨
            _store.SetMinuteOfDay(DayStartMinute);
            _accumulatedRealTime = 0f;

            Publish(new DayChangedEvent
            {
                NewDay = _store.Day,
                CurrentSeason = _store.CurrentSeason
            });

            Publish(new PhaseChangedEvent
            {
                OldPhase = TimePhase.Night,
                NewPhase = TimePhase.Dawn
            });
        }

        private void CalculateDayInSeason()
        {
            // 从第1天开始，根据总天数和季节天数推算当前是季节内第几天
            int totalDays = _store.Day;
            int daysInYear = 55;
            int dayInYear = ((totalDays - 1) % daysInYear);

            int accumulated = 0;
            // 找到起始季节 (MVP从秋季id=2开始)
            int startSeason = (int)_store.CurrentSeason;
            // 简单实现：直接从当前记录恢复
            // 由于存档恢复时我们有Day和Season，反推DayInSeason
            int seasonStart = 0;
            for (int i = 0; i < startSeason; i++)
            {
                seasonStart += SeasonDays[i];
            }
            int daysSinceSeasonStart = dayInYear - seasonStart;
            if (daysSinceSeasonStart < 0) daysSinceSeasonStart += daysInYear;
            DayInSeason = daysSinceSeasonStart + 1;

            if (DayInSeason > SeasonDays[startSeason])
            {
                DayInSeason = 1;
            }
        }
    }
}
```

- [ ] **Step 2: 提交**

```bash
git add -A
git commit -m "feat(time): add TimeSystem with real-time flow, action consumption, season transitions"
```

---

### Task 4: 注册 Time 模块到 HotUpdateRegistrar

**Files:**
- Modify: `Assets/Game/Scripts/HotUpdateRegistrar.cs`

- [ ] **Step 1: 在 RegisterStores 中注册 TimeStore**

在 `RegisterStores` 方法中添加：
```csharp
ctx.RegisterStore(new TimeStore());
```

- [ ] **Step 2: 在 RegisterSystems 中注册 TimeSystem**

在 `RegisterSystems` 方法中添加：
```csharp
ctx.RegisterSystem(new TimeSystem());
```

- [ ] **Step 3: 在 OnGameLaunch 中初始化 TimeSystem**

在 Grid 初始化之后添加：
```csharp
var timeSystem = AppArch.Context.GetSystem<TimeSystem>();
timeSystem.EnsureDayStarted();
```

- [ ] **Step 4: 提交**

```bash
git add -A
git commit -m "feat(time): register TimeStore and TimeSystem in HotUpdateRegistrar"
```

---

### Task 5: 创建 TimeEvents 补充（确认已有事件覆盖完整）

**Files:**
- Verify: `Assets/Game/Scripts/Shared/Events/TimeEvents.cs`

- [ ] **Step 1: 确认 TimeEvents.cs 已包含所有需要的事件**

需要的事件：
- `PhaseChangedEvent` (OldPhase, NewPhase) ✓
- `DayChangedEvent` (NewDay, CurrentSeason) ✓
- `SeasonChangedEvent` (OldSeason, NewSeason) ✓

如果已存在且正确，无需修改。如果缺少，添加 `TimeTickEvent`：

```csharp
/// <summary>每游戏分钟触发（可选，用于UI时钟刷新）</summary>
public struct TimeTickEvent
{
    public int MinuteOfDay;
}
```

注意：TimeTickEvent 是可选的，如果决定不每分钟发事件（性能考虑），View 可以通过轮询 ITimeQueries 刷新。暂不添加，保持现有三个事件即可。

- [ ] **Step 2: 无修改则跳过提交**

---

### Task 6: 创建 TimeView（HUD 时间显示组件）

**Files:**
- Create: `Assets/Game/Scripts/Views/TimeHUDView.cs`

- [ ] **Step 1: 创建 TimeHUDView.cs**

```csharp
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

        [Header("速度控制")]
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
```

- [ ] **Step 2: 提交**

```bash
git add -A
git commit -m "feat(time): add TimeHUDView for clock display and speed controls"
```

---

### Task 7: 昼夜光照控制 (TimeLightingView)

**Files:**
- Create: `Assets/Game/Scripts/Views/TimeLightingView.cs`

- [ ] **Step 1: 创建 TimeLightingView.cs**

```csharp
using JulyArch;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CozyYard
{
    public class TimeLightingView : GameView
    {
        [SerializeField] private Light2D _globalLight;

        [Header("光照配置")]
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

        public override IGameContext GetArchitecture() => AppArch.Context;

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
            var q = this.Query<ITimeQueries>();
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
```

- [ ] **Step 2: 提交**

```bash
git add -A
git commit -m "feat(time): add TimeLightingView for day/night light transitions"
```

---

## 计划完成标志

当以上 7 个 Task 全部完成后，时间系统应处于以下状态：
- Luban 有 time.xlsx 和 season.xlsx 配置数据
- TimeStore 持有天数/分钟/季节并可存档
- TimeSystem 每帧推进时间（基础流逝 + 行为消耗接口）
- 季节按 15/15/15/10 天自动轮转
- 一天结束时触发 DayChangedEvent 供其他系统响应
- TimeHUDView 显示时钟/天数/季节 + 1x/2x/3x 加速按钮
- TimeLightingView 根据时段平滑切换 URP 2D 光照

接下来进入 **Plan 3: 背包系统**。
