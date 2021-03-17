using DG.Tweening;
using System;
using Drawmasters.Proposal;
using UnityEngine;
using UnityEngine.UI;
using Drawmasters.Utils;
using Drawmasters.ServiceUtil;
using TMPro;


namespace Drawmasters.Ui
{
    [Serializable]
    public class UiHitmastersMapPropose : IInitializable, IDeinitializable
    {
        #region Fields
        
        private const float ScaleIconSkinsMultiplier = 0.85f;

        [SerializeField] private RectTransform rootRectTransform = default;
        [SerializeField] private RectTransform rootRectTransformActive = default;
        [SerializeField] private RectTransform rootRectTransformReload = default;
        [SerializeField] private TMP_Text timerText = default;
        [SerializeField] private Image modeImage = default;
        [SerializeField] private TMP_Text rewardText = default;

        private HitmastersProposeController controller;
        private LoopedInvokeTimer timeLeftRefreshTimer;

        #endregion



        #region Properties

        private bool AllowWorkWithLiveOps =>
            controller.IsMechanicAvailable && controller.IsEnoughLevelsFinished;

        #endregion


        
        #region Methods

        public void Initialize()
        {
            controller = GameServices.Instance.ProposalService.HitmastersProposeController;

            timeLeftRefreshTimer = timeLeftRefreshTimer ?? new LoopedInvokeTimer(RefreshTimeLeft);
            timeLeftRefreshTimer.Start();

            controller.OnStarted += Controller_OnLiveOpsStarted;
            controller.OnFinished += RefreshRootGameObject;

            RefreshTimeLeft();
            RefreshVisual();
            RefreshRootGameObject();
        }


        public void Deinitialize()
        {
            DOTween.Kill(this);
            
            controller.OnStarted -= Controller_OnLiveOpsStarted;
            controller.OnFinished -= RefreshRootGameObject;

            timeLeftRefreshTimer.Stop();
        }


        private void RefreshVisual()
        {
            rewardText.text = string.Empty;
            
            switch (controller.GeneratedLiveOpsReward)
            {
                case CurrencyReward currency:
                    rewardText.text = currency.UiRewardText;
                    modeImage.sprite = controller.VisualSettings.FindCurrencyIcon(currency.currencyType);
                    break;

                case ShooterSkinReward shooterSkinReward:
                    modeImage.sprite = controller.VisualSettings.FindGameModeRewardSprite(controller.LiveOpsGameMode);
                    break;

                case WeaponSkinReward weaponSkinReward:
                    modeImage.sprite = controller.VisualSettings.FindGameModeRewardSprite(controller.LiveOpsGameMode);;
                    break;

                default:
                    CustomDebug.Log($"Not implemented logic for {controller.GeneratedLiveOpsReward.Type}");
                    break;
            }

            bool isCurrencyReward = controller.GeneratedLiveOpsReward is CurrencyReward;
            modeImage.transform.localScale = isCurrencyReward ? Vector3.one : Vector3.one * ScaleIconSkinsMultiplier;
            modeImage.SetNativeSize();
        }


        private void RefreshRootGameObject()
        {
            CommonUtility.SetObjectActive(rootRectTransform.gameObject, AllowWorkWithLiveOps);
            CommonUtility.SetObjectActive(rootRectTransformActive.gameObject, controller.IsActive);
            CommonUtility.SetObjectActive(rootRectTransformReload.gameObject, !controller.IsActive);
        }


        private void RefreshTimeLeft()
        {
            if (!AllowWorkWithLiveOps)
            {
                return;
            }

            timerText.text = controller.TimeUi;
        }

        #endregion


        
        #region Events handlers

        private void Controller_OnLiveOpsStarted()
        {
            RefreshTimeLeft();
            RefreshRootGameObject();
            RefreshVisual();
        }

        #endregion
    }
}