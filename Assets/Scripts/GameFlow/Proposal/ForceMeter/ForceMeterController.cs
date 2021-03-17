using System;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Ui;
using Drawmasters.Proposal.Interfaces;


namespace Drawmasters.Proposal
{
    public class ForceMeterController : ModesRewardController, IShowsCount
    {
        #region Properties

        public ForceMeterRewardPackSettings Settings { get; }

        public override bool IsAvailable(GameMode mode) =>
            base.IsAvailable(mode) && 
            IsMinLevelReached(mode);

        
        protected override bool AbAllowToPropose => 
            abTestService.CommonData.isForceMeterProposalAvailable;
        

        protected override int AbLevelsDeltaCount =>
            abTestService.CommonData.finishedLevelsCountForForcemeterPropose;


        public int UaMinLevel
        {
            get => CustomPlayerPrefs.GetInt(PrefsKeys.AbTest.UaForcemeterMinLevel, abTestService.CommonData.minFinishedLevelsForForcemeterPropose);
            set => CustomPlayerPrefs.SetInt(PrefsKeys.AbTest.UaForcemeterMinLevel, value);
        }

        
        private int MinLevelForPropose => 
            IsUaMinLevelDataSetted ?
                UaMinLevel : 
                abTestService.CommonData.minFinishedLevelsForForcemeterPropose;

        
        private bool IsUaMinLevelDataSetted =>
            CustomPlayerPrefs.HasKey(PrefsKeys.AbTest.UaForcemeterMinLevel);

        #endregion



        #region Class lifecycle

        public ForceMeterController(ForceMeterRewardPackSettings _settings,
                                    string _levelsCounterPrefix,
                                    string uaAllowKey,
                                    string _uaDeltaLevelsKey,
                                    IAbTestService abTestService,
                                    ILevelEnvironment levelEnvironment)
            : base(_levelsCounterPrefix,
                   uaAllowKey,
                   _uaDeltaLevelsKey,
                   abTestService,
                   levelEnvironment)
        {
            Settings = _settings;
        }

        #endregion



        #region Public methods

        public void Propose(GameMode mode, Action hidedCallback)
        {
            if (IsAvailable(mode))
            {
                int showIndex = ShowsCount;
                ShowsCount++;
                InstantPropose(hidedCallback, Settings, showIndex);
               
                MarkProposed(mode);
            }
            else
            {
                hidedCallback?.Invoke();
            }
        }


        public void InstantPropose(Action hidedCallback, int showIndex = -1) =>
            InstantPropose(hidedCallback, Settings, showIndex);


        public void InstantPropose(Action hidedCallback, 
            SequenceRewardPackSettings sequenceRewardPackSettings, 
            int showIndex = -1, 
            Action hideBeginCallback = default)
        {
            UiScreenManager.Instance.ShowScreen(ScreenType.ForceMeter, onShowBegin: showedView =>
            {
                if (showedView is ForceMeterScreen forceMeterScreen)
                {
                    RewardData[] rewardData = sequenceRewardPackSettings.GetRewardPack(showIndex);
                    forceMeterScreen.SetReward(rewardData);
                }
            },
            onHided: view => hidedCallback?.Invoke(),
            onHideBegin: hiveView => hideBeginCallback?.Invoke());
        }
        
        #endregion



        #region Protected methods

        protected override void OnLevelCompleted(GameMode mode)
        {
            if (IsMinLevelReached(mode))
            {
                IncrementCompletedLevels(mode);
            }
        }

        #endregion



        #region Private methods

        private bool IsMinLevelReached(GameMode mode) => 
            mode.GetFinishedLevels() >= MinLevelForPropose;

        #endregion



        #region IShowsCount

        public int ShowsCount
        {
            get => CustomPlayerPrefs.GetInt(PrefsKeys.Proposal.ForcemeterShowCounter);
            set => CustomPlayerPrefs.SetInt(PrefsKeys.Proposal.ForcemeterShowCounter, value);
        }

        #endregion



        #region Ua API

        public void ClearUaMinLevelData() =>
            CustomPlayerPrefs.DeleteKey(PrefsKeys.AbTest.UaForcemeterMinLevel);

        #endregion
    }
}
