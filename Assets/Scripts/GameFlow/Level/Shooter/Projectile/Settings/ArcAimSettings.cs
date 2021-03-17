using System;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Drawmasters.Levels
{
    [Serializable]
    public class ArcAimSettings
    {
        public Vector2 defaultDirection = default;
        [MinValue(0.0f)] public float minOffsetForFlipX = default;
        [MinValue(0.0f)] public Vector2 directionOffsetPerUnit = default;

        [MinValue(0.0f)] public float distanceForMinFlyTime = default;
        [MinValue(0.0f)] public float distanceForMaxFlyTime = default;

        [MinValue(0.0f)] public float minProjectileFlyTime = default;
        [MinValue(0.0f)] public float maxProjectileFlyTime = default;

        [MinValue(0.0f)] public float trajectorySpeed = default;
        [MinValue(1)] public int dotsCount = default;
        [MinValue(0.01f)] public float dotsSpawnDelay = default;
        [MinValue(0.0f)] public float interval = default;
    }
}
