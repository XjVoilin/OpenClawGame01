using JulyArch;
using OffTrail;

namespace OffTrail.World
{
    public sealed class DayNightSystem : GameSystemBase, IAppArch, IUpdatableSystem
    {
        public const float GameHoursPerRealtimeSecond = 120f / 3600f;

        public new IGameContext GetArchitecture() => AppArch.Context;

        public void OnUpdate(float deltaTime)
        {
            if (deltaTime <= 0f) return;

            var timeSnap = this.Query<ITimeQueries>();

            var prevDay = timeSnap?.Day ?? 1;
            var prevSeason = timeSnap?.Season ?? 0;
            var beforeNightState = timeSnap?.IsNight == true;

            var hours = deltaTime * GameHoursPerRealtimeSecond;

            this.Mutate<TimeStore>(t => { t.AdvanceTime(hours); });

            var time = this.Query<ITimeQueries>();
            if (time == null)
                return;

            if (time.Day != prevDay)
                this.Publish(new DayChanged { NewDay = time.Day });

            if (time.Season != prevSeason)
            {
                this.Publish(new SeasonChanged
                    { NewSeason = time.Season, SeasonName = time.GetSeasonName() });
            }

            if (beforeNightState && !time.IsNight)
                this.Publish(new DawnBroke());

            if (!beforeNightState && time.IsNight)
                this.Publish(new NightFell());
        }
    }
}
