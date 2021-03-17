using System;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Proposal
{
    [Serializable]
    public class PetSkinReward : RewardData, IForcemeterReward
    {
        #region Fields

        public PetSkinType skinType = default;

        public float petChargePoints = default;

        public PetSkinReward()
        {
        }

        #endregion



        #region Properties

        public override RewardType Type =>
            RewardType.PetSkin;


        public override bool IsAvailableForRewardPack =>
            !GameServices.Instance.ShopService.PetSkins.IsBought(skinType);


        public override string RewardText =>
            string.Format(AdsVideoRewardKeys.PetSkinFormat, skinType);

        #endregion



        #region Methods

        public override void Open() =>
            GameServices.Instance.ShopService.PetSkins.Open(skinType);

        public override void Apply()
        {
            base.Apply();

            GameServices.Instance.PlayerStatisticService.PlayerData.CurrentPetSkin = skinType;

            GameServices.Instance.PetsService.ChargeController.ResetCharge();
            GameServices.Instance.PetsService.ChargeController.AddChargePoints(petChargePoints);
            GameServices.Instance.PetsService.ChargeController.ApplyChargePoints();
        }


        public override Sprite GetUiSprite() =>
            IngameData.Settings.pets.skinsSettings.GetSkinUiSprite(skinType);


        public Sprite GetForcemeterRewardSprite() =>
            IngameData.Settings.forceMeterRewardPackSettings.GetPetSkinSprite(skinType);


        public override bool IsEqualsReward(RewardData anotherReward)
        {
            bool result = base.IsEqualsReward(anotherReward);

            if (anotherReward is PetSkinReward petSkinReward)
            {
                result = skinType == petSkinReward.skinType;
            }

            return result;
        }

        #endregion
    }
}
