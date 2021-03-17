using UnityEngine;
using System;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "ProjectileSmashShakeSettings",
                        menuName = NamingUtility.MenuItems.IngameSettings + "ProjectileSmashShakeSettings")]
    public class ProjectileSmashShakeSettings : ScriptableObjectData<ProjectileSmashShakeSettings.Data, CollidableObjectType>
    {
        #region Nested types

        [Serializable]
        public class Data : ScriptableObjectBaseData<CollidableObjectType>
        {
            public CameraShakeSettings.Shake shake = default;
        }

        #endregion



        #region Methods

        public CameraShakeSettings.Shake FindShake(CollidableObjectType type)
        {
            Data data = FindData(type);
            return data == null ? default : data.shake;
        }

        #endregion
    }
}