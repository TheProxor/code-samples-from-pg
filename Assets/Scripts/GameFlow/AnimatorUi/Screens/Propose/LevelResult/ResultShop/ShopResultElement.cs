using Drawmasters.Proposal;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Drawmasters.Helpers;
using Modules.Sound;
using Drawmasters.Advertising;
using Drawmasters.Effects;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Ui
{
    public class ShopResultElement : UiRewardElement
    {
        #region Fields

        private const float skinsScaleMultiplier = 0.9f;
        private const float currencyScaleMultiplier = 1.0f;

        [Header("Shop Element Settings")]
        [SerializeField] private Button buyForCurrencyButton = default;
        [SerializeField] private TMP_Text receivedCurrencyText = default;
        [SerializeField] private TMP_Text[] currencyToReceiveTexts = default;

        [SerializeField] private Image[] weaponSkinImages = default;
        [SerializeField] private Image[] currencyIcons = default;

        [SerializeField] private TMP_Text priceText = default;
        [SerializeField] private Transform fxReceivedRoot = default;

        #endregion



        #region Properties

        public override string AdsPlacement => AdsVideoPlaceKeys.ShopResult;

        public Vector3 RewardIconPosition => fxReceivedRoot.position;

        #endregion



        #region Methods

        public override void InitializeButtons()
        {
            base.InitializeButtons();

            buyForCurrencyButton.onClick.AddListener(CurrencyButton_OnPressed);
        }


        public override void DeinitializeButtons()
        {
            buyForCurrencyButton.onClick.RemoveListener(CurrencyButton_OnPressed);

            base.DeinitializeButtons();
        }


        public override void Initialize(RewardData rewardData)
        {
            base.Initialize(rewardData);

            bool isCurrencyReward = RewardData is CurrencyReward;
            CommonUtility.SetObjectActive(receivedCurrencyText.gameObject, isCurrencyReward);

            foreach (var text in currencyToReceiveTexts)
            {
                CommonUtility.SetObjectActive(text.gameObject, isCurrencyReward);
            }

            Sprite spriteToSet = RewardData.GetUiSprite();

            if (RewardData is CurrencyReward currencyReward)
            {
                receivedCurrencyText.text = string.Format(CurrencyFormat, currencyReward.UiRewardText);

                foreach (var text in currencyToReceiveTexts)
                {
                    text.text = string.Format(CurrencyFormat, currencyReward.UiRewardText);
                }
            }

            foreach (var icon in currencyIcons)
            {
                icon.sprite = RewardData.GetUiShopPriceSprite();
                icon.SetNativeSize();
            }

            foreach (var weaponSkinImage in weaponSkinImages)
            {
                weaponSkinImage.sprite = spriteToSet;
                weaponSkinImage.SetNativeSize();

                float scaleMultiplier = isCurrencyReward ? currencyScaleMultiplier : skinsScaleMultiplier;
                weaponSkinImage.transform.localScale = Vector3.one * scaleMultiplier;
            }

            if (RewardData.IsPurchasable)
            {
                priceText.text = RewardData.price.ToShortFormat();
            }

            State stateToSet = default;

            switch (rewardData.receiveType)
            {
                case RewardDataReceiveType.Default:
                    stateToSet = State.Free;
                    break;

                case RewardDataReceiveType.Currency:
                    stateToSet = State.ForCurrency;
                    break;

                case RewardDataReceiveType.Video:
                    stateToSet = State.ForVideo;
                    break;

                default:
                    CustomDebug.Log($"No state for {rewardData.receiveType} in {this}");
                    break;
            }

            SetState(stateToSet);
        }


        protected override void OnReceivedFromVideo()
        {
            base.OnReceivedFromVideo();

            EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIButtonBuyClick, parent: fxReceivedRoot, transformMode: TransformMode.Local);
        }

        protected override void ReceiveReward(RecievePlacement placement)
        {
            base.ReceiveReward(placement);

            SoundManager.Instance.PlayOneShot(AudioKeys.Ui.BUYITEM);
        }

        #endregion



        #region Events handlers

        private void CurrencyButton_OnPressed()
        {
            if (RewardData.EnoughCurrencyForPurchase)
            {
                ReceiveReward(RecievePlacement.ForCurrency);

                EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIButtonBuyClick, parent: fxReceivedRoot, transformMode: TransformMode.Local);
            }
            else
            {
                bool isIAPsAvailable = GameServices.Instance.CommonStatisticService.IsIapsAvailable;

                if (isIAPsAvailable)
                {
                    GameServices.Instance.ProposalService.CurrencyShopProposal.Propose(RewardData.priceType);
                }
                else
                {
                    DialogPopupSettings settings = IngameData.Settings.dialogPopupSettings;
                    
                    bool isPopupExist = settings.TryGetNotEnoughPopupType(RewardData.priceType, 
                        out OkPopupType popupType);
                    
                    if (isPopupExist)
                    {
                        UiScreenManager.Instance.ShowPopup(popupType);
                    }
                }
            }
        }

        #endregion
    }
}