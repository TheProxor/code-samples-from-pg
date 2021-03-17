using System;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.NiceVibrations;


namespace Drawmasters.Vibration
{
    public static class VibrationManager
    {
        #region Fields

        private static readonly Dictionary<HapticTypes, float> cooldowns = new Dictionary<HapticTypes, float>();

        private static VibrationSettings settings;

        private const string VibrationKey = "Vibration";

        #endregion



        #region Properties

        public static bool IsVibrationEnabled
        {
            get => CustomPlayerPrefs.GetBool(VibrationKey, true);
            set => CustomPlayerPrefs.SetBool(VibrationKey, value);
        }

        #endregion



        #region Public methods

        public static void Initialize()
        {
            settings = IngameData.Settings.vibrationSettings;

            MMVibrationManager.iOSInitializeHaptics();

            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;

            foreach (HapticTypes type in (HapticTypes[])Enum.GetValues(typeof(HapticTypes)))
            {
                cooldowns.Add(type, default);
            }
        }


        public static void Deinitialize()
        {
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
        }


        public static void Play(HapticTypes hapticType) =>
            TryPlayVibration(hapticType);
        

        public static void Play(IngameVibrationType ingameType)
        {
            HapticTypes type = settings.FindVibrationType(ingameType);
            TryPlayVibration(type);
        }

        #endregion



        #region Private methods

        private static void TryPlayVibration(HapticTypes type)
        {
            if (type != HapticTypes.None)
            {
                if (cooldowns.TryGetValue(type, out float currentCooldown) &&
                    currentCooldown <= 0.0f)
                {
                    if (IsVibrationEnabled)
                    {
                        MMVibrationManager.Haptic(type);
                    }

                    float cooldown = settings.FindCooldown(type);
                    cooldowns[type] = cooldown;
                }
            }
        }

        #endregion



        #region Events handlers

        private static void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            foreach (var key in cooldowns.Keys.ToList())
            {
                cooldowns[key] -= deltaTime;
            }
        }

        #endregion
    }
}
