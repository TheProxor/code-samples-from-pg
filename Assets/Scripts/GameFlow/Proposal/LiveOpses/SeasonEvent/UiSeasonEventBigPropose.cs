using System;
using Drawmasters.Effects;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;


namespace Drawmasters.Ui
{
    [Serializable]
    public class UiSeasonEventBigPropose : UiSeasonEventBasePropose
    {
        #region Fields

        [SerializeField] private GameObject petRewardRoot = default;
        [SerializeField] private SkeletonGraphic petSkeletonGraphic = default;

        [SerializeField] private GameObject commonRewardRoot = default;
        [SerializeField] private Image commonRewardImage = default;

        [SerializeField] private UiSeasonEventBar progressBar = default;
        
        private SkeletonGraphicEffect petIdleEffect;
        
        #endregion



        #region Methods

        public override void Initialize()
        {
            base.Initialize();
            
            petIdleEffect = petIdleEffect ?? new SkeletonGraphicEffect(string.Empty, transform);

            progressBar.Initialize();
        }


        public override void Deinitialize()
        {
            petIdleEffect?.StopEffect();
            progressBar.Deinitialize();
            
            base.Deinitialize();
        }
        

        protected override void OnShouldRefreshVisual()
        {
            base.OnShouldRefreshVisual();

            commonRewardImage.sprite = controller.VisualSettings.commonRewardSprite;
            bool isPetMainReward = controller.IsPetMainReward;

            if (isPetMainReward)
            {
                PetUiSettings petUiSettings = IngameData.Settings.pets.uiSettings;
                petSkeletonGraphic.skeletonDataAsset =
                    petUiSettings.FindMainMenuSkeletonData(controller.PetMainRewardType);
                petSkeletonGraphic.Initialize(true);

                petIdleEffect.StopEffect();

                string idleFxKey = petUiSettings.FindMainMenuIdleFxKey(controller.PetMainRewardType);
                string boneName = petUiSettings.FindMainMenuBoneName(controller.PetMainRewardType);

                if (!string.IsNullOrEmpty(idleFxKey) && !string.IsNullOrEmpty(boneName))
                {
                    petIdleEffect.SetupFxKey(idleFxKey);

                    petIdleEffect.SetupBoneName(petSkeletonGraphic, boneName);
                    petIdleEffect.CreateAndPlayEffect();
                }
            }

            CommonUtility.SetObjectActive(petRewardRoot, isPetMainReward);
            CommonUtility.SetObjectActive(commonRewardRoot, !isPetMainReward);
        }

        #endregion
    }
}