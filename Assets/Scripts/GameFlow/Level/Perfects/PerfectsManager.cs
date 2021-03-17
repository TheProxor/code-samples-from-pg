using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Drawmasters.Vibration;


namespace Drawmasters.Levels
{
    public static class PerfectsManager
    {
        #region Fields

        public static event Action<PerfectType, Vector3, LevelTarget> OnPerfectReceived;
        
        private static bool EnableLog = false;

        private static readonly Dictionary<PerfectType, float> cooldowns = new Dictionary<PerfectType, float>();
         
        private static PerfectsSettings settings;

        #endregion



        #region Methods

        public static void Initialize()
        {
            settings = IngameData.Settings.currencySettings;

            foreach (PerfectType type in (PerfectType[])Enum.GetValues(typeof(PerfectType)))
            {
                cooldowns.Add(type, default);
            }

            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
        }


        public static void Deinitialize()
        {
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
        }


        public static void PerfectReceiveNotify(PerfectType type, Vector3 position, LevelTarget levelTarget)
        {
            if (cooldowns.TryGetValue(type, out float currentCooldown) &&
                levelTarget.AllowPerfects)
            {
                if (currentCooldown <= 0.0f)
                {
                    float cooldown = settings.GetMinDelayForNextReceive(type);
                    cooldowns[type] = cooldown;

                    OnPerfectReceived?.Invoke(type, position, levelTarget);

                    VibrationManager.Play(IngameVibrationType.PerfectReceive);

                    if (EnableLog)
                    {
                        CustomDebug.Log($"<color=red>Received perfect {type} </color>");
                    }
                }
                else if (EnableLog)
                {
                    CustomDebug.Log($"<color=red>Not expired cooldown for perfect {type}. Left {currentCooldown} </color>");
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
