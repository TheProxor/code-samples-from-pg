using System;
using UnityEngine;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "SkipAnimationSettings",
                     menuName = NamingUtility.MenuItems.IngameSettings + "SkipAnimationSettings")]
    public class SkipAnimationSettings : ScriptableObjectData<SkipAnimationSettings.Data, LevelType>
    {
        #region Nested types

        [Serializable]
        public class Data : ScriptableObjectBaseData<LevelType>
        {
            public RuntimeAnimatorController buttonAnimationController = default;
        }

        #endregion



        #region Methods

        public RuntimeAnimatorController FindButtonAnimationController(LevelType levelType)
        {
            Data foundData = FindData(levelType);

            return foundData == null ? default : foundData.buttonAnimationController;
        }

        #endregion
    }
}