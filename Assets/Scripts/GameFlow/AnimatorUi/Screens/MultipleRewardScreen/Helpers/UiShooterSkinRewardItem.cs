using Drawmasters.Levels;
using Drawmasters.Proposal;
using Drawmasters.Utils;
using Spine.Unity;
using UnityEngine;

namespace Drawmasters.Ui
{
    public class UiShooterSkinRewardItem : UiRewardItem
    {
        #region Fields

        [SerializeField] private SkeletonGraphic skeletonGraphic = default;

        #endregion



        #region Overrided

        public override void InitializeUiRewardItem(RewardData _rewardData, int sortingOrder)
        {
            base.InitializeUiRewardItem(_rewardData, sortingOrder);
            
            ApplyVisual(_rewardData as ShooterSkinReward);
        }

        #endregion



        #region Private methods

        private void ApplyVisual(ShooterSkinReward reward)
        {
            bool isCorrectReward = reward != null && reward.skinType != ShooterSkinType.None;
            CommonUtility.SetObjectActive(skeletonGraphic.gameObject, isCorrectReward);

            if (!isCorrectReward)
            {
                return;
            }

            skeletonGraphic.skeletonDataAsset = IngameData.Settings.shooterSkinsSettings.GetSkeletonDataAsset(reward.skinType);
            skeletonGraphic.initialSkinName = string.Empty;
            skeletonGraphic.Initialize(true);

            SpineUtility.SetShooterSkin(reward.skinType, skeletonGraphic);

            ShooterAnimationSettings animationSettings = IngameData.Settings.shooterAnimationSettings;
            skeletonGraphic.AnimationState.SetAnimation(0, animationSettings.RandomDanceAnimation, false);
            skeletonGraphic.AnimationState.AddAnimation(0, ShooterAnimation.IdleEmptyAnimation, true, 0f);
        }

        #endregion
    }
}