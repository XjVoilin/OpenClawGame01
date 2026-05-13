using JulyArch;
using UnityEngine;

namespace OffTrail.Survival
{
    public sealed class SurvivalData
    {
        public float Hunger = 80f;
        public float Thirst = 80f;
        public float Temperature = 50f;
        public float Stamina = 100f;
        public float Health = 100f;
    }

    public interface ISurvivalQueries : IStoreQueries
    {
        float Hunger { get; }
        float Thirst { get; }
        float Temperature { get; }
        float Stamina { get; }
        float Health { get; }
        bool IsAlive { get; }
    }

    public sealed class SurvivalStore : StoreBase<SurvivalData>, ISurvivalQueries
    {
        public float Hunger => Data.Hunger;
        public float Thirst => Data.Thirst;
        public float Temperature => Data.Temperature;
        public float Stamina => Data.Stamina;
        public float Health => Data.Health;

        public bool IsAlive => Data.Health > 0f;

        public void UpdateHunger(float delta)
        {
            var old = Data.Hunger;
            Data.Hunger = Mathf.Clamp(Data.Hunger + delta, 0f, 100f);
            if (!Mathf.Approximately(old, Data.Hunger))
                this.Publish(new SurvivalStatChanged { StatName = nameof(Hunger), OldValue = old, NewValue = Data.Hunger });
        }

        public void UpdateThirst(float delta)
        {
            var old = Data.Thirst;
            Data.Thirst = Mathf.Clamp(Data.Thirst + delta, 0f, 100f);
            if (!Mathf.Approximately(old, Data.Thirst))
                this.Publish(new SurvivalStatChanged { StatName = nameof(Thirst), OldValue = old, NewValue = Data.Thirst });
        }

        public void UpdateTemperature(float delta)
        {
            var old = Data.Temperature;
            Data.Temperature = Mathf.Clamp(Data.Temperature + delta, 0f, 100f);
            if (!Mathf.Approximately(old, Data.Temperature))
                this.Publish(new SurvivalStatChanged { StatName = nameof(Temperature), OldValue = old, NewValue = Data.Temperature });
        }

        public void UpdateStamina(float delta)
        {
            var old = Data.Stamina;
            Data.Stamina = Mathf.Clamp(Data.Stamina + delta, 0f, 100f);
            if (!Mathf.Approximately(old, Data.Stamina))
                this.Publish(new SurvivalStatChanged { StatName = nameof(Stamina), OldValue = old, NewValue = Data.Stamina });
        }

        public void UpdateHealth(float delta)
        {
            var old = Data.Health;
            Data.Health = Mathf.Clamp(Data.Health + delta, 0f, 100f);
            if (!Mathf.Approximately(old, Data.Health))
            {
                this.Publish(new SurvivalStatChanged { StatName = nameof(Health), OldValue = old, NewValue = Data.Health });
                if (old > 0f && Data.Health <= 0f)
                    this.Publish(new PlayerDied());
            }
        }
    }
}
