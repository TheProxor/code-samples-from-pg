using Drawmasters.ServiceUtil;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using JoystickPlugin;
using Drawmasters.Pets;
using System;
using DG.Tweening;
using Drawmasters.Effects;
using Drawmasters.Levels;
using Drawmasters.Tutorial;
using Drawmasters.Utils;
using Spine.Unity;
using Drawmasters.Levels.Data;


namespace Drawmasters.Ui
{
    public class PetIngameScreenBehaviour
    {
        #region Helpers

        [Serializable]
        public class Data
        {
            [Header("Pets")] [Required] public SkeletonGraphic[] skeletonGraphics = default;
            [Required] public UiPetsChargeIngameProgressBar chargeInactiveBar = default;
            [Required] public UiPetsChargeProgressBar chargeActiveBar = default;
            [Required] public UiPetsChargeInterimProgressBar[] interimChargeBars = default;

            [Required] public GameObject callPetButtonMainRoot = default;
            [Required] public Button callPetButtonActive = default;
            [Required] public Button callPetButtonInactive = default;
            [Required] public DynamicJoystick petMoveJoystick = default;

            [Required] public CanvasGroup whiteBackImageCanvasGroup = default;

            [Header("Fx")] [Required] public Transform fxRoot = default;

            [Header("Tutorial")] [Required] public Transform tutorialRoot = default;
        }

        #endregion


        
        #region Fields

        public event Action OnInvokePet;

        private readonly Data data;

        private Animator tutorialMoveAnimator;

        private EffectHandler effectHandler;

        private PetsChargeController petsChargeController;

        private PetAnimationSettings petAnimationSettings;

        private PetsInvokeController petsInvokeController;

        private PetSkinType petSkinType;

        private string currentAnimationName;

        private bool isCallPetButtonAvailable;

        #endregion



        #region Class lifecycle

        public PetIngameScreenBehaviour(Data _data)
        {
            data = _data;
        }

        #endregion


        
        #region Methods

        public void Initialize(bool _isCallPetButtonAvailable, LevelContext _levelContext)
        {
            petsChargeController = GameServices.Instance.PetsService.ChargeController;
            petsChargeController.OnCharged += PetsChargeController_OnCharged;

            petAnimationSettings = IngameData.Settings.pets.animationSettings;
            petSkinType = GameServices.Instance.PlayerStatisticService.PlayerData.CurrentPetSkin;

            petsInvokeController = GameServices.Instance.PetsService.InvokeController;

            isCallPetButtonAvailable = _isCallPetButtonAvailable &&
                                       petsChargeController.IsActive &&
                                       !_levelContext.Mode.IsHitmastersLiveOps();
            
            PetInvokeComponent.OnShouldInvokePetForLevel += PetInvokeComponent_OnShouldInvokePetForLevel;

            RefreshVisual();

            data.petMoveJoystick.SetActive(false);

            data.chargeActiveBar.SetupPetSkinType(petSkinType);
            data.chargeActiveBar.Initialize();

            data.chargeInactiveBar.Initialize();

            foreach (var chargeBar in data.interimChargeBars)
            {
                chargeBar.Initialize();
            }

            GameServices.Instance.LevelControllerService.SoulTrailController.OnTrailMoveFinish += SoulTrailController_OnTrailMoveFinish;
            GameServices.Instance.LevelControllerService.SoulTrailController.OnQueueEmpty += SoulTrailController_OnQueueEmpty;
        }


        public void Deinitialize()
        {
            GameServices.Instance.LevelControllerService.SoulTrailController.OnTrailMoveFinish -= SoulTrailController_OnTrailMoveFinish;
            GameServices.Instance.LevelControllerService.SoulTrailController.OnQueueEmpty -= SoulTrailController_OnQueueEmpty;
            PetInvokeComponent.OnShouldInvokePetForLevel -= PetInvokeComponent_OnShouldInvokePetForLevel;
            PetMoveComponent.OnMoveStarted -= PetMoveComponent_OnMoveStarted;
            petsChargeController.OnCharged -= PetsChargeController_OnCharged;

            AttemptDestroyTutorial();

            data.chargeInactiveBar.Deinitialize();
            data.chargeActiveBar.Deinitialize();

            foreach (var chargeBar in data.interimChargeBars)
            {
                chargeBar.Deinitialize();
            }

            DOTween.Kill(this);
        }


        public void InitializeButtons()
        {
            data.callPetButtonActive.onClick.AddListener(CallPetButton_OnClick);
        }


        public void DeinitializeButtons()
        {
            data.callPetButtonActive.onClick.RemoveListener(CallPetButton_OnClick);

            data.petMoveJoystick.SetActive(false);
        }


