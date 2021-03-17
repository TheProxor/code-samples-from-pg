using System;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.Effects;
using Drawmasters.Helpers;
using Drawmasters.Proposal;
using Drawmasters.Ui.Settings;
using Modules.General;
using Modules.General.InAppPurchase;
using Modules.InAppPurchase;
using Modules.Sound;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Ui
{
    public class UiCongratulationScreen : AnimatorScreen
    {
        #region Fields

        [SerializeField] [Required] protected Button claimButton = default;

        [SerializeField] protected ShooterSkinData shooterSkinData = default;
        [SerializeField] protected List<StoreItemIconData> storeItemIconDataList = default;
        
        [OdinSerialize, SerializeField] protected List<CellData> currencyData = default;
        
        [SerializeField] protected CellData permanentNoAds = default;
        [SerializeField] protected CellData temporaryNoAds = default;

        [SerializeField] protected UiRewardReceiveScreenSkinHandler.Data skinHandlerData = default;
        [SerializeField] protected IdleEffect idleShineEffect = default;
        [SerializeField] protected IdleEffect confettiEffect = default;
        
        protected UiRewardReceiveScreenSkinHandler skinHandler = default;

        protected Action callback;

        #endregion
        
        
        
        #region Overrided

        public override ScreenType ScreenType => 
            ScreenType.CongratulationScreen;
        

        public override void InitializeButtons()
        {
            claimButton.onClick.AddListener(ClaimButton_OnClick);    
        }
        
        
        public override void DeinitializeButtons()
        {
            claimButton.onClick.RemoveListener(ClaimButton_OnClick);
        }

        #endregion



        #region Public methods

        public void Initialize(RewardData[] rewards, 
            string storeItemIdentifier = null, Action _callback = null)
        {
            callback = _callback;
            
            skinHandler = new UiRewardReceiveScreenSkinHandler(skinHandlerData);
            
            DefineMainIcon(rewards, storeItemIdentifier);
            
            FillCurrencyCells(rewards);
            FillNoAdsCells(storeItemIdentifier);
        }


        public override void Deinitialize()
        {
            base.Deinitialize();
            
            idleShineEffect.StopEffect();
            confettiEffect.StopEffect();
        }

        #endregion



        #region Events handlers

        protected void ClaimButton_OnClick()
        {
            Hide(view => callback?.Invoke(), null);
        }

        #endregion



        #region Protected methods

        protected void DefineMainIcon(RewardData[] rewards, string storeItemIdentifier)
        {
            idleShineEffect.SetFxKey(EffectKeys.FxGUICharacterOpenShine);
            idleShineEffect.CreateAndPlayEffect();

            // confettiEffect.CreateAndPlayEffect();

            ShooterSkinReward shooterSkinReward = rewards.OfType<ShooterSkinReward>().FirstOrDefault();
            if (shooterSkinReward != null &&
                shooterSkinReward.skinType != ShooterSkinType.None)
            {
                skinHandler.SetupReward(shooterSkinReward);

                SoundManager.Instance.PlayOneShot(AudioKeys.Ui.SKIN_NEW_OPEN);
            }
            else if (!string.IsNullOrEmpty(storeItemIdentifier))
            {
                StoreItemIconData iconData = storeItemIconDataList.Find(i => i.storeId == storeItemIdentifier);
                if (iconData != null)
                {
                    CommonUtility.SetObjectActive(iconData.rootGameObject, true);                    
                }
                else
                {
                    CustomDebug.Log($"Not implemented logic.Store id: {storeItemIdentifier}");
                }
            }
            else
            {
                CustomDebug.Log("Not implemented logic.");
            }
        }
        

        protected void FillCurrencyCells(RewardData[] rewards)
        {
            CurrencyReward[] currencyRewards = rewards.OfType<CurrencyReward>().ToArray();
            
            foreach (CurrencyReward reward in currencyRewards)
            {
                CellData cell = currencyData.Find(i => i.currencyType == reward.currencyType);
                if (cell != null)
                {
                    CommonUtility.SetObjectActive(cell.rootGameObject, true);

                    cell.countText.text = $"+{reward.value.ToShortFormat()}";
                }
            }
        }


        protected void FillNoAdsCells(string storeItemIdentifier)
        {
            if (storeItemIdentifier != null)
            {
                if (storeItemIdentifier == IAPs.Keys.Consumable.SeasonPass ||
                    storeItemIdentifier == IAPs.Keys.Consumable.GoldenTicket)
                {
                    CommonUtility.SetObjectActive(temporaryNoAds.rootGameObject, true);
                }
                else if (storeItemIdentifier == IAPs.Keys.NonConsumable.NoAdsId)
                {
                    // CommonUtility.SetObjectActive(permanentNoAds.rootGameObject, true);
                }
            }
        }

        #endregion
    }    
}