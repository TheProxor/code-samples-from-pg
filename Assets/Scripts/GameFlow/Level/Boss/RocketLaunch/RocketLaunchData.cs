using System;
using UnityEngine;


namespace Drawmasters.Levels
{
    [Serializable]
    public class RocketLaunchData
    {
        [Serializable]
        public class Data
        {
            public Vector3[] trajectory = default;
            public ShooterColorType colorType = default;
        }

        public Data[] data = default;
    }
}
