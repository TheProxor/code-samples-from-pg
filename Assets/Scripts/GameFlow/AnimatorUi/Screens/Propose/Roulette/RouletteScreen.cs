using System;
using System.Linq;
using DG.Tweening;
using Modules.Sound;
using Drawmasters.Proposal;
using Modules.Analytics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.ServiceUtil;
using I2.Loc;


namespace Drawmasters.Ui
{
    public class RouletteScreen : RewardReceiveScreen, IProposalRewardScreen
    {
        #region Nested types

        [Serializable]
        private class BestRewardVisual
        {
            public RewardType rewardType = default;
            public GameObject root = default;
        }

        #endregion



        #region Fields
        
        public static Action<RewardData> OnShouldRewardReceive;
        
        private const string CurrencyFormat = "+{0}";

        [SerializeField] private Button nextButton = default;

        [SerializeField] private RouletteElement[] rouletteElements = default;

        [Header("Best reward visual")]
        [SerializeField] private BestRewardVisual[] bestRewardVisuals = default;
        [SerializeField] private TMP_Text bestRewardCurrency = default;
        [SerializeField] private Image[] bestRewardIcons = default;
        [SerializeField] private TMP_Text skipText = default;

        private RewardData[] currentReward;

        #endregion



        #region Properties

        public override ScreenType ScreenType => ScreenType.Roulette;

        #endregion



        #region Methods

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null, Action<AnimatorView> onHideEndCallback = null, Action<AnimatorView> onShowBeginCallback = null,
            Action<AnimatorView> onHideBeginCallback = null)
        {
            base.Initialize(onShowEndCallback, onHideEndCallback, onShowBeginCallback, onHideBeginCallback);
            
            uiHudTop.InitializeCurrencyRefresh();
            uiHudTop.SetupExcludedTypes(CurrencyType.RollBones);
            uiHudTop.RefreshCurrencyVisual(0.0f);
        }

        public void SetReward(RewardData[] _currentReward)
        {
            currentReward = _currentReward;

            if (rouletteElements.Length != currentReward.Length)
            {
                CustomDebug.Log($"Reward pack and UI cells length not equal in {this}");
                Hide();
            }

            foreach (var element in rouletteElements)
            {
                element.InitializeVideoButton();
                element.SetState(UiRewardElement.State.Free);
                element.OnShouldReceive += RouletteScreen_OnShouldReceive;
            }

            if (skipText.TryGetComponent(out Localize skipTextLoc))
            {
                skipTextLoc.SetTerm(IngameData.Settings.commonRewardSettings.nothingReceivedSkipKey);
            }

            RewardData bestReward = FindBestReward();

            if (bestReward != null)
            {
                foreach (var bestRewardVisual in bestRewardVisuals)
                {
                    CommonUtility.SetObjectActive(bestRewardVisual.root, bestRewardVisual.rewardType == bestReward.Type);
                }

                if (bestReward is CurrencyReward currencyReward)
                {
                    bestRewardCurrency.text = string.Format(CurrencyFormat, currencyReward.value);
                }

                Sprite skinSpriteToSet = bestReward.GetUiSprite();

                foreach (var bestRewardIcon in bestRewardIcons)
                {
                    bestRewardIcon.sprite = skinSpriteToSet;
                    bestRewardIcon.SetNativeSize();
                }
            }
        }


        private RewardData FindBestReward()
        {
            RewardData result = Array.Find(currentReward, e =>
            {
                if (e is CurrencyReward cr)
                {
                    return cr.currencyType == CurrencyType.MansionHammers || cr.currencyType == CurrencyType.RollBones;
                }
                else return e.Type == RewardType.ShooterSkin || e.Type == RewardType.WeaponSkin;
            });

            if (result == null)
            {
                RewardData[] commonReward = currentReward
                                .Where(e => e is CurrencyReward cr)
                                .ToArray();

                RewardData[] premiumReward = commonReward
                                .Where(e => e is CurrencyReward cr && cr.currencyType == CurrencyType.Premium)
                                .ToArray();

                RewardData[] arrayToCheck = premiumReward.Length > 0 ? premiumReward : commonReward;

                result = arrayToCheck
                            .OrderByDescending(e => (e as CurrencyReward).value)
                            .First();

                if (result == null)
                {
                    CustomDebug.Log("No best reward data found");
                }
            }

            return result;
        }

