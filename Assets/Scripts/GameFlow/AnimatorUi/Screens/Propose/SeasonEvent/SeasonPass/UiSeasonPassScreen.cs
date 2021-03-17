using TMPro;
using System;
using System.Linq;
using Drawmasters.Analytic;
using UnityEngine;
using UnityEngine.UI;
using Drawmasters.Helpers;
using Drawmasters.Levels;
using Drawmasters.OffersSystem;
using Drawmasters.Proposal;
using Drawmasters.Utils;
using Drawmasters.ServiceUtil;
using Drawmasters.Statistics.Data;
using Modules.General.InAppPurchase;
using Modules.InAppPurchase;
using Spine.Unity;
using I2.Loc;


namespace Drawmasters.Ui
{
    public class UiSeasonPassScreen : RewardReceiveScreen
    {
        #region Fields

        [SerializeField] private UiCurrencyData[] uiCurrencyData = default;
        [SerializeField] private Localize noAdsText = default;
        [SerializeField] private TMP_Text priceText = default;
        [SerializeField] private TMP_Text offerOldPriceText = default;
        [SerializeField] private TMP_Text offerNewPriceText = default;
        [SerializeField] private TMP_Text[] timerTexts = default;
        [SerializeField] private SkeletonGraphic timerGraphic = default;

        [SerializeField] private Button closeButton = default;
        
        [SerializeField] private Button purchaseButton = default;
        [SerializeField] private Button offerPurchaseButton = default;

        [Header("Concrete Reward data")]
        [SerializeField] private SkeletonGraphic petGraphic = default;

        [SerializeField] private GameObject[] petRewardRoots = default;
        [SerializeField] private GameObject[] commonRewardRoots = default;

        [Header("Texts")]
        [SerializeField] private TMP_Text headerDescription = default;

        [Header("Visual Data")]
        [SerializeField] private GameObject offerTimerRoot = default;
        [SerializeField] private RectTransformExpand priceLineExpand = default;
        [SerializeField] private UiSubscriptionScreenSelectorObjects[] selectorObjects = default;
        [SerializeField] private UiSubscriptionScreenSelectorOutline[] selectorOutlines = default;
        [SerializeField] private UiSubscriptionScreenSelectorColor[] selectorColors = default;

        private StoreItem seasonPassItem;
        private StoreItem seasonPassOfferItem;
        private CurrencyReward[] currencyRewards;

        private SeasonEventProposeController controller;
        private LoopedInvokeTimer timeLeftRefreshTimer;
        private GoldenTicketOffer offer;

        #endregion



        #region Properties

        public override ScreenType ScreenType =>
             ScreenType.SeasonPassScreen;

        #endregion


