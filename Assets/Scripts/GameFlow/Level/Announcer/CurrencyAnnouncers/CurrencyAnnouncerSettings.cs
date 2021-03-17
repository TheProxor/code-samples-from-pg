using UnityEngine;
using System;
using Drawmasters.Levels;


namespace Drawmasters.Announcer
{
    [CreateAssetMenu(fileName = "CurrencyAnnouncerSettings",
                     menuName = NamingUtility.MenuItems.IngameSettings + "CurrencyAnnouncerSettings")]
    public class CurrencyAnnouncerSettings : ScriptableObjectData<CurrencyAnnouncerSettings.Data, CurrencyType>
    {
        #region Nested types

        [Serializable]
        public class Data : ScriptableObjectBaseData<CurrencyType>
        {
            public Sprite sprite = default;
        }

        #endregion



        #region Fields

        public Vector3 announcerOffsetMove = default;
        public VectorAnimation announcerMoveAnimation = default;
        public FactorAnimation announcerAlphaAnimation = default;
        public VectorAnimation announcerScaleAnimation = default;

        #endregion



        #region Methods

        public Sprite FindSprite(CurrencyType type)
        {
            Data foundData = FindData(type);
            return foundData == null ? default : foundData.sprite;
        }

        #endregion
    }
}
