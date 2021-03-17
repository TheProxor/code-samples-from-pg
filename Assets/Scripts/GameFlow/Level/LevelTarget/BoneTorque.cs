using System;


namespace Drawmasters.Levels
{
    [Serializable]
    public class BoneTorque
    {
        [Enum(typeof(EnemiesBones))]
        public string boneName = default;

        public float force = default;
    }
}
