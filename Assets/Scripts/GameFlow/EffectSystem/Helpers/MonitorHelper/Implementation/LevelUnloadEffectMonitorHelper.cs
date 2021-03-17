using System.Collections.Generic;
using Drawmasters.Effects.Helpers.MonitorHelper.Abstraction;
using Drawmasters.Levels;


namespace Drawmasters.Effects.Helpers.MonitorHelper.Implementation
{
    public class LevelUnloadEffectMonitorHelper : BaseEffectMonitorHelper
    {
        protected readonly HashSet<LevelState> monitoredLevelStates = new HashSet<LevelState>()
        {
            LevelState.Unloaded
        };

        protected override HashSet<LevelState> MonitoredLevelStates => monitoredLevelStates;
    }
}