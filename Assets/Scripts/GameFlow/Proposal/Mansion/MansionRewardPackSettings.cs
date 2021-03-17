using System;
using System.Collections.Generic;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Mansion
{
    [CreateAssetMenu(fileName = "MansionRewardPackSettings",
                 menuName = NamingUtility.MenuItems.ProposalSettings + "MansionRewardPackSettings")]
    public class MansionRewardPackSettings : ScriptableObject
    {
        #region Nested Types

        [Serializable]
        public class RoomObjectsData
        {
            public MansionRoomObjectType type = default;
            public int totalUpgrades = default;
            public CurrencyReward reward = default;
        }

        [Serializable]
        private class PreviewIcon
        {
            public ShooterSkinType skinType = default;

            public Sprite previewIconSprite = default;
        }

        #endregion



        #region Fields

        [Header("Hammers")]
        public CurrencyReward hammersForVideoData = default;
        public CurrencyReward hammersForCurrencyData = default;

        [Header("Keys")]
        public CurrencyReward keysForVideoData = default;
        public CurrencyReward freeKeysData = default;

        [Header("Room")]
        [SerializeField] private MansionRoomData[] mansionRoomData = default;
        [SerializeField] private RoomObjectsData[] roomObjectsData = default;

        [Header("Preview icon")]
        [SerializeField] private List<PreviewIcon> previewIconsList = default;

        [Header("Force show")]
        public FactorAnimation fadeAnimation = default;
        public float beginInteractibleDelay = default;
        public float tapDelay = default;
        public float tapDuration = default;
        public float afterTapEndDelay = default;

        [Header("Tutorial")]
        public MansionRoomObjectType mansionRoomObjectTypeForTutorial = default;

        #endregion



        #region Methods

        public int FindObjectTotalUpgrades(MansionRoomObjectType type)
        {
            RoomObjectsData foundData = Array.Find(roomObjectsData, e => e.type == type);
            return foundData == null ? default : foundData.totalUpgrades;
        }


        public CurrencyReward FindObjectReward(MansionRoomObjectType type)
        {
            RoomObjectsData foundData = Array.Find(roomObjectsData, e => e.type == type);
            return foundData == null ? default : foundData.reward;
        }


        public float GetCurrencyPassiveIncomeCooldown(int i) =>
            GameServices.Instance.AbTestService.CommonData.mansionIncomeCooldown.SafeGet(i);


        public MansionRoomData FindMansionRoomData(int i)
        {
            if (i < 0 || i > mansionRoomData.Length)
            {
                CustomDebug.Log($"No data for room with number {i}");
                return default;
            }

            return mansionRoomData[i];
        }


        public Sprite GetPreviewSprite(ShooterSkinType skinType)
        {
            Sprite result = default;

            PreviewIcon data = previewIconsList.Find(i => i.skinType == skinType);
            if (data != null && data.previewIconSprite != null)
            {
                result = data.previewIconSprite;
            }
            else
            {
                //TODO discuss
                //CustomDebug.Log("Missing preview sprite. Skin type : " + skinType);
            }

            return result;
        }

        #endregion



        #region Editor

        private void OnValidate()
        {
            hammersForCurrencyData.priceType = CurrencyType.Premium;
            hammersForCurrencyData.receiveType = RewardDataReceiveType.Currency;
            hammersForCurrencyData.currencyType = CurrencyType.MansionHammers;

            hammersForVideoData.receiveType = RewardDataReceiveType.Video;
            hammersForVideoData.currencyType = CurrencyType.MansionHammers;

            keysForVideoData.receiveType = RewardDataReceiveType.Video;
        }

        #endregion
    }
}
