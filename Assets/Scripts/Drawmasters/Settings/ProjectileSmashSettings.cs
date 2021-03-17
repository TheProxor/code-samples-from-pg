using UnityEngine;
using System;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "ProjectileSmashSettings",
                        menuName = NamingUtility.MenuItems.IngameSettings + "ProjectileSmashSettings")]
    public class ProjectileSmashSettings : ScriptableObjectData<ProjectileSmashSettings.Data, CollidableObjectType>
    {
        #region Nested types

        [Serializable]
        public class Data : ScriptableObjectBaseData<CollidableObjectType>
        {
            public NumberAnimation gravityScaleAnimation = default;
            [Range(0.0f, 1.0f)] public float savedVelocityPart = default;
            public float aditionalSmashForce = default;
            public float torque = default;
        }

        #endregion



        #region Methods

        public NumberAnimation FindSmashGravityScaleAnimation(CollidableObjectType type)
        {
            Data data = FindData(type);
            return data == null ? default : data.gravityScaleAnimation;
        }


        public float FindAdditionalSmashForce(CollidableObjectType type)
        {
            Data data = FindData(type);
            return data == null ? default : data.aditionalSmashForce;
        }


        public float FindAdditionalTorque(CollidableObjectType type)
        {
            Data data = FindData(type);
            return data == null ? default : data.torque;
        }


        public float FindSavedVelocityPart(CollidableObjectType type)
        {
            Data data = FindData(type);
            return data == null ? default : data.savedVelocityPart;
        }
        #endregion
    }
}