        private void RefreshVisual()
        {
            bool isPetCharged = petsChargeController.IsPetCharged(petSkinType);

            CommonUtility.SetObjectActive(data.callPetButtonActive.gameObject, isCallPetButtonAvailable && isPetCharged);
            CommonUtility.SetObjectActive(data.callPetButtonInactive.gameObject, isCallPetButtonAvailable && !isPetCharged);

            if (effectHandler != null)
            {
                effectHandler.Stop();
            }

            if (!isCallPetButtonAvailable)
            {
                return;
            }

            string fxKey = isPetCharged ? EffectKeys.FxGUIPetBarFullIdleShine : EffectKeys.FxPetSleepUi;

            effectHandler = EffectManager.Instance.CreateSystem(fxKey, true, data.fxRoot.position, data.fxRoot.rotation,
                data.fxRoot);

            if (effectHandler != null)
            {
                effectHandler.transform.localScale = Vector3.one;
            }

            SkeletonDataAsset skeletonDataAsset =
                IngameData.Settings.pets.skinsSettings.GetSkeletonDataAsset(petSkinType);

            foreach (var skeletonGraphic in data.skeletonGraphics)
            {
                skeletonGraphic.skeletonDataAsset = skeletonDataAsset;
                skeletonGraphic.Initialize(true);
            }

            SetPetSkin();
        }


        private void RefreshIdleAnimation()
        {
            string animationName = petsChargeController.IsPetCharged(petSkinType)
                ? petAnimationSettings.idleName
                : petAnimationSettings.sleepIdleAnimationName;

            if (currentAnimationName != animationName)
            {
                foreach (var skeletonGraphic in data.skeletonGraphics)
                {
                    SpineUtility.SafeSetAnimation(skeletonGraphic, animationName, loop: true);
                }

                currentAnimationName = animationName;
            }
        }


        private void SetPetSkin()
        {
            string wakeUpAnimationName = petAnimationSettings.wakeUpAnimationName;

            foreach (var skeletonGraphic in data.skeletonGraphics)
            {
                SpineUtility.SafeSetAnimation(skeletonGraphic, wakeUpAnimationName, loop: false,
                    callback: RefreshIdleAnimation);

                CommonUtility.SetObjectActive(skeletonGraphic.gameObject, !petsInvokeController.WasPetInvoked);
            }
        }


        private void AttemptDestroyTutorial()
        {
            if (tutorialMoveAnimator != null)
            {
                Content.Management.DestroyObject(tutorialMoveAnimator.gameObject);
                tutorialMoveAnimator = null;
            }
        }

        #endregion


        
        #region Events handlers

        public void OnLevelReload() =>
            AttemptDestroyTutorial();
        
        
        private void SoulTrailController_OnTrailMoveFinish()
        {
            float petsChargePoints = petsChargeController.CurrentChargePointsCount +
                                     petsChargeController.RecievedChargePointsOnLevel;

            foreach (var chargeBar in data.interimChargeBars)
            {
                chargeBar.UpdateProgress(petsChargePoints);
            }
        }


        private void CallPetButton_OnClick()
        {
            if (effectHandler != null)
            {
                effectHandler.Stop();
            }

            effectHandler = EffectManager.Instance.CreateSystem(EffectKeys.FxGUIPetBarUsed, false,
                data.fxRoot.position, data.fxRoot.rotation, data.fxRoot);
            if (effectHandler != null)
            {
                effectHandler.transform.localScale = Vector3.one;
            }

            petsInvokeController.InvokePetForLevel(petSkinType);

            SetPetSkin();

            FactorAnimation factorAnimation = IngameData.Settings.pets.levelSettings.petButtonAlphaAnimation;
            factorAnimation.Play(value => data.whiteBackImageCanvasGroup.alpha = value, this);
            
            OnInvokePet?.Invoke();
        }


        private void PetInvokeComponent_OnShouldInvokePetForLevel(Pet pet)
        {
            PetsInputLevelController petsInputLevel =
                GameServices.Instance.LevelControllerService.PetsInputLevelController;
            petsInputLevel.SetupJoystick(data.petMoveJoystick);

            data.petMoveJoystick.SetActive(true);

            Animator tutorialAnimatorPrefab =
                IngameData.Settings.tutorialSettings.FindTutorialPrefabsAnimator(TutorialType.PetMove);
            tutorialMoveAnimator = Content.Management.Create(tutorialAnimatorPrefab, data.tutorialRoot);

            PetMoveComponent.OnMoveStarted += PetMoveComponent_OnMoveStarted;
        }


        private void PetMoveComponent_OnMoveStarted(Pet pet)
        {
            PetMoveComponent.OnMoveStarted -= PetMoveComponent_OnMoveStarted;

            tutorialMoveAnimator.SetTrigger(AnimationKeys.Screen.Hide);
        }


        private void PetsChargeController_OnCharged(PetSkinType petType)
        {
            if (petType == petSkinType)
            {
                RefreshVisual();
            }
        }

        
        private void SoulTrailController_OnQueueEmpty()
        {
            if (LevelsManager.Instance.Level.CurrentState != LevelState.AllTargetsHitted)
            {
                return;
            }

            float petsChargePoints = petsChargeController.CurrentChargePointsCount;

            data.chargeInactiveBar.UpdateProgress(petsChargePoints);
            data.chargeActiveBar.UpdateProgress(petsChargePoints);

            foreach (var chargeBar in data.interimChargeBars)
            {
                chargeBar.UpdateProgress(petsChargePoints);
            }
            
            bool isPetCharged = petsChargeController.IsPetCharged(petSkinType);

            Animator btnAnimator = isPetCharged
                ? data.callPetButtonActive.GetComponent<Animator>()
                : data.callPetButtonInactive.GetComponent<Animator>();
            
            btnAnimator.SetTrigger(AnimationKeys.Common.Bounce);
        }

        #endregion
    }
}