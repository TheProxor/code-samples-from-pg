using System;
using System.Collections.Generic;
using DG.Tweening;
using Drawmasters.Interfaces;
using Drawmasters.Proposal;
using Drawmasters.Ui;
using UnityEngine;


namespace Drawmasters
{
    public class CommonCurrencyRewardApplyHelper : IRewardApplyHelper
    {
        #region Fields
        
        protected readonly RewardReceiveScreen screen;
        protected readonly ICurrencyRewardTrail trail;

        protected readonly HashSet<Transform> currencyTargets;
        
        #endregion



        #region Ctor

        public CommonCurrencyRewardApplyHelper(RewardReceiveScreen withScreen)
        {
            screen = withScreen;
            trail = new CurrencyRewardTrail(screen);
            currencyTargets = new HashSet<Transform>();
        }

        #endregion

        

        #region IDeinitializable

        public void Deinitialize()
        {
            DOTween.Kill(this);
            
            trail.Deinitialize();

            foreach (var i in currencyTargets)
            {
                DOTween.Kill(i);
            }
        }

        #endregion
        
        
        
        #region IRewardApplyHelper
        
        public void ApplyReward(RewardData reward, Action onClaimed)
        {
            if (reward is CurrencyReward currencyReward)
            {
                //TODO ??
                if (screen.Hud == null || !CanPlayTrail(currencyReward))
                {
                    currencyReward.Open();
                    currencyReward.Apply();

                    onClaimed?.Invoke();
                    return;
                }

                bool wasSubscribed = screen.Hud.IsSubscribed;

                screen.Hud.DeinitializeCurrencyRefresh();

                currencyReward.Open();
                currencyReward.Apply();

                Transform currencyTransform = screen.GetCurrencyBounceRoot(currencyReward.currencyType);
                
                if (currencyTransform != null)
                {
                    currencyTargets.AddIfNotContains(currencyTransform);
                    
                    DOTween.Kill(currencyTransform, true);
                    
                    IngameData.Settings.commonRewardSettings.scaleInCurrencyAnimation.Play(value => 
                        currencyTransform.localScale = value, currencyTransform, screen.OnCurrencyRootScaledIn);
                    
                    IngameData.Settings.commonRewardSettings.scaleOutCurrencyAnimation.Play(value => 
                        currencyTransform.localScale = value, currencyTransform);
                }

                Vector3 startPosition = screen.GetCurrencyStartPosition(currencyReward);
                
                {
                    trail.PlayTrailFx(currencyReward, startPosition, () =>
                    {
                        screen.Hud.InitializeCurrencyRefresh();

                        if (!wasSubscribed)
                        {
                            screen.Hud.DeinitializeCurrencyRefresh();
                        }

                        screen.Hud.RefreshCurrencyVisual(0.3f);

                        onClaimed?.Invoke();
                    });
                }
            }
        }
        
        #endregion



        #region Protected methods

        protected virtual bool CanPlayTrail(CurrencyReward reward) => true;

        #endregion
    }
}