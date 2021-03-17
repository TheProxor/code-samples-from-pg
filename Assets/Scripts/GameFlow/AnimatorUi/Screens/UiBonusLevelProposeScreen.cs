using System;
using Modules.General.Abstraction;
using Drawmasters.Advertising;
using Drawmasters.Proposal;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public class UiBonusLevelProposeScreen : AnimatorScreen
    {
        #region Fields

        [SerializeField] private RewardedVideoButton goAdsButton = default;
        [SerializeField] private Button goFreeButton = default;

        [SerializeField] private Button closeButton = default;

        #endregion 



        #region Properties

        public override ScreenType ScreenType => ScreenType.BonusLevelPropose;

        public bool WasProposeAccepted { get; private set; }

        #endregion



        #region Overrided methods

        public override void Show()
        {
            base.Show();

            bool isForAds = false;
            CommonUtility.SetObjectActive(goAdsButton.gameObject, isForAds);
            CommonUtility.SetObjectActive(goFreeButton.gameObject, !isForAds);
            CommonUtility.SetObjectActive(closeButton.gameObject, isForAds);

            WasProposeAccepted = false;
        }

        #endregion



        #region Methods

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null,
            Action<AnimatorView> onHideEndCallback = null,
            Action<AnimatorView> onShowBeginCallback = null,
            Action<AnimatorView> onHideBeginCallback = null)
        {
            base.Initialize(onShowEndCallback, onHideEndCallback, onShowBeginCallback, onHideBeginCallback);

            goAdsButton.Initialize(AdsVideoPlaceKeys.BonusLevelPropose);

            goAdsButton.OnVideoShowEnded += OnClickGoAdsButton;
        }


        public override void Deinitialize()
        {
            goAdsButton.Deinitialize();
            
            goAdsButton.OnVideoShowEnded -= OnClickGoAdsButton;

            base.Deinitialize();
        }


        public override void InitializeButtons()
        {
            goAdsButton.InitializeButtons();
            goFreeButton.onClick.AddListener(OnAcceptPropose);

            closeButton.onClick.AddListener(Hide);
        }


        public override void DeinitializeButtons()
        {
            goAdsButton.DeinitializeButtons();
            goFreeButton.onClick.RemoveListener(OnAcceptPropose);

            closeButton.onClick.RemoveListener(Hide);
        }

        #endregion



        #region Events handlers

        private void OnClickGoAdsButton(AdActionResultType result)
        {
            if (result == AdActionResultType.Success)
            {
                OnAcceptPropose();
            }
        }


        private void OnAcceptPropose()
        {
            WasProposeAccepted = true;

            Hide();
        }

        #endregion
    }
}
