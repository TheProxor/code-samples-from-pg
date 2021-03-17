using System.Linq;
using Drawmasters.Levels;
using Drawmasters.Utils;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public abstract class UiLiveOpsPreviewScreen : AnimatorScreen
    {
        #region Fields

        [SerializeField] private Button[] closeButtons = default;
        [SerializeField] private Button openButton = default;
        [SerializeField] private TMP_Text timerText = default;

        private LiveOpsProposeController controller;
        private LoopedInvokeTimer timeLeftRefreshTimer;

        #endregion



        #region Properties

        private bool AllowWorkWithLiveOps =>
            controller.IsMechanicAvailable && controller.IsEnoughLevelsFinished;

        #endregion



        #region Methods

        public override void Show()
        {
            controller = GetController();

            base.Show();

            controller.OnStarted += Controller_OnLiveOpsStarted;
            controller.OnFinished += Hide;

            timeLeftRefreshTimer = timeLeftRefreshTimer ?? new LoopedInvokeTimer(RefreshTimeLeft);
            timeLeftRefreshTimer.Start();

            RefreshVisual();
            RefreshTimeLeft();

            bool isFirstLiveOps = controller.ShowsCount == 1;
            GameObject[] closeButtonsGO = closeButtons.Select(e => e.gameObject).ToArray();
            CommonUtility.SetObjectsActive(closeButtonsGO, !isFirstLiveOps);
        }


        public override void Hide()
        {
            FromLevelToLevel.PlayTransition(() =>
            {
                UiScreenManager.Instance.HideScreenImmediately(ScreenType);

                LevelsManager.Instance.UnloadLevel();
                LevelsManager.Instance.LoadScene(GameServices.Instance.PlayerStatisticService.PlayerData.LastPlayedMode, GameMode.MenuScene);
                UiScreenManager.Instance.ShowScreen(ScreenType.MainMenu);
            });
        }


        public override void Deinitialize()
        {
            controller.OnStarted -= Controller_OnLiveOpsStarted;
            controller.OnFinished -= Hide;

            timeLeftRefreshTimer.Stop();

            base.Deinitialize();
        }


        public override void DeinitializeButtons()
        {
            foreach (var closeButton in closeButtons)
            {
                closeButton.onClick.RemoveListener(Hide);
            }

            openButton.onClick.RemoveListener(OpenButton_OnClick);
        }

        public override void InitializeButtons()
        {
            foreach (var closeButton in closeButtons)
            {
                closeButton.onClick.AddListener(Hide);
            }

            openButton.onClick.AddListener(OpenButton_OnClick);
        }


        protected virtual void RefreshVisual()
        {
        }


        private void RefreshTimeLeft()
        {
            if (!AllowWorkWithLiveOps)
            {
                return;
            }

            if (timerText != null)
            {
                timerText.text = controller.TimeUi;
            }
        }


        protected abstract LiveOpsProposeController GetController();


        protected abstract void OnShouldShowLiveOpsScreen();

        #endregion



        #region Events handlers

        private void OpenButton_OnClick()
        {
            if (controller.IsEnoughLevelsFinished)
            {
                if (controller.IsActive)
                {
                    DeinitializeButtons();

                    FromLevelToLevel.PlayTransition(() =>
                    {
                        UiScreenManager.Instance.HideScreenImmediately(ScreenType);
                        UiScreenManager.Instance.HideScreenImmediately(ScreenType.MainMenu);
                        LevelsManager.Instance.UnloadLevel();
                       
                        OnShouldShowLiveOpsScreen();
                    });
                }
            }
            else
            {
                CustomDebug.LogError($"Live ops should not be available. Not enough levels finished. Ui proposal must be hide");
            }
        }


        private void Controller_OnLiveOpsStarted()
        {
            RefreshTimeLeft();
            RefreshVisual();
        }

        #endregion
    }
}