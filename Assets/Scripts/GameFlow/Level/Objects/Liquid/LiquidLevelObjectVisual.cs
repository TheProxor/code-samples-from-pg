using UnityEngine;
using System;


namespace Drawmasters.Levels
{
    [Serializable]
    public class LiquidLevelObjectVisual
    {
        public LiquidLevelObjectType type = default;
        public Color textureColor = default;

        [Enum(typeof(EffectKeys))]
        public string projectileCollisionEffectKey = default;
    }
}
