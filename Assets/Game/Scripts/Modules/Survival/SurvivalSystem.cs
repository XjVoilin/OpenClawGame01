using JulyArch;
using OffTrail;
using OffTrail.World;
using UnityEngine;

namespace OffTrail.Survival
{
    public sealed class SurvivalSystem : GameSystemBase, IAppArch, IUpdatableSystem
    {
        public const float HungerDecayRate = 0.5f;
        public const float ThirstDecayRate = 0.8f;
        public const float HealthDecayFromHunger = 1.0f;
        public const float HealthDecayFromThirst = 2.0f;
        public const float HealthDecayFromCold = 1.5f;
        public const float StaminaRecoveryRate = 2.0f;

        private const float TemperatureMovePerMinute = 18f;

        public new IGameContext GetArchitecture() => AppArch.Context;

        public void OnUpdate(float deltaTime)
        {
            if (deltaTime <= 0f) return;

            var stats = this.Query<ISurvivalQueries>();
            if (stats == null || !stats.IsAlive)
                return;

            var minutesElapsed = deltaTime / 60f;
            var time = this.Query<ITimeQueries>();

            float seasonTarget = 50f;
            if (time != null)
            {
                switch (time.Season)
                {
                    case 0:
                        seasonTarget = 55f;
                        break;
                    case 1:
                        seasonTarget = 75f;
                        break;
                    case 2:
                        seasonTarget = 50f;
                        break;
                    case 3:
                        seasonTarget = 35f;
                        break;
                }
            }

            var blendedTarget = seasonTarget;
            if (time != null && time.IsNight)
                blendedTarget -= 12f;

            blendedTarget = Mathf.Clamp(blendedTarget, 0f, 100f);

            var hungerLoss = HungerDecayRate * minutesElapsed;
            var thirstLoss = ThirstDecayRate * minutesElapsed;
            var moveStep = TemperatureMovePerMinute * minutesElapsed;

            var currentTemp = stats.Temperature;
            var temperatureDelta = Mathf.MoveTowards(currentTemp, blendedTarget, moveStep) - currentTemp;

            this.Mutate<SurvivalStore>(store =>
            {
                store.UpdateHunger(-hungerLoss);
                store.UpdateThirst(-thirstLoss);
                store.UpdateTemperature(temperatureDelta);

                if (store.Hunger <= 0f)
                    store.UpdateHealth(-HealthDecayFromHunger * minutesElapsed);

                if (store.Thirst <= 0f)
                    store.UpdateHealth(-HealthDecayFromThirst * minutesElapsed);

                if (store.Temperature <= 20f)
                    store.UpdateHealth(-HealthDecayFromCold * minutesElapsed);

                if (store.Hunger > 0f && store.Thirst > 0f)
                    store.UpdateStamina(StaminaRecoveryRate * minutesElapsed);
            });
        }
    }
}
