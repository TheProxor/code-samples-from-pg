using System;
using System.Collections.Generic;
using Drawmasters.Interfaces;
using Drawmasters.Proposal;
using UnityEngine;


namespace Drawmasters.Ui
{
    public abstract class RewardReceiveScreen : AnimatorScreen
    {
        #region Fields

        [SerializeField] protected UiHudTop uiHudTop = default;

        protected Dictionary<RewardType, IRewardApplyHelper> helpers;
        protected IRewardApplyHelper instantHelper;

        #endregion

        

        #region Properties

        public UiHudTop Hud => uiHudTop;
        
        public abstract Vector3 GetCurrencyStartPosition(RewardData rewardData);

        public virtual Vector3 GetCurrencyFinishPosition(CurrencyType currencyType) =>
            uiHudTop.FindCurrencyTextTransform(currencyType).position;

        public virtual Transform GetCurrencyBounceRoot(CurrencyType currencyType) =>
            uiHudTop.FindCurrencyRootTransform(currencyType);

        public virtual string RewardScreenIdleFxKey =>
            EffectKeys.FxGUICharacterOpenShine;

        protected virtual IRewardApplyHelper InstantHelper =>
            instantHelper ?? (instantHelper = new ImmediatelyRewardApplyHelper());

        protected virtual Dictionary<RewardType, IRewardApplyHelper> RewardApplyHelpers =>
            helpers ?? (helpers = new Dictionary<RewardType, IRewardApplyHelper>()
            {
                {RewardType.PetSkin, new CommonSkinRewardApplyHelper(this)},
                {RewardType.ShooterSkin, new CommonSkinRewardApplyHelper(this)},
                {RewardType.WeaponSkin, new CommonSkinRewardApplyHelper(this)},
                {RewardType.Currency, new CommonCurrencyRewardApplyHelper(this)},
                {
                    RewardType.SpinRouletteCash,
                    new SpinRouletteRewardApplyHelper<SpinRouletteCashReward>(IngameData.Settings.seasonEvent
                        .spinRouletteCashSeasonEventRewardPackSettings)
                },
                {
                    RewardType.SpinRouletteSkin,
                    new SpinRouletteRewardApplyHelper<SpinRouletteSkinReward>(IngameData.Settings.seasonEvent
                        .spinRouletteSkinSeasonEventRewardPackSettings)
                },
                {
                    RewardType.SpinRouletteWaipon,
                    new SpinRouletteRewardApplyHelper<SpinRouletteWaiponReward>(IngameData.Settings.seasonEvent
                        .spinRouletteWaiponSeasonEventRewardPackSettings)
                },
                {
                    RewardType.Forcemeter,
                    new ForcemeterRewardApplyHelper(IngameData.Settings.seasonEvent
                        .forceMeterSeasonEventRewardPackSettings)
                },
                {RewardType.None, new DefaultRewardApplyHelper()}
            });

        #endregion



        #region Public methods

        public virtual void OnCurrencyRootScaledIn() { }

        public override void Deinitialize()
        {
            if (helpers != null)
            {
                foreach (var helper in helpers)
                {
                    helper.Value.Deinitialize();
                }
            }

            base.Deinitialize();
        }
        
        
        #endregion



        #region Protected methods

        protected void OnShouldApplyReward(RewardData receivedRewardData, bool isImmediately = false) =>
            OnShouldApplyReward(receivedRewardData, null, isImmediately);


        protected void OnShouldApplyReward(RewardData receivedRewardData, 
            Action callback, bool isImmediately = false)
        {
            if (receivedRewardData == null)
            {
                callback?.Invoke();
                return;
            }

            RewardType receivedType = receivedRewardData.Type;
            bool isSkin = receivedType == RewardType.PetSkin ||
                          receivedType == RewardType.ShooterSkin ||
                          receivedType == RewardType.WeaponSkin;
            
            if (isImmediately && isSkin)
            {
                InstantHelper.ApplyReward(receivedRewardData, callback);
            }
            else if (RewardApplyHelpers.TryGetValue(receivedRewardData.Type, out IRewardApplyHelper helper))
            {
                helper.ApplyReward(receivedRewardData, callback);
            }
        }


        protected void OnShouldApplyReward(RewardData[] rewardData, Action callback = default)
        {
            IRewardApplyHelper helper = RewardApplyHelpers[RewardType.Currency];
            
            foreach (var rd in rewardData)
            {
                if (rd.Type == RewardType.Currency)
                {
                    helper.ApplyReward(rd, null);
                }
                else
                {
                    rd.Open();
                }
            }

            UiScreenManager.Instance.ShowScreen(ScreenType.SpinReward, onShowBegin: (view) =>
            {
                if (view is UiRewardReceiveScreen rewardScreen)
                {
                    rewardScreen.SetupFxKey(RewardScreenIdleFxKey);
                    rewardScreen.SetupReward(rewardData);
                }
            }, onHided: (hidedView) => callback?.Invoke());
        }

        #endregion
    }
}
