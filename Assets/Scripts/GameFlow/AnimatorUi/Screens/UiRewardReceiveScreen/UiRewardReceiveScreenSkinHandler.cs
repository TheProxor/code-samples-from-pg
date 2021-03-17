using Drawmasters.Levels;
using Drawmasters.Proposal;
using Drawmasters.Utils;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using System;


namespace Drawmasters.Ui
{
    public class UiRewardReceiveScreenSkinHandler
    {
        #region Helpers

        [Serializable]
        public class Data
        {
            public SkeletonGraphic shooterSkinAnimation = default;
            public GameObject[] shooterRewardRoots = default;

            public GameObject[] weaponRewardRoots = default;
            public Image weaponSkinIcon = default;

            public SkeletonGraphic petSkinAnimation = default;
            public GameObject[] petRewardRoots = default;
        }

        #endregion



        #region Fields

        private readonly Vector3 scaleWeaponIconRoot = Vector3.one * 3.0f;

        private readonly Data data;

        #endregion


        #region Class lifecycle

        public UiRewardReceiveScreenSkinHandler(Data _data)
        {
            data = _data;
            HideAll();
        }

        #endregion



        #region Methods

        public void SetupReward(RewardData rewardData)
        {
            CommonUtility.SetObjectActive(data.shooterSkinAnimation.gameObject, rewardData.Type == RewardType.ShooterSkin);
            CommonUtility.SetObjectsActive(data.shooterRewardRoots, rewardData.Type == RewardType.ShooterSkin);

            CommonUtility.SetObjectActive(data.weaponSkinIcon.gameObject, rewardData.Type == RewardType.WeaponSkin);
            CommonUtility.SetObjectsActive(data.weaponRewardRoots, rewardData.Type == RewardType.WeaponSkin);

            CommonUtility.SetObjectActive(data.petSkinAnimation.gameObject, rewardData.Type == RewardType.PetSkin);
            CommonUtility.SetObjectsActive(data.petRewardRoots, rewardData.Type == RewardType.PetSkin);

            if (rewardData is ShooterSkinReward shooterSkinReward)
            {
                SetupShooterSkin(shooterSkinReward.skinType);
            }
            else if (rewardData is WeaponSkinReward weaponSkinReward)
            {
                SetupWeaponSkin(weaponSkinReward.skinType);
            }
            else if (rewardData is PetSkinReward petSkinReward)
            {
                SetupPetSkin(petSkinReward.skinType);
            }
            else
            {
                CustomDebug.Log($"Not implemented logic for type {rewardData.Type} in {this}");
            }
        }


        public void HideAll()
        {
            CommonUtility.SetObjectActive(data.shooterSkinAnimation.gameObject, false);
            CommonUtility.SetObjectsActive(data.shooterRewardRoots, false);

            CommonUtility.SetObjectActive(data.weaponSkinIcon.gameObject, false);
            CommonUtility.SetObjectsActive(data.weaponRewardRoots, false);

            CommonUtility.SetObjectActive(data.petSkinAnimation.gameObject, false);
            CommonUtility.SetObjectsActive(data.petRewardRoots, false);
        }


        private void SetupShooterSkin(ShooterSkinType shooterSkinType)
        {
            CommonUtility.SetObjectActive(data.shooterSkinAnimation.gameObject, true);
            CommonUtility.SetObjectActive(data.weaponSkinIcon.gameObject, false);
            CommonUtility.SetObjectActive(data.petSkinAnimation.gameObject, false);

            data.shooterSkinAnimation.skeletonDataAsset = IngameData.Settings.shooterSkinsSettings.GetSkeletonDataAsset(shooterSkinType);
            data.shooterSkinAnimation.initialSkinName = string.Empty;
            data.shooterSkinAnimation.Initialize(true);

            SpineUtility.SetShooterSkin(shooterSkinType, data.shooterSkinAnimation);

            ShooterAnimationSettings animationSettings = IngameData.Settings.shooterAnimationSettings;
            data.shooterSkinAnimation.AnimationState.SetAnimation(0, animationSettings.RandomDanceAnimation, false);
            data.shooterSkinAnimation.AnimationState.AddAnimation(0, ShooterAnimation.IdleEmptyAnimation, true, 0f);
        }


        private void SetupWeaponSkin(WeaponSkinType weaponSkinType)
        {
            CommonUtility.SetObjectActive(data.shooterSkinAnimation.gameObject, false);
            CommonUtility.SetObjectActive(data.weaponSkinIcon.gameObject, true);
            CommonUtility.SetObjectActive(data.petSkinAnimation.gameObject, false);

            WeaponSkinSettings settings = IngameData.Settings.weaponSkinSettings;
            bool isResultSpriteExists = settings.TryGetSkinUiResultSprite(weaponSkinType, out Sprite resultUiSprite);

            Sprite rewardSprite = isResultSpriteExists ? resultUiSprite : settings.GetSkinUiSprite(weaponSkinType);
            data.weaponSkinIcon.sprite = rewardSprite;
            data.weaponSkinIcon.SetNativeSize();

            Vector3 imageScale = isResultSpriteExists ? Vector3.one : scaleWeaponIconRoot;
            data.weaponSkinIcon.rectTransform.localScale = imageScale;
        }


        private void SetupPetSkin(PetSkinType skinType)
        {
            CommonUtility.SetObjectActive(data.shooterSkinAnimation.gameObject, false);
            CommonUtility.SetObjectActive(data.weaponSkinIcon.gameObject, false);
            CommonUtility.SetObjectActive(data.petSkinAnimation.gameObject, true);

            data.petSkinAnimation.skeletonDataAsset = IngameData.Settings.pets.uiSettings.FindMainMenuSkeletonData(skinType);
            data.petSkinAnimation.Initialize(true);
        }

        #endregion

    }
}
