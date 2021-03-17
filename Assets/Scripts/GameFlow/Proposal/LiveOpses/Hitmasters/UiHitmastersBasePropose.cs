using UnityEngine;
using UnityEngine.UI;
using Drawmasters.Proposal;
using Drawmasters.Levels;
using Drawmasters.Effects;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Ui
{
    public class UiHitmastersBasePropose : UiLiveOpsPropose
    {
        #region Fields
        
        [SerializeField] protected Image activeModeImage = default;
        [SerializeField] protected Image reloadModeImage = default;
        [SerializeField] protected IdleEffect idleEffect = default;

        protected HitmastersProposeController controller;

        #endregion



        #region Properties

        protected virtual float ScaleIconSkinsMultiplier => 1.0f;
        
        public override LiveOpsProposeController LiveOpsProposeController =>
            GameServices.Instance.ProposalService.HitmastersProposeController;

        
        protected override LiveOpsEventController LiveOpsEventController =>
            default;

        
        protected override bool ShouldDestroyTutorialCanvasAfterClick =>
            controller.ShouldShowPreviewScreen;

        
        protected GameMode CurrentGameMode => 
            controller.IsActive ? controller.LiveOpsGameMode : controller.GetNextGameModeToPlay(controller.LiveOpsGameMode);
        
        #endregion



        #region Methods

        public override void Initialize()
        {
            controller = GameServices.Instance.ProposalService.HitmastersProposeController;

            base.Initialize();
        }


        public override void Deinitialize()
        {
            base.Deinitialize();

            idleEffect.StopEffect();
        }

        
        protected override void OnClickOpenButton(bool isForcePropose)
        {
            bool shouldShowPreviewScreen = controller.ShouldShowPreviewScreen;

            if (shouldShowPreviewScreen)
            {
                UiScreenManager.Instance.ShowScreen(ScreenType.HitmastersPreview, onShowed: (view) => SetFadeEnabled(false));
                controller.ShouldShowPreviewScreen = false;
            }
            else
            {
                DeinitializeButtons();

                FromLevelToLevel.PlayTransition(() =>
                {
                    UiScreenManager.Instance.HideScreenImmediately(ScreenType.MainMenu);

                    LevelsManager.Instance.UnloadLevel();

                    UiScreenManager.Instance.ShowScreen(ScreenType.HitmastersMap, isForceHideIfExist: true);
                });
            }
        }

        
        protected override void OnShouldRefreshVisual()
        {
            base.OnShouldRefreshVisual();
            
            Sprite sprite = controller.VisualSettings.FindGameModeSmallSprite(CurrentGameMode);

            Image[] modeImages = { activeModeImage, reloadModeImage };

            foreach (var modeImage in modeImages)
            {
                modeImage.sprite = sprite;
                modeImage.transform.localScale = Vector3.one * ScaleIconSkinsMultiplier;
                modeImage.SetNativeSize();
            }
        }

        
        protected override void OnPreForcePropose()
        {
            base.OnPreForcePropose();

            Canvas modeImageCanvas = activeModeImage.GetComponent<Canvas>();
            if (modeImageCanvas != null)
            {
                modeImageCanvas.sortingOrder = ProposeSortingOrder + 1;
            }
        }


        protected override void OnPostForcePropose()
        {
            Canvas modeImageCanvas = activeModeImage.GetComponent<Canvas>();
            if (modeImageCanvas != null)
            {
                modeImageCanvas.sortingOrder = NormalSortingOrder + 1;
            }

            base.OnPostForcePropose();
        }

        #endregion
    }
}
