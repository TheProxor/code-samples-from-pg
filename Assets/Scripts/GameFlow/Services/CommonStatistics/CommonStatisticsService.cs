using System;
using Drawmasters.Levels;
using Drawmasters.Levels.Data;


namespace Drawmasters.ServiceUtil.Interfaces
{
    public class CommonStatisticsService : ICommonStatisticsService
    {
        #region Fields

        private event Action onLevelUp;

        private readonly ILevelEnvironment levelEnvironment;
        private readonly IAbTestService abTestService;

        #endregion



        #region Properties

        private DateTime TodayMatchesCountDate
        {
            get => CustomPlayerPrefs.GetDateTime(PrefsKeys.Analytics.TodayMatchesCountDate);
            set => CustomPlayerPrefs.SetDateTime(PrefsKeys.Analytics.TodayMatchesCountDate, value);
        }


        public bool WasStartSubscrptionShowed =>
            CustomPlayerPrefs.GetBool(PrefsKeys.Subscription.WasShowed);

        public Action OnLevelUp
        {
            get => onLevelUp;
            set => onLevelUp = value;
        }

        #endregion



        #region Ctor

        public CommonStatisticsService(ILevelEnvironment _levelEnvironment,
                                       IAbTestService _abTestService)
        {
            levelEnvironment = _levelEnvironment;
            abTestService = _abTestService;

            CheckFirstLaunchDate();

            SessionCount++;

            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
        }

        #endregion



        #region ICommonStatisticsService

        public int SessionCount
        {
            get => CustomPlayerPrefs.GetInt(PrefsKeys.Analytics.SessionsCount, 0);
            private set => CustomPlayerPrefs.SetInt(PrefsKeys.Analytics.SessionsCount, value);
        }

        public int MatchesCount
        {
            get => CustomPlayerPrefs.GetInt(PrefsKeys.Analytics.SessionsCount, 0);
            private set => CustomPlayerPrefs.SetInt(PrefsKeys.Analytics.SessionsCount, value);
        }

        public int TodayMatchesCount
        {
            get => CustomPlayerPrefs.GetInt(PrefsKeys.Analytics.TodayMatchesCount, 0);
            private set => CustomPlayerPrefs.SetInt(PrefsKeys.Analytics.TodayMatchesCount, value);
        }


        public int UniqueLevelsFinishedCount
        {
            get => CustomPlayerPrefs.GetInt(PrefsKeys.Analytics.UniqueLevelsCount, 0);
            private set => CustomPlayerPrefs.SetInt(PrefsKeys.Analytics.UniqueLevelsCount, value);
        }


        public DateTime FirstLaunchDateTime
        {
            get => CustomPlayerPrefs.GetDateTime(PrefsKeys.Analytics.FirstLaunchDateTime, DateTime.MinValue);
            private set => CustomPlayerPrefs.SetDateTime(PrefsKeys.Analytics.FirstLaunchDateTime, value);
        }

        public int LevelsFinishedCount
        {
            get
            {
                int result = default;

                GameMode[] modes = IngameData.Settings.modesInfo.Modes;

                foreach (var m in modes)
                {
                    if (!m.IsHitmastersLiveOps())
                    {
                        result += GetLevelsFinishedCount(m);    
                    }
                }

                return result;
            }
        }


        public int GetLevelsFinishedCount(GameMode mode) =>
            CustomPlayerPrefs.GetInt(string.Concat(PrefsKeys.Analytics.UniqueLevelsCount, mode), 0);


        public bool IsFirstLaunch =>
            SessionCount == 1;


        public void MarkStartSubscrptionShowed() =>
            CustomPlayerPrefs.SetBool(PrefsKeys.Subscription.WasShowed, true);


        public void ResetLevelsFinishedCount(GameMode mode) =>
            SetLevelsFinishedCount(mode, 0);

        public bool IsIapsAvailable =>
            abTestService.CommonData.isIapsAvailable &&
            !BuildInfo.IsChinaBuild;


        public void SetLevelsFinishedCount(GameMode mode, int value) =>
            CustomPlayerPrefs.SetInt(string.Concat(PrefsKeys.Analytics.UniqueLevelsCount, mode), value);


        #endregion



        #region Private methods

        private void CheckFirstLaunchDate()
        {
            if (!CustomPlayerPrefs.HasKey(PrefsKeys.Analytics.FirstLaunchDateTime))
            {
                FirstLaunchDateTime = DateTime.Now;
            }
        }

        private void IncrementMatchesCount()
        {
            MatchesCount++;

            bool needResetCounter = TimeUtility.IsAtLeastOneDayPassed(TodayMatchesCountDate, DateTime.Now);
            if (needResetCounter)
            {
                TodayMatchesCount = 0;
            }

            TodayMatchesCount++;
            TodayMatchesCountDate = DateTime.Now;
        }
        

        private void IncrementUniqueLevelsCompletedCount()
        {
            //TODO fix logic, need store level id to define whether it is unique
            {
                UniqueLevelsFinishedCount++;
            }
        }


        private void IncrementFinishedLevelsCount()
        {
            GameMode mode = levelEnvironment.Context.Mode;

            int currentValue = GetLevelsFinishedCount(mode);

            SetLevelsFinishedCount(mode, currentValue + 1);
        }

        #endregion



        #region Event handler

        private void Level_OnLevelStateChanged(LevelState levelState)
        {
            //HACK
            if (levelState == LevelState.Unloaded)
            {
                return;
            }

            LevelContext levelContext = levelEnvironment.Context;

            if (!levelContext.Mode.IsPlayable() ||
                 (!levelContext.Mode.IsHitmastersLiveOps() && levelContext.SceneMode != GameMode.None))
            {
                return;
            }
            // increment matches count
            if (levelState == LevelState.Initialized)
            {
                IncrementMatchesCount();
            }
            //increment levels finished
            else if (levelState == LevelState.Finished)
            {
                LevelResult levelResult = levelEnvironment.Progress.LevelResultState;
                LevelContext currentLevelContext = levelEnvironment.Context;

                bool shouldUpLevel = (levelResult.IsLevelAccomplished() ||
                                      (currentLevelContext.Mode.IsHitmastersLiveOps() &&
                                       levelResult.IsLevelOrProposalAccomplished())) &&
                                      GameServices.Instance.LevelEnvironment.Context.IsEndOfLevel;

                if (shouldUpLevel)
                {
                    if (!currentLevelContext.Mode.IsHitmastersLiveOps())
                    {
                        IncrementUniqueLevelsCompletedCount();    
                    }

                    IncrementFinishedLevelsCount();
                    onLevelUp?.Invoke();
                }
            }
        }        

        #endregion
    }
}

