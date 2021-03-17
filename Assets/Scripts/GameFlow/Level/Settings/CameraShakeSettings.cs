using System;
using UnityEngine;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "CameraShakeSettings",
                        menuName = NamingUtility.MenuItems.IngameSettings + "CameraShakeSettings")]
    public class CameraShakeSettings : ScriptableObject
    {
        #region Helper types

        [Serializable]
        public class Shake
        {
            [Header("Длительность шейка")]
            public float duration = default;

            [Header("Сила(амплитуда) шейка")]
            public float strength = default;

            [Header("Количество вибраций")]
            public int vibrato = default;

            [Header("Слуайность направления вибраций")]
            public float randomness = default;

            public bool isFadeOut = default;

            public float delay = default;
        }

        #endregion



        #region Fields

        public Shake dynamiteExplosion = default;

        [Tooltip("когда стартанули босс уровень")]
        public Shake[] bossFirstCome = default;
        [Tooltip("когда попали в босса")]
        public Shake bossStageChange = default;
        [Tooltip("когда хотя бы один объект начинает падать")]
        public Shake objectsFreeFall = default;

        #endregion
    }
}
