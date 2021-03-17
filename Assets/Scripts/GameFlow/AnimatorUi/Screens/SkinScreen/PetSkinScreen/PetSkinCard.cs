using Drawmasters.Announcer;
using Drawmasters.Effects;
using Drawmasters.Levels;
using Drawmasters.Pets;
using Drawmasters.ServiceUtil;
using Drawmasters.Utils;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public class PetSkinCard : ChoosableCard<PetSkinType>
    {
        #region Fields

        [SerializeField] private SkeletonGraphic[] skeletonGraphics = default;

        [SerializeField] private UiPetsChargeProgressBar[] uiPetsChargeProgressBars = default;
        [Space]
        [SerializeField] private Button notBoughtButton = default;
        [Space]
        [SerializeField] private GameObject hidePetInactiveRoot = default;
        [SerializeField] private GameObject petInactiveRoot = default;
        [Space]
        [SerializeField] private GameObject hidePetActiveRoot = default;
        [SerializeField] private GameObject petActiveRoot = default;

        private PetUiSettings petUiSettings;
        private PetAnimationSettings petAnimationSettings;

        private MonopolyCurrencyAnnouncer announcer;
        private PetsChargeController petsChargeController;

        private float savedAnimationTime;
        private string currentAnimationName;

        #endregion



        #region Properties

        public override bool IsActive =>
            GameServices.Instance.PlayerStatisticService.PlayerData.CurrentPetSkin == currentType;

        #endregion



        #region Overrided Methods

        public override void SetupType(PetSkinType type)
        {
            petUiSettings = IngameData.Settings.pets.uiSettings;
            petAnimationSettings = IngameData.Settings.pets.animationSettings;
            petsChargeController = GameServices.Instance.PetsService.ChargeController;

            foreach (var uiPetsChargeProgressBar in uiPetsChargeProgressBars)
            {
                uiPetsChargeProgressBar.SetupPetSkinType(type);
            }

            base.SetupType(type);
        }


        public override void Initialize()
        {
            base.Initialize();

            notBoughtButton.onClick.AddListener(NotBoughtButton_OnClick);

            petsChargeController.OnCharged += PetsChargeController_OnCharged;

            foreach (var bar in uiPetsChargeProgressBars)
            {
                bar.Initialize();
            }
        }


        public override void Deinitialize()
        {
            foreach (var bar in uiPetsChargeProgressBars)
            {
                bar.Deinitialize();
            }

            petsChargeController.OnCharged -= PetsChargeController_OnCharged;

            notBoughtButton.onClick.RemoveListener(NotBoughtButton_OnClick);
            DestroyAnnouncer();

            base.Deinitialize();


            void DestroyAnnouncer()
            {
                if (announcer != null)
                {
                    announcer.Deinitialize();
                    Object.Destroy(announcer.gameObject);
                    announcer = null;

                }
            }
        }


        protected override void InitialRefresh()
        {
            base.InitialRefresh();

            SkeletonDataAsset skeletonDataAsset = IngameData.Settings.pets.skinsSettings.GetSkeletonDataAsset(currentType);

            foreach (var skeletonGraphic in skeletonGraphics)
            {
                skeletonGraphic.skeletonDataAsset = skeletonDataAsset;
                skeletonGraphic.Initialize(true);
            }
        }


        protected override void RefreshIcons()
        {
            base.RefreshIcons();

            RefreshIdleAnimation();
        }


        protected override void OnChangeCardState(ChoosableCardState state)
        {
            SkeletonGraphic skeletonGraphic = skeletonGraphics.Find(e => e.gameObject.activeSelf);
            savedAnimationTime = skeletonGraphic == null || skeletonGraphic.AnimationState == null || skeletonGraphic.AnimationState.GetCurrent(0) == null ? default : skeletonGraphic.AnimationState.GetCurrent(0).TrackTime;

            base.OnChangeCardState(state);

            bool isHidePetCard = currentType == PetSkinType.None;
            CommonUtility.SetObjectActive(hidePetInactiveRoot, state == ChoosableCardState.Inactive && isHidePetCard);
            CommonUtility.SetObjectActive(petInactiveRoot, state == ChoosableCardState.Inactive && !isHidePetCard);

            CommonUtility.SetObjectActive(hidePetActiveRoot, state == ChoosableCardState.Active && isHidePetCard);
            CommonUtility.SetObjectActive(petActiveRoot, state == ChoosableCardState.Active && !isHidePetCard);

            foreach (var skeleton in skeletonGraphics)
            {
                if (skeleton.AnimationState != null &&
                    skeleton.AnimationState.GetCurrent(default) != null)
                {
                    skeleton.AnimationState.GetCurrent(default).TrackTime = savedAnimationTime;
                    skeleton.AnimationState.Update(default);

                }
            }
        }


        protected override bool IsBought(PetSkinType type) =>
            GameServices.Instance.ShopService.PetSkins.IsBought(type);


        protected override Sprite GetIconSprite(PetSkinType type) =>
             IngameData.Settings.pets.skinsSettings.GetSkinUiDisabledSprite(type);


        protected override void OnChooseCard() =>
            GameServices.Instance.PlayerStatisticService.PlayerData.CurrentPetSkin = currentType;

        #endregion



        #region Methods

        private void AttemptCreateAnnouncer()
        {
            if (announcer == null)
            {
                announcer = Instantiate(petUiSettings.petUnboughtAnnouncerPrefab,
                    transform.position,
                    transform.rotation,
                    transform);
            }

            CommonAnnouncer.Data announcerData = petUiSettings.unboughtAnnouncerData;
            announcer.SetupData(announcerData);
        }


        private void AttemptPlayAnnouncer()
        {
            if (!announcer.IsTweenActive)
            {
                announcer.PlayLocal(Vector3.zero, petUiSettings.unboughtAnnouncerOffset);
            }
        }


        private void RefreshIdleAnimation()
        {

            bool isCharged = petsChargeController.IsPetCharged(currentType) ||
                             GameServices.Instance.PetsService.TutorialController.ShouldReckonPetCharged;

            string animationName = isCharged ?
                petAnimationSettings.idleName : petAnimationSettings.sleepIdleAnimationName;

            if (currentAnimationName != animationName)
            {
                foreach (var skeletonGraphic in skeletonGraphics)
                {
                    SpineUtility.SafeSetAnimation(skeletonGraphic, animationName, loop: true);
                }

                currentAnimationName = animationName;
            }
        }

        #endregion



        #region Events handlers

        private void PetsChargeController_OnCharged(PetSkinType petSkinType)
        {
            if (currentType != petSkinType)
            {
                return;
            }

            string wakeUpAnimationName = petAnimationSettings.wakeUpAnimationName;
            foreach (var skeletonGraphic in skeletonGraphics)
            {
                SpineUtility.SafeSetAnimation(skeletonGraphic, wakeUpAnimationName, loop: false, callback: RefreshIdleAnimation);
            }

            //EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIPetBarFilledShineMenu, default, default, transform, TransformMode.Local);
        }


        private void NotBoughtButton_OnClick()
        {
            AttemptCreateAnnouncer();

            Sprite announcerBackground = petUiSettings.unboughtAnnouncerBackgroundSprite;
            string announcerTextKey = petUiSettings.FindUnboughtAnnouncerKey(currentType);
            announcer.SetupValues(announcerBackground, announcerTextKey);

            AttemptPlayAnnouncer();
        }

        #endregion
    }
}
