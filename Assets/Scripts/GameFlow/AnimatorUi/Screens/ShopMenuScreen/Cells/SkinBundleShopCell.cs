using Modules.InAppPurchase;
using Drawmasters.Utils;
using Drawmasters.Levels;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine;


namespace Drawmasters.Ui
{
    public class SkinBundleShopCell : CurrencyBundleShopCell
    {
        #region Fields

        [SerializeField] [Required] private SkeletonGraphic animationUi = default;

        #endregion



        #region Protected methods

        protected override void RefreshVisual(IapsRewardSettings.RewardData rewardData)
        {
            base.RefreshVisual(rewardData);

            if (rewardData != null)
            {
                if (animationUi != null &&
                    rewardData.shooterSkinReward.skinType != ShooterSkinType.None)
                {
                    animationUi.skeletonDataAsset = IngameData.Settings.shooterSkinsSettings.GetSkeletonDataAsset(rewardData.shooterSkinReward.skinType);
                    animationUi.initialSkinName = string.Empty;
                    animationUi.Initialize(true);

                    SpineUtility.SetShooterSkin(rewardData.shooterSkinReward.skinType, animationUi);

                    ShooterAnimationSettings animationSettings = IngameData.Settings.shooterAnimationSettings;
                    animationUi.AnimationState.SetAnimation(0, animationSettings.RandomDanceAnimation, false);
                    animationUi.AnimationState.AddAnimation(0, ShooterAnimation.IdleEmptyAnimation, true, 0f);
                }
            }
        }

        #endregion
    }
}
