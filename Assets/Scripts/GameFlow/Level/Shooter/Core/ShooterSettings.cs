using System;
using UnityEngine;


namespace Drawmasters
{
    [CreateAssetMenu(fileName = "ShooterSettings",
                     menuName = NamingUtility.MenuItems.IngameSettings + "ShooterSettings")]
    public class ShooterSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        public class Input
        {
            public float shotRendererMagnitude = default;
            public float returnedAimingTime = default;

            public float minDistanceBetweenPoints = 0.1f;
        }


        [Serializable]
        public class Collision
        {
            public float delayToSelfKillAllow = 0.1f;
            public Vector2 collisionRectOffset = default;
            public Rect collisionRect = default;
        }

        #endregion



        #region Fields

        public Input input = default;
        public Collision collision = default;

        #endregion
    }
}
