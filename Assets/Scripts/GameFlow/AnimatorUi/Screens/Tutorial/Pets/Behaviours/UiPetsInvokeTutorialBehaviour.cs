using System;
using Drawmasters.ServiceUtil;
using UnityEngine;
using static Drawmasters.Ui.UiPetsChargeTutorialScreen;


namespace Drawmasters.Ui
{
    public class UiPetsInvokeTutorialBehaviour : IUiBehaviour
    {
        #region Nested types

        [Serializable]
        public class Data : BehaviourData
        {
        }

        #endregion



        #region Fields

        private readonly Data data;
        private readonly UiPetsChargeTutorialScreen screen;

        private UiOverlayTutorialHelper uiTutorial;
        private IUiOverlayTutorialObject uiOverlayObject;

        private Animator petVisual;

        #endregion



        #region Class lifecycle

        public UiPetsInvokeTutorialBehaviour(UiPetsChargeTutorialScreen _screen, Data _data)
        {
            data = _data;
            screen = _screen;
        }

        #endregion



        #region IUiBehaviour

        public void Enable()
        {
            CommonUtility.SetObjectActive(data.root, true);

            IngameScreen ingameScreen = UiScreenManager.Instance.LoadedScreen<IngameScreen>(ScreenType.Ingame);
            if (ingameScreen == null)
            {
                CustomDebug.Log($"{nameof(IngameScreen)} is not loaded. Completing tutorial");
                screen.CompleteTutorial();
            }

            uiTutorial = uiTutorial ?? new UiOverlayTutorialHelper(data.fadeImage, data.fadeAnimation, true);
            uiTutorial.Initialize();

            uiOverlayObject = uiOverlayObject ?? new UiOverlayTutorialCommonObject(ingameScreen.CallPetButtonMainRoot);
            uiTutorial.SetupOverlayedObjects(uiOverlayObject);
            uiTutorial.SetupSortingOrder(screen.SortingOrder + 1);
            uiTutorial.PlayTutorial();

            petVisual = Content.Management.Create(data.petVisualPrefab, uiOverlayObject.OverlayTutorialObject.transform);
            petVisual.transform.position = uiOverlayObject.OverlayTutorialObject.transform.position;

            petVisual.SetTrigger(AnimationKeys.Screen.Show);
        }


        public void Disable()
        {
            uiTutorial?.StopTutorial(default);
            uiTutorial?.Deinitialize();

            uiOverlayObject = null;
        }


        public void Deinitialize()
        {
            Disable();

            if (petVisual != null)
            {
                Content.Management.DestroyObject(petVisual.gameObject);
                petVisual = null;
            }

            uiTutorial = null;
        }


        public void InitializeButtons()
        {
            GameServices.Instance.PetsService.InvokeController.OnInvokePetForLevel += InvokeController_OnInvokePetForLevel;
        }


        public void DeinitializeButtons()
        {
            GameServices.Instance.PetsService.InvokeController.OnInvokePetForLevel -= InvokeController_OnInvokePetForLevel;
        }

        #endregion



        #region Events handlers

        private void InvokeController_OnInvokePetForLevel(PetSkinType petSkinType)
        {
            petVisual.SetTrigger(AnimationKeys.Screen.Hide);
            uiTutorial?.SetInputEnabled(false);
            screen.CompleteTutorial();
        }

        #endregion
    }
}
