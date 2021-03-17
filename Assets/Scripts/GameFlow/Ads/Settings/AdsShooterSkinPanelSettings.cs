using UnityEngine;
using System.Linq;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Proposal
{
    [CreateAssetMenu(fileName = "AdsShooterSkinPanelSettings",
                 menuName = NamingUtility.MenuItems.ProposalSettings + "AdsShooterSkinPanelSettings")]
    public class AdsShooterSkinPanelSettings : AdsSkinPanelsSettings
    {
        #region Fields

        [SerializeField] private ShooterSkinReward[] reward = default;
        [SerializeField] private ShooterSkinReward[] sequenceReward = default;

        #endregion



        #region Properties

        protected override float ButtonCooldown =>
            GameServices.Instance.AbTestService.CommonData.videoShooterSkinProposalCooldown;

        protected override string TimerPostfix =>
            PrefsKeys.Proposal.ShooterSkinsPanelButtonTimer;

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
            if (data is ShooterSkinReward rewardData)
            {
                bool isBought = GameServices.Instance.ShopService.ShooterSkins.IsBought(rewardData.skinType);
                return !isBought;
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