        #region Methods

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null,
                                        Action<AnimatorView> onHideEndCallback = null,
                                        Action<AnimatorView> onShowBeginCallback = null,
                                        Action<AnimatorView> onHideBeginCallback = null)
        {
            base.Initialize(onShowEndCallback, onHideEndCallback, onShowBeginCallback, onHideBeginCallback);

            currencyRewards = IngameData.Settings.seasonEvent.seasonEventSettings.passRewardData;
            controller = GameServices.Instance.ProposalService.SeasonEventProposeController;
            offer = GameServices.Instance.ProposalService.GetOffer<GoldenTicketOffer>();

            seasonPassItem = IAPs.GetStoreItem(IAPs.Keys.Consumable.SeasonPass);
            seasonPassOfferItem = IAPs.GetStoreItem(offer.InAppId);
            
            if (timerGraphic != null)
            {
                timerGraphic.Initialize(false);
            }

            if (controller.IsPetMainReward)
            {
                PetUiSettings petUiSettings = IngameData.Settings.pets.uiSettings;
                petGraphic.skeletonDataAsset = petUiSettings.FindMainMenuSkeletonData(controller.PetMainRewardType);
                petGraphic.Initialize(true);
            }

            CommonUtility.SetObjectsActive(petRewardRoots, controller.IsPetMainReward);
            CommonUtility.SetObjectsActive(commonRewardRoots, !controller.IsPetMainReward);

            timeLeftRefreshTimer = timeLeftRefreshTimer ?? new LoopedInvokeTimer(RefreshTimeLeft);
            timeLeftRefreshTimer.Start();
            
            RefreshTimeLeft();

            foreach (var data in uiCurrencyData)
            {
                CurrencyReward currencyReward = Array.Find(currencyRewards, e => e.currencyType == data.currencyType);

                if (currencyReward != null)
                {
                    if (data.text.TryGetComponent(out Localize localize))
                    {
                        localize.SetStringParams(currencyReward.value.ToShortFormat());
                    }
                    else
                    {
                        string text = string.Concat(currencyReward.value.ToShortFormat(), " ", currencyReward.currencyType.UiCurrencyName().ToUpper(), "!");
                        data.text.text = text;
                    }
                }
            }


            RefreshDescription();

            RefreshPrice();

            controller.OnFinished += Controller_OnLiveOpsFinished;

            priceLineExpand.Initialize();
            SetupVisual(offer.IsActive);
        }


        public override void Deinitialize()
        {
            priceLineExpand.Deinitialize();
            controller.OnFinished -= Controller_OnLiveOpsFinished;

            StoreManager.Instance.ItemDataReceived -= Instance_ItemDataReceived;
            timeLeftRefreshTimer.Stop();

            base.Deinitialize();
        }


        public override void InitializeButtons()
        {
            closeButton.onClick.AddListener(CloseButton_OnClick);
            purchaseButton.onClick.AddListener(PurchaseButton_OnClick);
            offerPurchaseButton.onClick.AddListener(OfferPurchaseButton_OnClick);
        }


        public override void DeinitializeButtons()
        {
            closeButton.onClick.RemoveListener(CloseButton_OnClick);
            purchaseButton.onClick.RemoveListener(PurchaseButton_OnClick);
            offerPurchaseButton.onClick.RemoveListener(OfferPurchaseButton_OnClick);
        }


        public override Vector3 GetCurrencyStartPosition(RewardData rewardData)
        {
            Vector3 result = default;

            if (rewardData is CurrencyReward currencyReward)
            {
                UiCurrencyData data = Array.Find(uiCurrencyData, e => e.currencyType == currencyReward.currencyType);
                if (data != null)
                {
                    result = (data.icon as RectTransform).anchoredPosition3D;
                }
            }

            return result;
        }


        // This methods called as even in Unity Animator
        private void StopEffects()
        {
        }

        
        private void SetupVisual(bool isOfferActive)
        {
            UiOfferType offerType = isOfferActive ? UiOfferType.Offer : UiOfferType.Common;

            foreach (var selector in selectorObjects)
            {
                selector.Select(offerType);
            }

            foreach (var selector in selectorOutlines)
            {
                selector.Select(offerType);
            }

            foreach (var selector in selectorColors)
            {
                selector.Select(offerType);
            }
        }

        
        private void RefreshPrice()
        {
            if (seasonPassItem == null)
            {
                CustomDebug.Log("Season pass item is NULL");
                return;
            }

            if (seasonPassOfferItem == null)
            {
                CustomDebug.Log("Season pass offer item is NULL");
                return;
            }

            if (!seasonPassItem.IsPriceActual)
            {
                StoreManager.Instance.RequestItemsData(seasonPassItem);
                StoreManager.Instance.ItemDataReceived += Instance_ItemDataReceived;
            }
            
            if (!seasonPassOfferItem.IsPriceActual)
            {
                StoreManager.Instance.RequestItemsData(seasonPassOfferItem);
                StoreManager.Instance.ItemDataReceived += Instance_ItemOfferDataReceived;
            }
            
            priceText.text = seasonPassItem.IsPriceActual ?
                seasonPassItem.LocalizedPrice : $"{seasonPassItem.TierPrice}$";
            
            offerOldPriceText.text = priceText.text;
            
            offerNewPriceText.text = seasonPassOfferItem.IsPriceActual ?
                seasonPassOfferItem.LocalizedPrice : $"{seasonPassOfferItem.TierPrice}$";
        }


