using System;
using UnityEngine;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "CoinLevelObjectSettings",
                     menuName = NamingUtility.MenuItems.IngameSettings + "CoinLevelObjectSettings")]
    public class CoinLevelObjectSettings : ScriptableObjectData<CoinLevelObjectSettings.Data, CurrencyType>
    {
        #region Nested types

        [Serializable]
        public class Data : ScriptableObjectBaseData<CurrencyType>
        {
            public Mesh mesh = default;
            public Material material = default;
        }

        #endregion



        #region Fields

        [Header("AB Test")]
        public CurrencyType mansionReplacedCurrencyType = default;

        [Header("Idle animation")]
        public float idleRotateDuration = default;

        [Header("Collect animation")]
        [Enum(typeof(EffectKeys))] public string fxKeyOnCollect = default;
        public VectorAnimation scaleCollectAnimation = default;

        [Header("Collect announceranimation")]
        public Vector3 announcerOffsetMove = default;
        public VectorAnimation announcerMoveAnimation = default;
        public FactorAnimation announcerAlphaAnimation = default;
        public VectorAnimation announcerScaleAnimation = default;

        [Header("Absorb")]
        public VectorAnimation absorbAnimation = default;

        #endregion



        #region Methods

        public Mesh FindMesh(CurrencyType type)
        {
            Data settings = FindData(type);
            return settings == null ? default : settings.mesh;
        }


        public Material FindMaterial(CurrencyType type)
        {
            Data settings = FindData(type);
            return settings == null ? default : settings.material;
        }

        #endregion
    }
}
