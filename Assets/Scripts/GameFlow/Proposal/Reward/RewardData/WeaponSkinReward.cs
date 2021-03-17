using System;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Proposal
{
    [Serializable]
    public class WeaponSkinReward : RewardData, IForcemeterReward
    {
        #region Fields

        public WeaponSkinType skinType = default;

        public WeaponSkinReward()
        {
        }

        #endregion



        #region Properties

        public override RewardType Type => RewardType.WeaponSkin;

        public override bool IsAvailableForRewardPack => !GameServices.Instance.ShopService.WeaponSkins.IsBought(skinType);

        public override string RewardText
            => string.Format(AdsVideoRewardKeys.WeaponSkinFormat, skinType);

        #endregion



        #region Methods

        public override void Open() => GameServices.Instance.ShopService.WeaponSkins.Open(skinType);

        public override void Apply()
        {
            base.Apply();

            WeaponType weaponType = IngameData.Settings.weaponSkinSettings.GetWeaponType(skinType);            

            GameServices.Instance.PlayerStatisticService.PlayerData.SetWeaponSkin(weaponType, skinType);
        }


        public override Sprite GetUiSprite() =>
            IngameData.Settings.weaponSkinSettings.GetSkinUiSprite(skinType);


        public Sprite GetForcemeterRewardSprite() =>
            IngameData.Settings.forceMeterRewardPackSettings.GetWeaponSkinSprite(skinType);


        public override bool IsEqualsReward(RewardData anotherReward)
        {
            bool result = base.IsEqualsReward(anotherReward);

            if (anotherReward is WeaponSkinReward skinReward)
            {
                result = skinType == skinReward.skinType;
            }

            return result;
        }

        #endregion
    }
}
