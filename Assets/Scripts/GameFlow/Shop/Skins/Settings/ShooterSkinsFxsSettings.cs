using System;
using System.Linq;
using UnityEngine;
using Drawmasters.Levels;
using Spine.Unity;


namespace Drawmasters
{
    [CreateAssetMenu(fileName = "ShooterSkinsFxsSettings",
                    menuName = NamingUtility.MenuItems.IngameSettings + "ShooterSkinsFxsSettings")]
    public class ShooterSkinsFxsSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        public class Data
        {
            public ShooterColorType[] colorTypes = default;

            [Tooltip("For bones serialization predominantly")] public SkeletonDataAsset dataAsset = default;

            [Enum(typeof(EffectKeys))] public string fxKey = default;
            [SpineBone(dataField = "dataAsset")] public string boneName = default;
            public ShooterSkinType skinType = default;
        }

        #endregion



        #region Fields

        [Header("Fxs")]
        public Data[] fxData = default;

        #endregion



        #region Methods

        public bool IsAnyDataExists(ShooterSkinType skinType, ShooterColorType colorType) =>
            Array.Exists(fxData, e => Array.Exists(e.colorTypes, de => de == colorType) && e.skinType == skinType);


        public (string fxKey, string boneName)[] FindFxsKeyAndBoneName(ShooterSkinType skinType, ShooterColorType colorType)
        {
            Data[] data = FindFxsData(skinType, colorType);
            return data == null ? default : data.Select(e => (e.fxKey, e.boneName)).ToArray();
        }


        private Data[] FindFxsData(ShooterSkinType skinType, ShooterColorType colorType)
        {
            Data[] foundInfo = Array.FindAll(fxData, e => Array.Exists(e.colorTypes, de => de == colorType) && e.skinType == skinType);

            if (foundInfo == null)
            {
                CustomDebug.Log($"No data found for colorType {colorType} and skinType {skinType} in {this}");
            }

            return foundInfo;
        }

        #endregion
    }
}
