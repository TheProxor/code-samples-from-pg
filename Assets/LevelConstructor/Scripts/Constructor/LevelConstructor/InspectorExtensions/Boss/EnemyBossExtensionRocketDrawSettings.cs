using System;
using Drawmasters.Levels;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    [CreateAssetMenu(fileName = "EnemyBossExtensionRocketDrawSettings",
                     menuName = NamingUtility.MenuItems.ConstructorData + "EnemyBossExtensionRocketDrawSettings")]
    public class EnemyBossExtensionRocketDrawSettings : ScriptableObjectData<EnemyBossExtensionRocketDrawSettings.Data, ShooterColorType>
    {
        #region Nested types

        [Serializable]
        public class Data : ScriptableObjectBaseData<ShooterColorType>
        {
            public Color color = default;
        }

        #endregion



        #region Fields

        public float pointsDistance = default;

        #endregion



        #region Methods

        public Color GetColor(ShooterColorType colorType)
        {
            Data foundData = FindData(colorType);
            return foundData == null ? Color.white : foundData.color;
        }

        #endregion
    }
}