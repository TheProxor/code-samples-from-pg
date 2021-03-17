using System;
using Modules.General.Abstraction;
using Drawmasters.Advertising;
using Drawmasters.Helpers;
using Drawmasters.Mansion;
using Drawmasters.Proposal;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Drawmasters.Effects;
using Drawmasters.ServiceUtil;
using Drawmasters.Utils.UiTimeProvider.Implementation;
using Drawmasters.Utils.UiTimeProvider.Interfaces;


namespace Drawmasters.Ui.Mansion
{
    public class UiMansionHammerPropose : MonoBehaviour
    {
        #region Fields

        public event Action<RewardData> OnShouldReceiveReward;

        private const string ReceiveTextFormat = "+{0}";

        [SerializeField] private GameObject root = default;

        [Header("Hammers for Premium currency")]
        [SerializeField] private TMP_Text currencyButtonPriceText = default;
        [SerializeField] private Button currencyButton = default;
        [SerializeField] private UiCurrencyData forCurrencyData = default;

        [Header("Hammers for Rewarded video")]
        [SerializeField] private RewardedVideoButton adsButton = default;
        [SerializeField] private UiCurrencyData forVideoData = default;

        // TODO write logic
        [Header("Hammers for Rewarded video disabled")]
        [SerializeField] private GameObject disabledRewardedVideoRoot = default;
        [SerializeField] private TMP_Text timerText = default;

        private MansionRewardPackSettings settings;

        private RealtimeTimer refreshTimer;
        
        private readonly ITimeUiTextConverter converter = new FlexibleUiTimerTimeConverter();

        #endregion



        #region Properties

        private float TimerCooldown =>
            GameServices.Instance.AbTestService.CommonData.mansionHammersProposeSecondsCooldown;


        #endregion



        #region Methods

        public void SetEnabled(bool enabled) =>
            CommonUtility.SetObjectActive(root, enabled);


        public void Initialize()
        {
            settings = IngameData.Settings.mansionRewardPackSettings;

            forCurrencyData.text.text = string.Format(ReceiveTextFormat, settings.hammersForCurrencyData.UiRewardText);
            currencyButtonPriceText.text = settings.hammersForCurrencyData.price.ToShortFormat();

            forVideoData.text.text = string.Format(ReceiveTextFormat, settings.hammersForVideoData.UiRewardText);

            adsButton.Initialize(AdsVideoPlaceKeys.MansionHammers);
            adsButton.OnVideoShowEnded += AdsButton_OnVideoShowEnded;
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;

            refreshTimer = new RealtimeTimer(PrefsKeys.Proposal.MansionHammerProposeTimer, GameServices.Instance.TimeValidator);
            refreshTimer.SetTimeOverCallback(RefreshVisual);
            refreshTimer.Initialize();

            RefreshVisual();
        }


        public void Deinitialize()
        {
            adsButton.OnVideoShowEnded -= AdsButton_OnVideoShowEnded;
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
            adsButton.Deinitialize();
            refreshTimer.Deinitialize();
        }


        public void InitializeButtons()
        {
            adsButton.InitializeButtons();
            currencyButton.onClick.AddListener(CurrencyButton_OnClick);
        }


        public void DeinitializeButtons()
        {
            adsButton.DeinitializeButtons();
            currencyButton.onClick.RemoveListener(CurrencyButton_OnClick);
        }


        public bool TryFindReward(RewardData rewardData, out Vector3 pos)
        {
            pos = default;

            if (rewardData == settings.hammersForCurrencyData)
            {
                pos = forCurrencyData.icon.position;
                return true;
            }

            if (rewardData == settings.hammersForVideoData)
            {
                pos = forVideoData.icon.position;
                return true;
            }

            return false;
        }


        private void RefreshVisual()
        {
            bool isVideoProposeAvailable = !refreshTimer.IsTimerActive;

            CommonUtility.SetObjectActive(adsButton.gameObject, isVideoProposeAvailable);
            CommonUtility.SetObjectActive(disabledRewardedVideoRoot, !isVideoProposeAvailable);
        }

        #endregion



        #region Events handler

        private void MonoBehaviourLifecycle_OnUpdate(float delta) =>
            timerText.text = refreshTimer.ConvertTime(converter);


        private void AdsButton_OnVideoShowEnded(AdActionResultType result)
        {
            EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUICottageGetHammersClick, parent: adsButton.transform, transformMode: TransformMode.Local);

            if (result == AdActionResultType.Success)
            {
                refreshTimer.Start(TimerCooldown);
                OnShouldReceiveReward?.Invoke(settings.hammersForVideoData);
                RefreshVisual();
            }
        }


        private void CurrencyButton_OnClick()
        {
            DeinitializeButtons();
            EventSystemController.SetSystemEnabled(false, this);

            IProposable proposal = new CurrencyProposal((settings.hammersForCurrencyData.price, settings.hammersForCurrencyData.priceType));

            proposal.Propose((result) =>
            {
                if (result)
                {
                    OnShouldReceiveReward?.Invoke(settings.hammersForCurrencyData);
                }

                InitializeButtons();
                EventSystemController.SetSystemEnabled(true, this);
            });

            EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUICottageGetHammersClick, parent: currencyButton.transform, transformMode: TransformMode.Local);
        }

        #endregion
    }
}
