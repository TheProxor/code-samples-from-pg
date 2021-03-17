using Drawmasters.Levels;
using Drawmasters.Utils.Ui;
using UnityEngine;
using System;
using Drawmasters.Effects;
using Drawmasters.Proposal;
using Drawmasters.Utils;
using I2.Loc;
using Modules.Sound;
using Spine.Unity;
using UnityEngine.UI;
using TMPro;


namespace Drawmasters.Ui
{
    public class UiLeagueApplyScreen : AnimatorScreen
    {
        #region Fields

        [SerializeField] private Button infoButton = default;

        [SerializeField] private IdleEffect[] idleEffects = default;
        [SerializeField] private Localize headerLocalizeText = default;

        [SerializeField] private UserInputField userInputField = default;
        [SerializeField] private Button openButtonAvailable = default;
        [SerializeField] private Button openButtonDisable = default;

        [SerializeField] private TMP_Text enterNameText = default;

        [Header("Header animations")]
        [SerializeField] private SkeletonGraphic leagueTypeSkeletonGraphic = default;

        private LeagueProposeController controller;
        
        #endregion



        #region Properties

        public override ScreenType ScreenType => 
            ScreenType.LeagueApply;

        #endregion



        #region Methods

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null,
                                        Action<AnimatorView> onHideEndCallback = null,
                                        Action<AnimatorView> onShowBeginCallback = null,
                                        Action<AnimatorView> onHideBeginCallback = null)
        {
            base.Initialize(onShowEndCallback, onHideEndCallback, onShowBeginCallback, onHideBeginCallback);

            userInputField.Initialize();
            userInputField.OnInputSubmited += UserInputField_OnInputSubmited;
            
            
        }


        public override void Deinitialize()
        {
            userInputField.Deinitialize();
            userInputField.OnInputSubmited -= UserInputField_OnInputSubmited;

            foreach (var idleEffect in idleEffects)
            {
                idleEffect.StopEffect();
            }
            
            base.Deinitialize();
        }


        public override void InitializeButtons()
        {
            openButtonAvailable.onClick.AddListener(OpenButton_OnClick);
            openButtonDisable.onClick.AddListener(OpenButtonDisable_OnClick);
            infoButton.onClick.AddListener(InfoButton_OnClick);
        }


        public override void DeinitializeButtons()
        {
            openButtonAvailable.onClick.RemoveListener(OpenButton_OnClick);
            openButtonDisable.onClick.RemoveListener(OpenButtonDisable_OnClick);
            infoButton.onClick.RemoveListener(InfoButton_OnClick);
        }


        public void SetupController(LeagueProposeController _controller)
        {
            controller = _controller;

            userInputField.SetSubmitText(controller.LeaderBoard.PlayerData.NickName);

            LeagueType currentLeagueType = controller.LeaderBoard.LeagueType;

            string key = controller.VisualSettings.FindHeaderKey(currentLeagueType);
            headerLocalizeText.SetTerm(key);

            foreach (var idleEffect in idleEffects)
            {
                idleEffect.CreateAndPlayEffect();
            }

            SpineUtility.SafeSetAnimation(leagueTypeSkeletonGraphic,
                controller.VisualSettings.FindShowWhiteAnimationKey(currentLeagueType), 0, false, () =>
                {
                    SpineUtility.SafeSetAnimation(leagueTypeSkeletonGraphic,
                        controller.VisualSettings.FindIdleWhiteAnimationKey(currentLeagueType), 0, true);
                });

            SoundManager.Instance.PlayOneShot(AudioKeys.Ingame.CUP_APPEAR_TOURNAMENT);
        }

        #endregion



        #region Events handlers

        private void UserInputField_OnInputSubmited(string resultNickName)
        {
            controller.LeaderBoard.PlayerData.NickName = resultNickName;

            bool isEmpty = string.IsNullOrEmpty(controller.LeaderBoard.PlayerData.NickName);
            
            CommonUtility.SetObjectActive(openButtonDisable.gameObject, isEmpty);
            CommonUtility.SetObjectActive(openButtonAvailable.gameObject, !isEmpty);
        }


        private void OpenButton_OnClick()
        {
            DeinitializeButtons();

            FromLevelToLevel.PlayTransition(() =>
            {
                HideImmediately();
                controller.ProposeLeaderBoard();
            });
        }


        private void OpenButtonDisable_OnClick()
        {
            controller.VisualSettings.enterNameTextScaleAnimation.Play((value) => enterNameText.transform.localScale = value, this,
                () => controller.VisualSettings.enterNameTextScaleAnimation.Play((value) => enterNameText.transform.localScale = value, this, isReversed: true));

            controller.VisualSettings.enterNameTextColorAnimation.Play((value) => enterNameText.color = value, this,
                () => controller.VisualSettings.enterNameTextColorAnimation.Play((value) => enterNameText.color = value, this, isReversed: true));
        }


        private void InfoButton_OnClick() =>
            UiScreenManager.Instance.ShowScreen(ScreenType.LeagueInfo, isForceHideIfExist: true);

        #endregion
    }
}
