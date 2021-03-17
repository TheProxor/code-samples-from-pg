using Drawmasters.Levels;
using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil.Interfaces;
using UnityEngine;


namespace Drawmasters.Proposal
{
    public abstract class RewardController
    {
        #region Fields

        private readonly string uaAllowKey;
        private readonly string uaDeltaLevelsKey;

        protected readonly IAbTestService abTestService;
        protected readonly ILevelEnvironment levelEnvironment;

        #endregion



        #region Class lifecycle

        protected RewardController(string _uaAllowKey,
                                string _uaDeltaLevelsKey,
                                IAbTestService _abTestService,
                                ILevelEnvironment _levelEnvironment)
        {
            uaAllowKey = _uaAllowKey;
            uaDeltaLevelsKey = _uaDeltaLevelsKey;

            abTestService = _abTestService;
            levelEnvironment = _levelEnvironment;

            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
        }


        #endregion



        #region Properties

        public bool AllowToPropose => IsUaAllowDataSetted ? 
            UaAllow : 
            AbAllowToPropose;
        

        protected int ActualCompletedDeltaCounterLevels => 
            IsUaLevelDataSetted ? 
                UaLevelsDeltaCount : 
                AbLevelsDeltaCount;


        public int UaLevelsDeltaCount
        {
            get => CustomPlayerPrefs.GetInt(uaDeltaLevelsKey, 0);
            set => CustomPlayerPrefs.SetInt(uaDeltaLevelsKey, value);
        }


        public bool UaAllow
        {
            get => CustomPlayerPrefs.GetBool(uaAllowKey, true);
            set => CustomPlayerPrefs.SetBool(uaAllowKey, value);
        }


        protected abstract bool AbAllowToPropose { get; }

        protected abstract int AbLevelsDeltaCount { get; }

        private bool IsUaLevelDataSetted => PlayerPrefs.HasKey(uaDeltaLevelsKey);

        private bool IsUaAllowDataSetted => PlayerPrefs.HasKey(uaAllowKey);

        #endregion



        #region Public methods

        public void ClearUaLevelsData() => CustomPlayerPrefs.DeleteKey(uaDeltaLevelsKey);

        public void ClearUaAllowData() => CustomPlayerPrefs.DeleteKey(uaAllowKey);

        #endregion



        #region Protected methods

        protected abstract void OnLevelCompleted(GameMode mode);

        #endregion



        #region Events handlers

        private void Level_OnLevelStateChanged(LevelState levelState)
        {
            if (levelState == LevelState.Finished)
            {
                LevelContext levelContext = levelEnvironment.Context;

                bool isSceneMode = levelContext.SceneMode.IsSceneMode();

                if (isSceneMode || !levelContext.IsEndOfLevel)
                {
                    return;
                }

                OnLevelCompleted(levelContext.Mode);
            }
        }

        #endregion
    }
}