        private void RefreshTimeLeft()
        {
            foreach (var timerText in timerTexts)
            {
                string timeLeftText = offer.IsActive ? offer.TimeUi : controller.TimeUi;
                timerText.text = timeLeftText;
            }

            if (noAdsText != null)
            {
                string timeLeftText = controller.LiveOpsHoursTimeUi;

                noAdsText.SetStringParams(timeLeftText);
            }

            CommonUtility.SetObjectActive(offerTimerRoot, offer.IsActive);
        }

        
        private void Purchase(StoreItem storeItem)
        {
            InAppProposal iAPProposal = new InAppProposal(storeItem);
            iAPProposal.Propose(result =>
            {
                if (result)
                {
                    if (offer.IsActive)
                    {
                        AnalyticHelper.SendOfferPurchase(offer.OfferType, offer.EntryPoint, storeItem.TierPrice, -1);
                    }
                    
                    controller.MarkSeasonPassPurchased();

                    AnimatorScreen screen = UiScreenManager.Instance.ShowScreen(ScreenType.CongratulationScreen);
                    UiCongratulationScreen congratulationScreen = screen as UiCongratulationScreen;
                    if (congratulationScreen != null)
                    {
                        RewardData[] rewards = currencyRewards.OfType<RewardData>().ToArray();
                        
                        congratulationScreen.Initialize(rewards, storeItem.Identifier, () =>
                        {
                            for (int i = 0; i < currencyRewards.Length; i++)
                            {
                                Action callback = i == currencyRewards.Length - 1 ? (Action)Hide : null;
                                
                                OnShouldApplyReward(currencyRewards[i], callback, true);
                            }
                        });
                    }
                }
            });
        }
        

        private void RefreshDescription()
        {
            string headerDescriptionKey = controller.IsGoldenTicketLock ?
                controller.VisualSettings.seasonPassIapHeaderKey : controller.VisualSettings.seasonPassAdsHeaderKey;

            headerDescription.text = LocalizationManager.GetTermTranslation(headerDescriptionKey);

            headerDescription.text = headerDescription.text.Replace("\\n", "\n");
        }

        #endregion



        #region Events handlers

        private void Controller_OnLiveOpsFinished()
        {
            DeinitializeButtons();

            FromLevelToLevel.PlayTransition(() =>
            {
                UiScreenManager.Instance.HideAll(true);

                LevelsManager.Instance.UnloadLevel();
                LevelsManager.Instance.LoadScene(GameServices.Instance.PlayerStatisticService.PlayerData.LastPlayedMode, GameMode.MenuScene);
                UiScreenManager.Instance.ShowScreen(ScreenType.MainMenu, isForceHideIfExist: true);
            });
        }

        
        private void Instance_ItemDataReceived(StoreItem item)
        {
            if (item == seasonPassItem)
            {
                StoreManager.Instance.ItemDataReceived -= Instance_ItemDataReceived;
                RefreshPrice();
            }
        }


        private void Instance_ItemOfferDataReceived(StoreItem item)
        {
            if (item == seasonPassOfferItem)
            {
                StoreManager.Instance.ItemDataReceived -= Instance_ItemOfferDataReceived;
                RefreshPrice();
            }
        }


        private void CloseButton_OnClick() =>
            Hide();


        private void PurchaseButton_OnClick() => 
            Purchase(seasonPassItem);

        
        private void OfferPurchaseButton_OnClick() => 
            Purchase(seasonPassOfferItem);

        #endregion
    }
}
