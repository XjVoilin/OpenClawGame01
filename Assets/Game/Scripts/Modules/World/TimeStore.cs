using JulyArch;

namespace OffTrail.World
{
    public sealed class TimeData
    {
        public int Day = 1;
        public int Season = 0;
        public float TimeOfDay = 6f;
    }

    public interface ITimeQueries : IStoreQueries
    {
        int Day { get; }
        int Season { get; }
        float TimeOfDay { get; }
        bool IsNight { get; }
        string GetSeasonName();
        string GetTimeOfDayName();
    }

    public sealed class TimeStore : StoreBase<TimeData>, ITimeQueries
    {
        public int Day => Data.Day;
        public int Season => Data.Season;
        public float TimeOfDay => Data.TimeOfDay;

        public bool IsNight => Data.TimeOfDay >= 20f || Data.TimeOfDay < 5f;

        public void AdvanceTime(float hours)
        {
            if (hours == 0f)
                return;

            Data.TimeOfDay += hours;
            while (Data.TimeOfDay >= 24f)
            {
                Data.TimeOfDay -= 24f;
                Data.Day++;
            }

            while (Data.TimeOfDay < 0f)
            {
                Data.TimeOfDay += 24f;
                Data.Day = UnityEngine.Mathf.Max(1, Data.Day - 1);
            }

            Data.Season = ((Data.Day - 1) / 30) % 4;
        }

        public string GetSeasonName()
        {
            return Data.Season switch
            {
                0 => "Spring",
                1 => "Summer",
                2 => "Autumn",
                _ => "Winter"
            };
        }

        public string GetTimeOfDayName()
        {
            var t = Data.TimeOfDay;
            if (t >= 5f && t < 7f)
                return "Dawn";
            if (t >= 7f && t < 17f)
                return "Day";
            if (t >= 17f && t < 20f)
                return "Dusk";
            return "Night";
        }
    }
}
