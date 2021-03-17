using System;
using UnityEngine;
using Drawmasters.ServiceUtil;
using Drawmasters.Proposal;
using Drawmasters.Levels;
using Drawmasters.ServiceUtil.Interfaces;
using System.Collections.Generic;
using System.Linq;


namespace Drawmasters
{
    [CreateAssetMenu(fileName = "SubscriptionRewardSettings",
                  menuName = NamingUtility.MenuItems.IngameSettings + "SubscriptionRewardSettings")]
    public class SubscriptionRewardSettings : ScriptableObject
    {
        #region Fields

        [Header("Reward")]
        [SerializeField] private RewardDataInspectorSerialization[] rewards = default;

        [Header("Additional reward to cancel (Old users support)")]
        [SerializeField] private RewardDataInspectorSerialization[] oldUsersReward = default;

        [SerializeField] private ColorFxData[] fxData = default;

        #endregion



        #region Properties

        public float CurrencyRewardPerDay =>
            GameServices.Instance.AbTestService.CommonData.subscriptionReward;


        public float PremiumCurrencyRewardPerDay =>
            GameServices.Instance.AbTestService.CommonData.subscriptionPremiumCurrencyReward;


        public ShooterSkinType[] ShooterSkinTypes =>
            rewards.Select(e => e.RewardData).OfType<ShooterSkinReward>().Select(e => e.skinType).ToArray();


        public WeaponSkinType[] WeaponSkinTypes =>
            rewards.Select(e => e.RewardData).OfType<WeaponSkinReward>().Select(e => e.skinType).ToArray();


        private RewardDataSerializationArray RewardDataToCancel { get; set; }

        private bool ShouldCancelOldUsersReward =>
           !CustomPlayerPrefs.HasKey(PrefsKeys.Subscription.WasOldUsersRewardCancelCheck);

        #endregion



        #region Methods

        public void Initialize()
        {
            if (ShouldCancelOldUsersReward)
            {
                RewardDataToCancel = new RewardDataSerializationArray(PrefsKeys.Subscription.WasOldUsersRewardCancelCheck)
                {
                    Data = oldUsersReward.Select(e => e.RewardData).Where(e => !e.IsAvailableForRewardPack).ToArray()
                };
            }
            else
            {
                CustomPlayerPrefs.SetBool(PrefsKeys.Subscription.WasOldUsersRewardCancelCheck, true);
            }
        }

        public bool IsSkinForSubscription(ShooterSkinType type) =>
            Array.Exists(ShooterSkinTypes, e => e == type);


        public bool IsSkinForSubscription(WeaponSkinType type) =>
            Array.Exists(WeaponSkinTypes, e => e == type);


        public void OpenSubscriptionReward()
        {
            foreach (var reward in rewards)
            {
                reward.RewardData.Open();
            }
        }


        public void CancelSubscriptionReward()
        {
            IShopService shopService = GameServices.Instance.ShopService;

            List<ShooterSkinType> shooterSkinTypes = new List<ShooterSkinType>();
            shooterSkinTypes.AddRange(ShooterSkinTypes);
            if (ShouldCancelOldUsersReward)
            {
                shooterSkinTypes.AddRange(RewardDataToCancel.Data.OfType<ShooterSkinReward>().Select(e => e.skinType));

            }

            foreach (var shooterSkinType in shooterSkinTypes)
            {
                if (shopService.ShooterSkins.IsBought(shooterSkinType))
                {
                    shopService.ShooterSkins.CancelBought(shooterSkinType);
                }
            }

            List<WeaponSkinType> weaponSkinTypes = new List<WeaponSkinType>();
            weaponSkinTypes.AddRange(WeaponSkinTypes);
            if (ShouldCancelOldUsersReward)
            {
                weaponSkinTypes.AddRange(RewardDataToCancel.Data.OfType<WeaponSkinReward>().Select(e => e.skinType));
            }

            foreach (var weaponSkinType in weaponSkinTypes)
            {
                if (shopService.WeaponSkins.IsBought(weaponSkinType))
                {
                    shopService.WeaponSkins.CancelBought(weaponSkinType);
                }
            }
        }


        public (string fxKey, string boneName) FindFxsKeyAndBoneName(ShooterColorType colorType)
        {
            ColorFxData data = FindFxsData(colorType);
            return data == null ? default : (data.fxKey, data.boneName);
        }


        private ColorFxData FindFxsData(ShooterColorType colorType)
        {
            ColorFxData foundInfo = Array.Find(fxData, e => e.key == colorType);

            if (foundInfo == null)
            {
                CustomDebug.Log($"No data found for colorType {colorType} in {this}");
            }

            return foundInfo;
        }

        #endregion
    }
}
