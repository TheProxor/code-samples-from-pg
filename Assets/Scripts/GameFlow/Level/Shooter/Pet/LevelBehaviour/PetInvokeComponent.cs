using System;
using Drawmasters.Levels;
using Drawmasters.ServiceUtil;
using Drawmasters.Ui;
using UnityEngine;
using DG.Tweening;
using Drawmasters.Utils;

namespace Drawmasters.Pets
{
    public class PetInvokeComponent : PetComponent
    {
        #region Fields

        public static event Action<Pet> OnShouldInvokePetForLevel;
        public static event Action<Pet> OnPreInvokePetForLevel;

        private IPetsService petsService;
        private PetLevelSettings petLevelSettings;
        private PetsInputLevelController inputLevelController;

        #endregion



        #region Methods

        public override void Initialize(Pet _pet)
        {
            base.Initialize(_pet);

            petsService = GameServices.Instance.PetsService;
            petLevelSettings = IngameData.Settings.pets.levelSettings;
            inputLevelController = GameServices.Instance.LevelControllerService.PetsInputLevelController;

            petsService.InvokeController.OnInvokePetForLevel += InvokeController_OnInvokePetForLevel;
        }


        public override void Deinitialize()
        {
            DOTween.Kill(this);

            inputLevelController.Remove(mainPet);

            petsService.InvokeController.OnInvokePetForLevel -= InvokeController_OnInvokePetForLevel;

            base.Deinitialize();
        }


        #endregion



        #region Events handlers

        private void InvokeController_OnInvokePetForLevel(PetSkinType petSkinType)
        {
            if (mainPet.CurrentSkinLink != null &&
                mainPet.CurrentSkinLink.SkinType == petSkinType)
            {
                petsService.InvokeController.OnInvokePetForLevel -= InvokeController_OnInvokePetForLevel;

                inputLevelController.Add(mainPet);

                bool isEditor = GameServices.Instance.LevelEnvironment.Context.IsEditor;
                if (isEditor)
                {
                    OnPreInvokePetForLevel?.Invoke(mainPet);
                    OnShouldInvokePetForLevel?.Invoke(mainPet);
                }
                else
                {
                    IngameScreen ingameScreen = UiScreenManager.Instance.LoadedScreen<IngameScreen>(ScreenType.Ingame);
                    if (ingameScreen == null)
                    {
                        CustomDebug.Log($"No {nameof(IngameScreen)} was found. Starting appear pet from default position");
                    }

                    Camera gameCamera = IngameCamera.Instance.Camera;
                    Vector3 startPosition = ingameScreen != null ? ingameScreen.PetStartPosition : default;

                    petLevelSettings.invokeAnimation.beginValue = startPosition.UiToWorldPosition().SetZ(0.0f);

                    petLevelSettings.invokeAnimation.endValue = Vector3.zero;

                    petLevelSettings.invokeAnimation.Play((value) => mainPet.CurrentSkinLink.CurrentPosition = value, this, () =>
                    {
                        mainPet.CurrentSkinLink.MainCollider2D.enabled = true;

                        OnShouldInvokePetForLevel?.Invoke(mainPet);
                    });

                    mainPet.CurrentSkinLink.MainCollider2D.enabled = false;
                    OnPreInvokePetForLevel?.Invoke(mainPet);
                }
            }
        }

        #endregion
    }
}
