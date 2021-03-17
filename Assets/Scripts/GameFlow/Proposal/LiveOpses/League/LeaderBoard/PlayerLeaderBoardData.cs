using Drawmasters.Proposal;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Statistics.Data;


namespace Drawmasters
{
    public class PlayerLeaderBoardData: LeaderBoardItem
    {
        #region Fields
        
        private IPlayerStatisticService playerStatisticService;

        #endregion



        #region Properties

        public bool WasPlayerElementAnimatedOnStart
        {
            get => PlayerData.WasPlayerElementAnimatedOnStart;
            set => PlayerData.WasPlayerElementAnimatedOnStart = value;
        }


        public override LeaderBordItemType ItemType => 
            LeaderBordItemType.Player;
        
        
        public LeagueType LeagueType
        {
            get => PlayerData.LeagueType;
            set => PlayerData.LeagueType = value;
        }
        
        
        public bool ShouldShowPreviewScreen
        {
            get => PlayerData.ShouldShowPreviewScreen;
            set => PlayerData.ShouldShowPreviewScreen = value;
        }
        
        
        public bool ShouldShowLeagueApplyScreen
        {
            get => PlayerData.ShouldShowLeagueApplyScreen;
            set => PlayerData.ShouldShowLeagueApplyScreen = value;
        }

        
        public int PreviousLeaderBoardPosition
        {
            get => PlayerData.PreviousLeaderBoardPosition;
            set => PlayerData.PreviousLeaderBoardPosition = value;
        }


        public int PreviousLeaderBoardAnimationPosition
        {
            get => PlayerData.PreviousLeaderBoardAnimationPosition;
            set => PlayerData.PreviousLeaderBoardAnimationPosition = value;
        }


        public float SkullsCountOnPreviousLeaderboardShow
        {
            get => PlayerData.SkullsCountOnPreviousLeaderboardShow;
            set => PlayerData.SkullsCountOnPreviousLeaderboardShow = value;
        }


        public float OldUsersSkullsCount
        {
            get => PlayerData.OldUsersSkullsCount;
            set => PlayerData.OldUsersSkullsCount = value;
        }


        public override string NickName
        {
            get => PlayerData.NickName;
            set => PlayerData.NickName = value?.Trim();
        }


        public override ShooterSkinType SkinType => 
            playerStatisticService.PlayerData.CurrentSkin;
        
        
        protected PlayerLiveOpsLeagueData PlayerData => 
            playerStatisticService.PlayerLiveOpsLeagueData;

        
        public override float SkullsCount => 
            playerStatisticService.CurrencyData.GetEarnedCurrency(CurrencyType.Skulls);
        
        #endregion

        
        
        #region Class lifecycle

        public PlayerLeaderBoardData(IPlayerStatisticService _playerStatisticService)
        {
            playerStatisticService = _playerStatisticService;
            
            id = PlayerData.Id;
            nickName = PlayerData.NickName;
            skullsCount = PlayerData.OldUsersSkullsCount;
        }
        
        #endregion
        
        
        
        #region Methods
        
        public override void AddSkulls(float skulls)
        {
        }
        
        
        public override void Reset()
        {
            base.Reset();

            playerStatisticService.CurrencyData.TryRemoveCurrency(CurrencyType.Skulls, 
                playerStatisticService.CurrencyData.GetEarnedCurrency(CurrencyType.Skulls));

            PlayerData.Reset();
        }
        
        #endregion
    }
}

