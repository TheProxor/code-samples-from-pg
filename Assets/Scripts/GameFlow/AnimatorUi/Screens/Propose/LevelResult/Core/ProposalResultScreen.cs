using System;
using Modules.Sound;
using Drawmasters.Proposal;
using Drawmasters.Proposal.Interfaces;
using Modules.Analytics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using I2.Loc;


namespace Drawmasters.Ui
{
    public abstract class ProposalResultScreen : RewardReceiveScreen, IProposalRewardScreen
    {
        #region Fields

        public static Action<RewardData> OnShouldRewardReceive;
        
        [SerializeField] private Button nextButton = default;

        [SerializeField] private ShopResultElement[] resultShopElements = default;

        [SerializeField] private TMP_Text skipText = default;

        private RewardData[] currentReward;

        #endregion



        #region Methods

        public void SetReward(RewardData[] _currentReward)
        {
            currentReward = _currentReward;

            if (resultShopElements.Length != currentReward.Length)
            {
                CustomDebug.Log($"Reward pack and UI cells length not equal in {this}");

                Hide();
            }
            else
            {
                for (int i = 0; i < resultShopElements.Length; i++)
                {
                    bool isSafeIndex = true;
                    isSafeIndex = i >= 0 && i < resultShopElements.Length;
                    isSafeIndex = i >= 0 && i < currentReward.Length;
                    if (!isSafeIndex)
                    {
                        continue;
                    }

                    ShopResultElement shopResultElement = resultShopElements[i];
                    if (shopResultElement.IsNull())
                    {
                        continue;
                    }

                    RewardData rewardData = currentReward[i];
                    if (rewardData == null)
                    {
                        continue;
                    }

                    shopResultElement.InitializeVideoButton();
                    shopResultElement.Initialize(rewardData);

                    shopResultElement.InitializeButtons();
                    shopResultElement.OnShouldReceive += RouletteScreen_OnShouldReceive;
                }

                if (skipText.TryGetComponent(out Localize skipTextLoc))
                {
                    skipTextLoc.SetTerm(IngameData.Settings.commonRewardSettings.nothingReceivedSkipKey);
                }
            }

        }

        public override Vector3 GetCurrencyStartPosition(RewardData rewardData)
        {
            Vector3 result = Vector3.zero;
            
            ShopResultElement element = resultShopElements.Find(i => i.RewardData == rewardData);
            if (element != null)
            {
                result = element.RewardIconPosition;
            }

            return result;
        }

        #endregion


        #region Overrided methods

        public override void Initialize(Action<AnimatorView> onShowEndCallback = null,
                                        Action<AnimatorView> onHideEndCallback = null,
                                        Action<AnimatorView> onShowBeginCallback = null,
                                        Action<AnimatorView> onHideBeginCallback = null)
        {
            base.Initialize(onShowEndCallback, onHideEndCallback, onShowBeginCallback, onHideBeginCallback);

            uiHudTop.InitializeCurrencyRefresh();
            uiHudTop.SetupExcludedTypes(CurrencyType.RollBones);
            uiHudTop.RefreshCurrencyVisual(0.0f);
        }

        public override void Deinitialize()
        {
            base.Deinitialize();

            foreach (var element in resultShopElements)
            {
                element.Deinitialize();
                element.DeinitializeButtons();
                element.OnShouldReceive -= RouletteScreen_OnShouldReceive;
            }

            currentReward = null;
            uiHudTop.DeinitializeCurrencyRefresh();
        }


        public override void Show()
        {
            base.Show();

            nextButton.interactable = false;
            skipText.alpha = default;
            IngameData.Settings.commonRewardSettings.SkipRootAlphaAnimation.Play((value) => skipText.alpha = value, this, () => nextButton.interactable = true);
            SoundManager.Instance.PlayOneShot(AudioKeys.Ui.SHOPENTER);
        }


        public override void InitializeButtons()
        {
            nextButton.onClick.AddListener(NextButton_OnClick);
        }


        public override void DeinitializeButtons()
        {
            nextButton.onClick.RemoveListener(NextButton_OnClick);
        }

        #endregion



        #region Events handlers

        private void NextButton_OnClick()
        {
            DeinitializeButtons();

            Hide();
        }


        private void RouletteScreen_OnShouldReceive(UiRewardElement receivedElement,
                                                    UiRewardElement.RecievePlacement placement)
        {
            bool wasReceived = false;

            if (receivedElement.IsForCurrency)
            {
                IProposable proposable = new CurrencyProposal((receivedElement.RewardData.price, receivedElement.RewardData.priceType));
                proposable.Propose((result) => wasReceived = result);
            }
            else
            {
                wasReceived = true;
            }

            if (wasReceived)
            {
                if (placement == UiRewardElement.RecievePlacement.ForVideo)
                {
                    CommonEvents.SendAdVideoReward(receivedElement.AdsPlacement);
                }
                
                Action onClaimed = () =>
                {
                    receivedElement.SetState(UiRewardElement.State.Received);

                    if (skipText.TryGetComponent(out Localize skipTextLoc))
                    {
                        skipTextLoc.SetTerm(IngameData.Settings.commonRewardSettings.somethingReceivedSkipKey);
                    }

                    OnShouldRewardReceive?.Invoke(receivedElement.RewardData);
                };
                if (receivedElement.RewardData is CurrencyReward)
                {
                    OnShouldApplyReward(receivedElement.RewardData, () =>
                    {
                        onClaimed?.Invoke();
                    });
                }
                else
                {
                    receivedElement.RewardData.Open();
                    receivedElement.RewardData.Apply();
                    
                    onClaimed?.Invoke();
                }
            }
        }

        #endregion
    }
}
