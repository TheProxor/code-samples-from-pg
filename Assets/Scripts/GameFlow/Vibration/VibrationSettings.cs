using System;
using MoreMountains.NiceVibrations;
using UnityEngine;


namespace Drawmasters.Vibration
{
    [CreateAssetMenu(fileName = "VibrationSettings",
                    menuName = NamingUtility.MenuItems.IngameSettings + "VibrationSettings")]
    public class VibrationSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        private class CooldownData
        {
            public HapticTypes type = default;
            public float cooldown = default;
        }


        [Serializable]
        private class TypeData
        {
            public IngameVibrationType ingameType = default;
            public HapticTypes vibrationType = default;
        }

        #endregion



        #region Fields

        [SerializeField] private CooldownData[] cooldownData = default;
        [SerializeField] private TypeData[] typeData = default;

        #endregion



        #region Methods

        public float FindCooldown(HapticTypes type)
        {
            CooldownData foundData = Array.Find(cooldownData, element => element.type == type);

            if (foundData == null)
            {
                CustomDebug.Log($"Do data found for type {type} in {this}");
                return default;
            }

            return foundData.cooldown;
        }


        public HapticTypes FindVibrationType(IngameVibrationType ingameType)
        {
            TypeData foundData = Array.Find(typeData, element => element.ingameType == ingameType);

            if (foundData == null)
            {
                CustomDebug.Log($"Do data found for type {ingameType} in {this}");
                return default;
            }

            return foundData.vibrationType;
        }

        #endregion
    }
}
