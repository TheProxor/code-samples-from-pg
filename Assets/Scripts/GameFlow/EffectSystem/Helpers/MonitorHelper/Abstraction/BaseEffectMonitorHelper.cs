using System.Collections.Generic;
using Drawmasters.Effects.Helpers.MonitorHelper.Interfaces;
using Drawmasters.Levels;


namespace Drawmasters.Effects.Helpers.MonitorHelper.Abstraction
{
    public abstract class BaseEffectMonitorHelper : IMonitorHelper
    {
        protected abstract HashSet<LevelState> MonitoredLevelStates { get; }
        
        protected readonly HashSet<EffectHandler> effects = new HashSet<EffectHandler>();

        public void BindComponent(EffectHandler effect) =>
            effects.AddIfNotContains(effect);

        public BaseEffectMonitorHelper()
        {
            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
        }

        private void Level_OnLevelStateChanged(LevelState levelState)
        {
            bool isMonitored = MonitoredLevelStates.Contains(levelState);
            if (isMonitored)
            {
                FlushEffects();
            }
        }


        protected void FlushEffects()
        {
            foreach (var handler in effects)
            {
                if (handler != null && !handler.InPool)
                {
                    EffectManager.Instance.PoolHelper.PushObject(handler);
                }
            }

            effects.Clear();
        }
    }
}