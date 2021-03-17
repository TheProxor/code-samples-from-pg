using System;

namespace Drawmasters.Levels
{
    [Serializable]
    public class AttackedBossSerializableData : BossSerializableData
    {
        public RocketLaunchData[] rocketLaunchData = default;
        public LevelObjectMoveSettings[] stagesMovement = default;
    }
}