        #endregion



        #region Overrided methods

        public override void Deinitialize()
        {
            foreach (var element in rouletteElements)
            {
                element.Deinitialize();
                element.OnShouldReceive -= RouletteScreen_OnShouldReceive;
            }

            DOTween.Kill(this);
            currentReward = null;
            
            uiHudTop.DeinitializeCurrencyRefresh();

            base.Deinitialize();
        }

        public override Vector3 GetCurrencyStartPosition(RewardData rewardData)
        {
            Vector3 result = Vector3.zero;
            
            RouletteElement element = rouletteElements.Find(i => i.RewardData == rewardData);

            if (element != null)
            {
                result = element.RewardIconPosition;
            }

            return result;
        }


        public override void Show()
        {
            base.Show();

            nextButton.interactable = false;
            skipText.alpha = default;
            
            IngameData.Settings.commonRewardSettings.SkipRootAlphaAnimation.Play(value => 
                skipText.alpha = value, this, () => nextButton.interactable = true);
            
            SoundManager.Instance.PlayOneShot(AudioKeys.Ui.SHOPENTER);
        }


        public override void InitializeButtons()
        {
            foreach (var element in rouletteElements)
            {
                element.InitializeButtons();
            }

            nextButton.onClick.AddListener(Hide);
        }


        public override void DeinitializeButtons()
        {
            foreach (var element in rouletteElements)
            {
                element.DeinitializeButtons();
            }

            nextButton.onClick.RemoveListener(Hide);
        }

        #endregion



        #region Events handlers

        private void RouletteScreen_OnShouldReceive(UiRewardElement receivedElement,
                                                    UiRewardElement.RecievePlacement placement)
        {
            RewardData[] availableReward = currentReward
                                                    .Where(e => !Array.Exists(rouletteElements, r => r.RewardData == e))
                                                    .ToArray();

            RewardData bestReward = FindBestReward();

            RewardData[] receivedReward = currentReward
                                                    .Where(e => !Array.Exists(availableReward, r => r == e))
                                                    .ToArray();

            bool isBestRewardClaimed = Array.Exists(receivedReward, e => e == bestReward);

            RewardData rewardToReceive = default;

            if (!isBestRewardClaimed)
            {
                RouletteRewardPackSettings settings = IngameData.Settings.rouletteRewardSettings;
                int currentIterationIndex = receivedReward.Length;
                int showNumber = GameServices.Instance.ProposalService.RouletteRewardController.ShowsCount - 1;
                bool shouldGiveBestReward = settings.ShouldGiveBestReward(showNumber, currentIterationIndex);

                rewardToReceive = shouldGiveBestReward ?
                    bestReward : availableReward.Where(e => e != bestReward).ToArray().RandomObject();
            }
            else
            {
                rewardToReceive = availableReward.RandomObject();
            }

            receivedElement.Initialize(rewardToReceive);

            if (placement == UiRewardElement.RecievePlacement.ForVideo)
            {
                CommonEvents.SendAdVideoReward(receivedElement.AdsPlacement);
            }

            Action onClaimed = () =>
            {
                receivedElement.SetState(UiRewardElement.State.Received);

                foreach (var element in rouletteElements)
                {
                    if (!element.IsReceived && !element.IsForVideo)
                    {
                        element.PlayMoveupAnimation();
                        element.SetState(UiRewardElement.State.ForVideo);
                    }
                }

                if (skipText.TryGetComponent(out Localize skipTextLoc))
                {
                    skipTextLoc.SetTerm(IngameData.Settings.commonRewardSettings.somethingReceivedSkipKey);
                }

                OnShouldRewardReceive?.Invoke(receivedElement.RewardData);
            };

            if (rewardToReceive is CurrencyReward)
            {
                OnShouldApplyReward(rewardToReceive);
            }
            else
            {
                rewardToReceive.Open();
                rewardToReceive.Apply();
            }

            onClaimed?.Invoke();
        }

        #endregion
    }
}
