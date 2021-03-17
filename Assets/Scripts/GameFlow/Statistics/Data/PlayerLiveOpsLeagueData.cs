using System;
using Drawmasters.Utils;
using Drawmasters.Proposal;


namespace Drawmasters.Statistics.Data
{
    public class PlayerLiveOpsLeagueData : BaseDataSaveHolder<PlayerLiveOpsLeagueData.Data>
    {
        #region Helpers

        [Serializable]
        public class Data : BaseDataSaveHolderData
        {
            public string nickName = default;
            public float skullsCount = default;

            public int intermediateRewardEarned = default;
            public int intermediateRewardStage = default;
            public float intermediateRewardPoints = default;

            public LeagueType leagueType = default;
            public bool shouldShowPreviewScreen = default;
            public bool shouldShowLeagueApplyScreen = default;

            // for ui predominantly
            public float skullsCountOnPreviousLeaderboardShow = default;
            // for ui cuz of separator in content
            public int previousLeaderBoardPosition = default;
            public int previousLeaderBoardAnimationPosition = default;
            public bool wasPlayerElementAnimatedOnStart = default;

            public Data()
            {
                const string DefaultPlayerNickName = "Drawmaster";

                nickName = DefaultPlayerNickName;
            }
        }

        #endregion



        #region Properties

        public string Id =>
            "player";


        public string NickName
        {
            get => data.nickName;
            set
            {
                data.nickName = value;
                SaveData();
            }
        }


        public float OldUsersSkullsCount
        {
            get => data.skullsCount;
            set
            {
                data.skullsCount = value;
                SaveData();
            }
        }


        public int LeagueIntermediateRewardEarned
        {
            get => data.intermediateRewardEarned;
            set
            {
                data.intermediateRewardEarned = value;
                SaveData();
            }
        }


        public int LeagueIntermediateRewardStage
        {
            get => data.intermediateRewardStage;
            set
            {
                data.intermediateRewardStage = value;
                SaveData();
            }
        }


        public float LeagueIntermediateRewardPoints
        {
            get => data.intermediateRewardPoints;
            set
            {
                data.intermediateRewardPoints = value;
                SaveData();
            }
        }


        public LeagueType LeagueType
        {
            get => data.leagueType;
            set
            {
                data.leagueType = value;
                SaveData();
            }
        }


        public bool ShouldShowPreviewScreen
        {
            get => data.shouldShowPreviewScreen;
            set
            {
                data.shouldShowPreviewScreen = value;
                SaveData();
            }
        }


        public bool ShouldShowLeagueApplyScreen
        {
            get => data.shouldShowLeagueApplyScreen;
            set
            {
                data.shouldShowLeagueApplyScreen = value;
                SaveData();
            }
        }


        public float SkullsCountOnPreviousLeaderboardShow
        {
            get => data.skullsCountOnPreviousLeaderboardShow;
            set
            {
                data.skullsCountOnPreviousLeaderboardShow = value;
                SaveData();
            }
        }


        public int PreviousLeaderBoardPosition
        {
            get => data.previousLeaderBoardPosition;
            set
            {
                data.previousLeaderBoardPosition = value;
                SaveData();
            }
        }


        public int PreviousLeaderBoardAnimationPosition
        {
            get => data.previousLeaderBoardAnimationPosition;
            set
            {
                data.previousLeaderBoardAnimationPosition = value;
                SaveData();
            }
        }


        public bool WasPlayerElementAnimatedOnStart
        {
            get => data.wasPlayerElementAnimatedOnStart;
            set
            {
                data.wasPlayerElementAnimatedOnStart = value;
                SaveData();
            }
        }


        protected override string SaveKey =>
            PrefsKeys.Proposal.League.LeaguePlayerLiveOpsData;
        
        #endregion


        
        #region Methods

        public void Reset()
        {
            SkullsCountOnPreviousLeaderboardShow = 0;
            PreviousLeaderBoardPosition = 0;
            PreviousLeaderBoardAnimationPosition = 0;
            WasPlayerElementAnimatedOnStart = false;
        }

        #endregion
    }
}