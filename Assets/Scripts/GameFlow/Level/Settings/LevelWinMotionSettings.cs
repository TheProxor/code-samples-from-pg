using System;
using UnityEngine;

namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "LevelWinMotionSettings",
                     menuName = NamingUtility.MenuItems.IngameSettings + "LevelWinMotionSettings")]
    public class LevelWinMotionSettings : ScriptableObject
    {
        #region Helper types

        [Serializable]
        public class Zoom
        {
            [Range(0f, 1f)] public float zoomFactor = default;
            public float duration = default;
            public AnimationCurve curve = default;
        }


        [Serializable]
        public class SlowMotion
        {
            public float timeScaleEndValue = default;
            public float duration = default;
            public float delay = default;
            public AnimationCurve curve = default;
        }


        [Serializable]
        public class CameraMove
        {
            public Vector2 cameraRelative = default;
            public float duration = default;
            public AnimationCurve curve = default;
        }

        #endregion


        #region Fields

        [Header("Camera zoom")]
        public Zoom zoom = default;

        [Header("Slow motion")]
        public SlowMotion[] slowMotion = default;

        [Header("Camera move")]
        public CameraMove cameraMove = default;

        #endregion
    }
}

