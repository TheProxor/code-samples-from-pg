using UnityEngine;
using System.Linq;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Proposal
{
    [CreateAssetMenu(fileName = "AdsWeaponSkinPanelSettings",
                 menuName = NamingUtility.MenuItems.ProposalSettings + "AdsWeaponSkinPanelSettings")]
    public class AdsWeaponSkinPanelSettings : AdsSkinPanelsSettings
    {
        #region Fields

        [SerializeField] private WeaponSkinReward[] reward = default;
        [SerializeField] private WeaponSkinReward[] sequenceReward = default;

        #endregion



        #region Properties

        protected override float ButtonCooldown =>
            GameServices.Instance.AbTestService.CommonData.videoWeaponSkinProposalCooldown;

        protected override string TimerPostfix =>
            PrefsKeys.Proposal.WeaponSkinsPanelButtonTimer;

        protected override RewardData[] CommonAvailableRewards =>
            reward.Where(e => IsRewardAvailable(e)).ToArray();

        #endregion



        #region Methods

        protected override RewardData GetSequenceReward(int showIndex)
        {
            if (showIndex >= sequenceReward.Length)
            {
                CustomDebug.Log($"Wrong index");
                return default;
            }

            return sequenceReward[showIndex];
        }


        protected override bool ShouldProposeSequenceReward(int showIndex) =>
            showIndex < sequenceReward.Length;


        protected override bool IsRewardAvailable(RewardData data)
        {
            if (data is WeaponSkinReward rewardData)
            {
                bool isBought = GameServices.Instance.ShopService.WeaponSkins.IsBought(rewardData.skinType);
                bool isAvailableFill = true;

                if (availableWeaponType != WeaponType.None)
                {
                    WeaponType rewardWeaponType = IngameData.Settings.weaponSkinSettings.GetWeaponType(rewardData.skinType);

                    isAvailableFill = rewardWeaponType.Equals(availableWeaponType);
                }

                return !isBought && isAvailableFill;
            }
            else
            {
                CustomDebug.Log($"Wrong check logic detected in {this} for reward data");
                return false;
            }
        }

        #endregion
    }
}
