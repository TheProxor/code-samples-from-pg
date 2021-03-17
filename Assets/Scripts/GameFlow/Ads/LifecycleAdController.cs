using Modules.Advertising;
using Drawmasters.Ui;
using System.Collections.Generic;
using System;
using Drawmasters.Proposal;

namespace Drawmasters.Advertising
{
    public class LifecycleAdController
    {
        #region Fields

        private const float InactivityTimerCheckDeltaTime = 1f;

        private static readonly ScreenType[] ScreensToLockTimer = { ScreenType.SeasonPassScreen,
                                                                    ScreenType.SubscriptionScreenRoot,
                                                                    ScreenType.ShopMenu };

        private static readonly object MonopolyAvailabilityToken = new object();

        private readonly List<object> timerBlockers = new List<object>();

        private float currentDeltaTime;
        private bool needUpdate;
        private bool wasGameLoaded;


        #endregion



        #region Ctor


        public LifecycleAdController()
        {
            SetTimerBlocked(false, this);

            TouchManager.OnBeganTouchAnywhere += TouchManager_OnBeganTouchAnywhere;
            TouchManager.OnEndTouchAnywhere += TouchManager_OnEndTouchAnywhere;

            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;

            UiMonopolyScreen.OnRollsStarted += UiMonopolyScreen_OnRollsStarted;

            AnimatorScreen.OnScreenShow += AnimatorScreen_OnScreenShow;
            AnimatorScreen.OnScreenHide += AnimatorScreen_OnScreenHide;

            UiProposal.OnTutorialProposeStarted += UiProposal_OnTutorialProposeStarted;
            UiProposal.OnTutorialProposeFinished += UiProposal_OnTutorialProposeFinished;

            ApplicationManager.OnApplicationStarted += ApplicationManager_OnApplicationStarted;
        }

        #endregion



        #region Private methods

        private void SetTimerBlocked(bool blocked, object target)
        {
            if (blocked)
            {
                timerBlockers.AddExclusive(target);
            }
            else
            {
                timerBlockers.Remove(target);
            }

            needUpdate = timerBlockers.Count == 0;
        }

        #endregion



        #region Events handlers

        private void TouchManager_OnBeganTouchAnywhere()
        {
            SetTimerBlocked(true, this);

            AdvertisingManager.Instance.ResetInactivityTimer();
        }


        private void TouchManager_OnEndTouchAnywhere() =>
            SetTimerBlocked(false, this);


        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            if (needUpdate && wasGameLoaded)
            {
                currentDeltaTime += deltaTime;

                if (currentDeltaTime > InactivityTimerCheckDeltaTime)
                {
                    AdvertisingManager.Instance.UpdateInactivityTimer(currentDeltaTime);

                    currentDeltaTime -= InactivityTimerCheckDeltaTime;
                }
            }
        }


        private void UiMonopolyScreen_OnRollsStarted()
        {
            SetTimerBlocked(true, MonopolyAvailabilityToken);

            UiMonopolyScreen.OnRollsFinished += UiMonopolyScreen_OnRollsFinished;
        }


        private void UiMonopolyScreen_OnRollsFinished()
        {
            UiMonopolyScreen.OnRollsFinished -= UiMonopolyScreen_OnRollsFinished;
            
            SetTimerBlocked(false, MonopolyAvailabilityToken);
        }


        private void AnimatorScreen_OnScreenShow(ScreenType screenType)
        {
            bool isScreenToLockTimer = Array.Exists(ScreensToLockTimer, e => e == screenType);

            if (isScreenToLockTimer)
            {
                SetTimerBlocked(true, screenType);
            }
        }


        private void AnimatorScreen_OnScreenHide(ScreenType screenType) =>
            SetTimerBlocked(false, screenType);


        private void UiProposal_OnTutorialProposeStarted(object key) =>
            SetTimerBlocked(true, key);


        private void UiProposal_OnTutorialProposeFinished(object key) =>
            SetTimerBlocked(false, key);

        private void ApplicationManager_OnApplicationStarted() =>
            wasGameLoaded = true;

        #endregion
    }
}
