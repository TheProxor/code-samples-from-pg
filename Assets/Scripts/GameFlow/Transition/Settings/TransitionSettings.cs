
﻿using System;
using UnityEngine;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "TransitionSettings",
                     menuName = NamingUtility.MenuItems.IngameSettings + "TransitionSettings")]
    public class TransitionSettings : ScriptableObject
    {
        #region Helper types

        [Serializable]
        public class FromLevelToLevel
        {
            public Mesh skullMesh = default;

            public Material skullMaterial = default;

            public float beginScaleFactor = default;
            public float endScaleFactor = default;
            public float duration = default;
        }

        #endregion



        #region Fields

        [Header("Transition settings between levels.")]
        public FromLevelToLevel fromLevelToLevel = default;

        #endregion
    }
}