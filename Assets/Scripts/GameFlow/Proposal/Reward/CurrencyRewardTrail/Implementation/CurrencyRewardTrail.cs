using System;
using DG.Tweening;
using Drawmasters.Effects;
using Drawmasters.Interfaces;
using Drawmasters.Proposal;
using Drawmasters.Ui;
using Modules.General;
using UnityEngine;


namespace Drawmasters
{
    public class CurrencyRewardTrail : ICurrencyRewardTrail
    {
        protected readonly RewardReceiveScreen screen;
        protected readonly VectorAnimation trailAnimation;

        #region Ctor

        public CurrencyRewardTrail(RewardReceiveScreen withScreen)
        {
            screen = withScreen;
            trailAnimation = IngameData.Settings.commonRewardSettings.moneyTrailAnimation;
        }

        #endregion
        
        
        
        #region IDeinitializable
        
        public void Deinitialize()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            DOTween.Kill(screen.transform);
        }
        
        #endregion



        #region ICurrencyRewardTrail
        
        public void PlayTrailFx(CurrencyReward currencyReward, Vector3 startPosition, Action callback = null) =>
            PlayTrailFx(currencyReward.currencyType, startPosition, callback);
        

        public void PlayTrailFx(CurrencyType currencyRewardType, Vector3 startPosition, Action callback = null)
        {
            string fxKey = GetTrailVfx(currencyRewardType);
            
            var handler = EffectManager.Instance.CreateSystem(fxKey, 
                true, 
                startPosition,
                parent: screen.transform);

            float callbackDelay = trailAnimation.delay + trailAnimation.duration;

            if (handler != null)
            {
                int fxSortingOrder = screen.SortingOrder + (ViewManager.OrderOffset - 1);
                handler.SetSortingOrder(fxSortingOrder);

                Scheduler.Instance.CallMethodWithDelay(this, 
                    () => EffectManager.Instance.ReturnHandlerToPool(handler), 
                    callbackDelay + 1f); //additionalTrailHideDelay = 1f

                handler.transform.position = startPosition;
                handler.transform.localScale = Vector3.one;
                handler.Play();

                trailAnimation.SetupBeginValue(startPosition);

                Vector3 finishPosition = screen.GetCurrencyFinishPosition(currencyRewardType);
                
                trailAnimation.SetupEndValue(finishPosition);
                trailAnimation.Play(value => handler.transform.position = value, 
                    screen.transform, 
                    callback);
            }
            else
            {
                Scheduler.Instance.CallMethodWithDelay(this, callback, callbackDelay);
            }
        }
        
        #endregion
        
        protected string GetTrailVfx(CurrencyType type) =>
            IngameData.Settings.commonRewardSettings.FindCurrencyTrailFx(type);
    }
}