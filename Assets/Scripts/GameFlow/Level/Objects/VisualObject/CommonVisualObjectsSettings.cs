using UnityEngine;
using System;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "CommonVisualObjectsSettings",
                 menuName = NamingUtility.MenuItems.IngameSettings + "CommonVisualObjectsSettings")]
    public class CommonVisualObjectsSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        private class Data
        {
            public WeaponType allowedWeaponType = default;
            public Sprite[] sprites = default;
            public int[] uaSpriteIndexesToDisable = default;
        }

        #endregion



        #region Fields

        [SerializeField] private Data[] data = default;

        #endregion



        #region Methods

        public Sprite[] FindSprites(WeaponType weaponType)
        {
            Data foundData = FindData(weaponType);

            return foundData == null ? null : foundData.sprites;
        }


        public Sprite FindSprite(WeaponType weaponType, int index)
        {
            Data foundData = FindData(weaponType);

            return foundData == null || index >= foundData.sprites.Length ? null : foundData.sprites[index];
        }


        public bool IsUaGroupSprite(WeaponType weaponType, int index)
        {
            Data foundData = FindData(weaponType);

            return foundData == null ? default : Array.Exists(foundData.uaSpriteIndexesToDisable, e => e == index);
        }


        private Data FindData(WeaponType weaponType)
        {
            Data foundData = Array.Find(data, element => element.allowedWeaponType == weaponType);

            if (foundData == null)
            {
                CustomDebug.Log($"No sprite for weapon {weaponType} found in {this}");
                return null;
            }

            return foundData;
        }

        #endregion
    }
}
