using System;
using Drawmasters.AbTesting;
using Drawmasters.Interfaces;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Ui;
using UnityEngine;


namespace Drawmasters.Proposal
{
    public class RateUsProposal
    {
        #region Fields

        public static IUaAbTestMechanic UaAbTestEveryLevel { get; } = new CommonMechanicAvailability(PrefsKeys.Dev.DevRateUsEveryLevel);

        // we incerement matches count in start
        private const int matchesDelay = 5 + 1;

        public const string RateIOSUrl = "https://apps.apple.com/app/id1524936482";
        public const string RateAndroidUrl = "https://play.google.com/store/apps/details?id=com.romank.drawmasters";

        private readonly ICommonStatisticsService commonStatistic;
        private readonly IRateUsService rateUsService;
        private readonly IAbTestService abTestService;

        #endregion



        #region Ctor

        public RateUsProposal(ICommonStatisticsService _commonStatistic,
                              IRateUsService _rateUsService,
                              IAbTestService _abTestService)
        {
            commonStatistic = _commonStatistic;
            rateUsService = _rateUsService;
            abTestService = _abTestService;
        }

        #endregion



        #region Properties

        private DateTime ProposeDate
        {
            get => CustomPlayerPrefs.GetDateTime(PrefsKeys.Proposal.RateUsProposingData, DateTime.MinValue);
            set => CustomPlayerPrefs.SetDateTime(PrefsKeys.Proposal.RateUsProposingData, value);
        }

        private int ProposeMatchesCount
        {
            get => CustomPlayerPrefs.GetInt(PrefsKeys.Proposal.RateUsProposingMatches);
            set => CustomPlayerPrefs.SetInt(PrefsKeys.Proposal.RateUsProposingMatches, value);
        }


        private bool IsAtLeastOneDayPassed =>
            TimeUtility.IsAtLeastOneDayPassed(ProposeDate, DateTime.Now);


        private bool IsMatchesDelaySpend =>
            (commonStatistic.MatchesCount - ProposeMatchesCount) >= matchesDelay;


        private bool IsTodayMatchesDelaySpend =>
            commonStatistic.TodayMatchesCount >= matchesDelay;


        private bool ShouldShowEveryLevelUA =>
            UaAbTestEveryLevel.WasAvailabilityChanged && UaAbTestEveryLevel.IsMechanicAvailable;


        public bool CanPropose
        {
            get
            {
                bool canPropose = true;

                canPropose &= IsAtLeastOneDayPassed;
                canPropose &= IsMatchesDelaySpend;
                canPropose &= IsTodayMatchesDelaySpend;
                canPropose &= rateUsService.IsAvailable;
                canPropose &= !rateUsService.IsRated;

                canPropose |= ShouldShowEveryLevelUA;

                return canPropose;
            }
        }

        #endregion



        #region Methods

        public void Propose(Action onProposed)
        {
            if (CanPropose)
            {
                ProposeDate = DateTime.Now;
                ProposeMatchesCount = commonStatistic.MatchesCount;

                if (LLRatePopupManager.IsAvalaiblePopUp)
                {
                    LLRatePopupManager.ShowPopUp();

                    rateUsService.RateApplication();

                    onProposed?.Invoke();
                }
                else
                {
                    UiScreenManager.Instance.ShowScreen(ScreenType.RateUsFeedbackScreen, onHided: view =>
                       {
                           onProposed?.Invoke();
                       });
                }
            }
            else
            {
                onProposed?.Invoke();
            }
        }


        public void RateRequset()
        {
            #if UNITY_IOS
                Application.OpenURL(RateIOSUrl);
            #elif UNITY_ANDROID
                Application.OpenURL(RateAndroidUrl);
            #endif

            rateUsService.RateApplication();
        }

#endregion
    }
}
