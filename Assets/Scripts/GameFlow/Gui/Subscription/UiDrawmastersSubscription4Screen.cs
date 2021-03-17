using Modules.UiKit;
using System;
using Drawmasters.Analytic;
using Spine.Unity;
using UnityEngine;
using Drawmasters.Utils;
using Drawmasters.Levels;
using Drawmasters.Helpers;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Modules.General;
using TMPro;
using UnityEngine.UI;
using Modules.General.InAppPurchase;
using Drawmasters.OffersSystem;
using static Modules.General.InAppPurchase.LLStoreSettings;
using I2.Loc;


namespace Drawmasters.Ui
{
    public class UiDrawmastersSubscription4Screen : UiPopupSubscription4Screen
    {
        #region Nested types

        [Serializable]
        private class AnimationData
        {
            public SkeletonGraphic skeletonGraphic = default;

            [SpineAnimation(dataField = "skeletonGraphic")]
            public string[] animationsSequence = default;
        }


        [Serializable]
        private class FreezeObjectsData
        {
            public AnimationEffectPlayer animationEffectPlayer = default;
            public AnimationEffectPlayer destroyAnimationEffectPlayer = default;
            public Image freezeImage = default;
        }

        #endregion



        #region Fields

        private const string ForceShowStep4 = "ForceShowStep4";
        private const int AnimationsIndex = 0;
        private const int OfferSubscriptionType = 4;


        [Header("Common Data")]
        [SerializeField] private Localize rewardProfitText = default;
        [SerializeField] private Localize rewardPremiumCurrencyProfitText = default;

        [SerializeField] private AnimationData wardrobeAnimation = default;

        [SerializeField] private AnimationEffectPlayer wardrobeAnimationEffectPlayers = default;

        [SerializeField] private SkeletonGraphic bonusLevelSkeletonGraphic = default;

        [SerializeField] private FreezeObjectsData[] freezeObjectsData = default;

        [Header("Offer Data")]
        [SerializeField] private GameObject offerTimerRoot = default;
        [SerializeField] private TMP_Text offerTimerLeftText = default;

        [Header("Visual Data")]
        // TODO: refactoring. Change on selector Objects and use via common list.
        [SerializeField] private GameObject[] commonSubscriptionRoots = default;
        [SerializeField] private GameObject[] offerSubscriptionRoots = default;
        [SerializeField] private UiSubscriptionScreenSelectorOutline[] selectorOutlines = default;
        [SerializeField] private UiSubscriptionScreenSelectorColor[] selectorColors = default;

        private BonusLevelSettings settings;
        private LoopedInvokeTimer offerTimeLeftTimer;
        private SubscriptionOffer subscriptionOffer;

        #endregion



        #region Methods

        public void ForceMoveToLastPage() =>
            animator.SetTrigger(ForceShowStep4);


        protected override void OnShow()
        {
            subscriptionOffer = subscriptionOffer?? GameServices.Instance.ProposalService.GetOffer<SubscriptionOffer>();

            base.OnShow();

            settings = IngameData.Settings.bonusLevelSettings;

            offerTimeLeftTimer = offerTimeLeftTimer ?? new LoopedInvokeTimer(RefreshOfferTimeLeft);
            offerTimeLeftTimer.Start();
            RefreshOfferTimeLeft();

            SetupVisual(subscriptionOffer.IsActive);

            RefreshLocalizationText();

            void SetupVisual(bool isOfferActive)
            {
                CommonUtility.SetObjectsActive(commonSubscriptionRoots, !isOfferActive);
                CommonUtility.SetObjectsActive(offerSubscriptionRoots, isOfferActive);

                UiOfferType offerType = isOfferActive ? UiOfferType.Offer : UiOfferType.Common;

                foreach (var selector in selectorOutlines)
                {
                    selector.Select(offerType);
                }

                foreach (var selector in selectorColors)
                {
                    selector.Select(offerType);
                }
            }
        }


        protected override void OnHide()
        {
            foreach (var data in freezeObjectsData)
            {
                data.animationEffectPlayer.OnEventHappend -= CreateFreezeSprites;

                data.animationEffectPlayer.Deinitialize();
                data.destroyAnimationEffectPlayer.Deinitialize();
            }

            wardrobeAnimationEffectPlayers.Deinitialize();

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            offerTimeLeftTimer.Stop();

            base.OnHide();
        }


        protected override void Screen1Button_OnClick()
        {
            base.Screen1Button_OnClick();

            for (int i = 0; i < wardrobeAnimation.animationsSequence.Length; i++)
            {
                string animationName = wardrobeAnimation.animationsSequence[i];
                bool shouldLoop = i == wardrobeAnimation.animationsSequence.Length - 1;
                bool isFirst = i == 0;

                if (isFirst)
                {
                    wardrobeAnimation.skeletonGraphic.Initialize(true);
                    wardrobeAnimation.skeletonGraphic.AnimationState.SetAnimation(AnimationsIndex, animationName, shouldLoop);
                }
                else
                {
                    float prevAnimationDelay = wardrobeAnimation.skeletonGraphic.SkeletonData.FindAnimation(wardrobeAnimation.animationsSequence[i - 1]).Duration;
                    wardrobeAnimation.skeletonGraphic.AnimationState.AddAnimation(AnimationsIndex, animationName, shouldLoop, prevAnimationDelay);

                    bool isLast = i == wardrobeAnimation.animationsSequence.Length - 1;
                    if (isLast)
                    {
                        Scheduler.Instance.CallMethodWithDelay(this, () => wardrobeAnimationEffectPlayers.Initialize(), prevAnimationDelay);
                    }
                }
            }
        }


