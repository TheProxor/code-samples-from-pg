using System;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Proposal
{
    [Serializable]
    public class ShooterSkinReward : RewardData, IForcemeterReward
    {
        #region Fields

        public ShooterSkinType skinType = default;

        public ShooterSkinReward()
        {
        }

        #endregion



        #region Properties

        public override RewardType Type => RewardType.ShooterSkin;
        public override bool IsAvailableForRewardPack => !GameServices.Instance.ShopService.ShooterSkins.IsBought(skinType);

        public override string RewardText
            => string.Format(AdsVideoRewardKeys.ShooterSkinFormat, skinType);

        #endregion



        #region Methods

        public override void Open() => GameServices.Instance.ShopService.ShooterSkins.Open(skinType);

        public override void Apply()
        {
            base.Apply();
            
            GameServices.Instance.PlayerStatisticService.PlayerData.CurrentSkin = skinType;
        }


        public override Sprite GetUiSprite() =>
            IngameData.Settings.shooterSkinsSettings.GetSkinUiSprite(skinType);


        public Sprite GetForcemeterRewardSprite() =>
            IngameData.Settings.forceMeterRewardPackSettings.GetShooterSkinSprite(skinType);


        public override bool IsEqualsReward(RewardData anotherReward)
        {
            bool result = base.IsEqualsReward(anotherReward);

            if (anotherReward is ShooterSkinReward shooterSkinReward)
            {
                result = skinType == shooterSkinReward.skinType;
            }

            return result;
        }

        #endregion
    }
}
