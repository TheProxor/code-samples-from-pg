using Drawmasters;
using System;
using System.Collections.Generic;
using UnityEngine;
using Drawmasters.Proposal;


namespace Modules.InAppPurchase
{
    [CreateAssetMenu(fileName = "IapsRewardSettings",
                     menuName = NamingUtility.MenuItems.Settings + "IapsRewardSettings")]
    public class IapsRewardSettings : ScriptableObject
    {
        #region Helpers

        [Serializable]
        public class RewardData
        {
            [Enum(typeof(IAPs.Keys.Consumable))] public string storeId = default;

            public CurrencyReward[] currencyData = default;
            public ShooterSkinReward shooterSkinReward = default;
        }
         
        [Serializable]
        public class ProposeRewardData
        {
            public string id = default;
            public Sprite uiSprite = default;
        }


        [Serializable]
        public class HeaderData
        {
            public CurrencyType[] types = default;
            public string text = default;
        }

        #endregion



        #region Fields

        [SerializeField] private List<RewardData> rewardsData = default;

        [Header("Propose MiniBank")]
        [SerializeField] private ProposeRewardData[] proposeRewardData = default;
        [SerializeField] private HeaderData[] headerData = default;

        #endregion



        #region Public methods

        public RewardData GetRewardData(string storeId)
        {
            RewardData foundData = rewardsData.Find(i => i.storeId == storeId);

            if (foundData == null)
            {
                // CustomDebug.Log("Cannot find reward data. Store id : " + storeId);
            }

            return foundData;
        }

        public RewardData[] GetBankProposeRewardData(CurrencyType currencyType)
        {
            List<RewardData> foundData = rewardsData.FindAll(i => Array.Exists(i.currencyData, d => d.currencyType == currencyType) &&
                                                                  Array.Exists(proposeRewardData, k => k.id == i.storeId));

            // if (foundData == null)
            // {
            //     CustomDebug.Log("Cannot find bank propose reward data. CurrencyType : " + currencyType);
            // }

            return foundData.ToArray();
        }


        public Sprite FindProposeUiSprite(string id)
        {
            ProposeRewardData foundData = Array.Find(proposeRewardData, e => e.id.Equals(id, StringComparison.Ordinal));
            return foundData == null ? default : foundData.uiSprite;
        }


        public string FindHeaderKey(CurrencyType type)
        {
            HeaderData foundData = Array.Find(headerData, e => Array.Exists(e.types, t => t == type));
            return foundData == null ? string.Empty : foundData.text;
        }
        
        #endregion
    }
}