        protected override void Screen2Button_OnClick()
        {
            base.Screen2Button_OnClick();

            bonusLevelSkeletonGraphic.Initialize(true);

            foreach (var data in freezeObjectsData)
            {
                data.animationEffectPlayer.Initialize();
                data.animationEffectPlayer.OnEventHappend += CreateFreezeSprites;

                data.destroyAnimationEffectPlayer.Initialize();
                data.destroyAnimationEffectPlayer.OnEventHappend += DestroyFreezeSprites;

                DestroyFreezeSprites(data.destroyAnimationEffectPlayer);
            }
        }


        protected override void SubscriptionButton_OnClick(StoreItem storeItem)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                ShowOverlayPopup(UiPopupType.NoInternet);
                return;
            }

            if (storeItem != null)
            {
                PurchaseProcess(true);

                storeItem.Purchase(result =>
                {
                    if (result.ResultCode == PurchaseItemResultCode.Ok)
                    {
                        StoreItem offerStoreItem = GetSubscriptionStoreItem((SubscriptionType)OfferSubscriptionType);
                        if (offerStoreItem.ProductId == storeItem.ProductId)
                        {
                            AnalyticHelper.SendOfferPurchase(subscriptionOffer.OfferType, subscriptionOffer.EntryPoint, storeItem.TierPrice, -1);
                        }
                        
                        Hide();
                            
                        closeCallback.Invoke(SubscriptionPopupResult.SubscriptionPurchased);
                    }

                    PurchaseProcess(false);
                });
            }
        }


        protected override void UpdateButtonState(SubscriptionButtonSettings buttonSettings)
        {
            if (buttonSettings != null)
            {
                string price = string.IsNullOrEmpty(buttonSettings.storeItem.LocalizedPrice) ?
                        $"${buttonSettings.storeItem.TierPrice:F2}" :
                        buttonSettings.storeItem.LocalizedPrice;

                // Really need to check priceLabel for null? to Dmitry.S
                if (buttonSettings.priceLabel != null)
                {
                    buttonSettings.priceLabel.text = price;
                }

                // Really need to check  trialPriceLabel for null? to Dmitry.S
                if (buttonSettings.trialPriceLabel != null &&
                    buttonSettings.trialPriceLabel.TryGetComponent(out Localize trialPriceLoc))
                {
                    trialPriceLoc.SetStringParams(price);
                }

                TryUpdateDescriptionState(buttonSettings);
            }
        }


        protected override void TryUpdateDescriptionState(SubscriptionButtonSettings settings)
        {
            base.TryUpdateDescriptionState(settings);

            // Shitty hack, cuz this method can be called on enable, before OnShow etc.
            subscriptionOffer = subscriptionOffer ?? GameServices.Instance.ProposalService.GetOffer<SubscriptionOffer>();

            // Dirty hack. It's custom Subscription type - Weekly Sale. It was made to avoid plugin changes
            SubscriptionType typeToFind = subscriptionOffer.IsActive ?
                (SubscriptionType)OfferSubscriptionType : SubscriptionType.Weekly;

            SubscriptionButtonSettings foundSettings = subscriptionButtonSettings.Find(e => e.subscriptionType == typeToFind);
            // One more dirty hack, idk why it's null. Vladislav.K
            foundSettings.storeItem = GetSubscriptionStoreItem(foundSettings.subscriptionType);

            string price = string.IsNullOrEmpty(foundSettings.storeItem.LocalizedPrice) ?
                    $"${foundSettings.storeItem.TierPrice:F2}" : foundSettings.storeItem.LocalizedPrice;

            if (descriptionLabel.TryGetComponent(out Localize descriptionLabelLoc))
            {
                descriptionLabelLoc.SetStringParams(price, AccountType, StoreType);
            }
        }


        protected override void ShowSubscriptionPurchasedPopup()
        {
            // hotfix. We show purchased popup as daily reward popup.
            // Here is also a hot fix for unity editor. Can't always hide screen because of wrong ui manager lifecycle
            if (!SubscriptionManager.Instance.IsRewardPopupAvailable)
            {
                Hide();
            }
        }

        private void CreateFreezeSprites(AnimationEffectPlayer from)
        {
            FreezeObjectsData data = Array.Find(freezeObjectsData, e => e.animationEffectPlayer == from);

            if (data != null)
            {
                //settings.spriteScaleAnimation.SetupEndValue(spriteAndScale.Item2);
                data.freezeImage.transform.localScale = Vector3.zero;
                settings.spriteScaleAnimation.Play((value) => data.freezeImage.transform.localScale = value, this);
            }
        }


        private void DestroyFreezeSprites(AnimationEffectPlayer from)
        {
            FreezeObjectsData data = Array.Find(freezeObjectsData, e => e.destroyAnimationEffectPlayer == from);

            if (data != null)
            {
                data.freezeImage.transform.localScale = Vector3.zero;
            }
        }


        private void RefreshOfferTimeLeft()
        {
            offerTimerLeftText.text = subscriptionOffer.TimeUi;
            CommonUtility.SetObjectActive(offerTimerRoot, subscriptionOffer.IsActive);
        }


        private void RefreshLocalizationText()
        {
            IAbTestService abTestService = GameServices.Instance.AbTestService;

            rewardProfitText.SetStringParams(abTestService.CommonData.subscriptionReward.ToShortFormat());
            rewardPremiumCurrencyProfitText.SetStringParams(abTestService.CommonData.subscriptionPremiumCurrencyReward.ToShortFormat());
        }

        #endregion
    }
}
