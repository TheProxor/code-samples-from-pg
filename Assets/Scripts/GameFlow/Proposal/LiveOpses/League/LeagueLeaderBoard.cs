using System;
using System.Collections.Generic;
using System.Linq;
using Drawmasters;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Utils;
using UnityEngine;
using Random = UnityEngine.Random;


namespace GameFlow.Proposal.League
{
    public class LeagueLeaderBoard
    {
        #region Fields

        private readonly LeagueProposeController controller;
        private IPlayerStatisticService playerStatisticService;
        
        private readonly BotController botController;
        
        private readonly List<LeaderBoardItem> items;
        
        private Dictionary<string, float> competorsSortedDictionary;
        
        private readonly PlayerLeaderBoardData playerData;

        public event Action<int, int> OnChangePlayerPosition;

        private int minPlayerPosition;

        #endregion



        #region Properties

        public bool IsChangePlayerSkullCount =>
            !Mathf.Approximately(SkullsCountOnPreviousLeaderboardShow, PlayerData.SkullsCount) &&
            PreviousLeaderBoardPosition > 0;
        
        
        public string[] LidersListId => 
            competorsSortedDictionary.Keys.ToArray();
            
        
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
        
        
        public float SkullsCountOnPreviousLeaderboardShow
        {
            get => PlayerData.SkullsCountOnPreviousLeaderboardShow;
            set => PlayerData.SkullsCountOnPreviousLeaderboardShow = value;
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


        public LeaderBoardItem[] Items => 
            items.ToArray();


        public LeaderBoardItem[] TournamentResult
        {
            get
            {
                List<LeaderBoardItem> result = new List<LeaderBoardItem>();
                string[] botsId = LidersListId;
                
                for (int i = 0; i < 3; i++)
                {
                    string botId = botsId[i];
                    
                    LeaderBoardItem bot = items.Find(b => b.Id.Equals(botId));

                    if (bot != null)
                    {
                        result.Add(bot);
                    }
                }

                if (CurrentPlayerPosition > 2)
                {
                    result.Add(PlayerData);
                }
                
                return result.ToArray();
            }
        }
        
        
        public PlayerLeaderBoardData PlayerData => 
            playerData;

        
        public BotController BotController => 
            botController;
        
        
        public bool IsChangeBoardPosition
        { 
            get
            {
                bool result = controller.IsActive;

                result &= items.Count > 0;
                result &= GetPosition(PlayerData.Id) != PreviousLeaderBoardPosition;
                
                return result;
            } 
        }


        public bool IsOldMechanic
        {
            get => botController.UseOldMechanic;
            private set => botController.UseOldMechanic = value;
        }
        
        
        public int CurrentPlayerPosition => 
            GetPosition(PlayerData.Id);


        public bool CanPropose
        {
            get
            {
                bool isRewardChanged = 
                    controller.RewardController.IsRewardChanged(LeagueType, PreviousLeaderBoardPosition, CurrentPlayerPosition);

                int currentPosition = CurrentPlayerPosition;

                bool isAchivedNewMinPosition = currentPosition < minPlayerPosition;

                bool isUniquePosition = isRewardChanged || IsUniquePositionMove;

                if (isAchivedNewMinPosition && isUniquePosition)
                {
                    minPlayerPosition = currentPosition;

                    return true;
                }

                return false;
            }
        }


        private bool IsUniquePositionMove
        {
            get
            {
                foreach (var uniquePosition in controller.Settings.LeaderBoardUniqueProposePositions)
                {
                    if (PreviousLeaderBoardPosition > uniquePosition && CurrentPlayerPosition <= uniquePosition)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
       
        #endregion

        
        
        #region Class lifecycle

        public LeagueLeaderBoard(LeagueProposeController controller, IPlayerStatisticService _playerStatisticService, 
            ITimeValidator _timeValidator)
        {
            this.controller = controller;
            playerStatisticService = _playerStatisticService;

            competorsSortedDictionary = new Dictionary<string, float>();
            items = new List<LeaderBoardItem>();
            
            playerData = new PlayerLeaderBoardData(playerStatisticService);
            
            botController = new BotController(controller, IngameData.Settings.league.botSettings, this, _timeValidator);
            
            AttemptRestoreOldUsersData();

            botController.Initialize();

            Initialize();
        }

        #endregion

        
        
        #region Methods

        public void StartNewBoard()
        {
            ShouldShowLeagueApplyScreen = controller.ShowsCount == 0;
            ShouldShowPreviewScreen = false;
            LeagueType = controller.LeagueReachController.ReachedLeague;
            playerData.Reset();
            
            botController.GeneratedBots(LeagueType, PlayerData.SkullsCount);
            Initialize();
        }

        
        public bool IsNextLeagueAchived(string id)
        {
            UpdateLeaderList();
            int leaderBoardPosition = GetPosition(id);
            
            bool result = controller.IsMechanicAvailable;
            result &= LeagueType.IsNextLeagueAvailable();
            result &= leaderBoardPosition < controller.Settings.CountPositionForNextLeagueAchived;

            return result;
        }
        
        
        public void UpdateLeaderList()
        {
            competorsSortedDictionary.Clear();

            int oldPosition = CurrentPlayerPosition;
            
            if (items.Count == 0)
            {
                return;
            }
            
            foreach (var bot in items)
            {
                competorsSortedDictionary.Add(bot.Id, bot.SkullsCount);
            }
            
            competorsSortedDictionary = competorsSortedDictionary.OrderByDescending(e => e.Value).
                ToDictionary(k => k.Key, v => v.Value);

            int currentPosition = CurrentPlayerPosition;
            controller.IsCurrentLiveOpsTaskCompleted = currentPosition == 0;

            if (oldPosition != currentPosition)
            {
                OnChangePlayerPosition?.Invoke(oldPosition, currentPosition);
            }
        }


        public int GetPosition(string id)
        {
            if (competorsSortedDictionary == null)
            {
                return int.MaxValue;
            }

            int result = competorsSortedDictionary.Keys.ToList().IndexOf(id);

            return result == -1 ? int.MaxValue : result;
        }


        public void Reset()
        {
            playerData.Reset();
            botController.Reset();
        }

        
        private void Initialize()
        {
            items.Clear();

            if (botController.Bots.Count == 0)
            {
                return;
            }
            items.AddRange(botController.Bots);
            items.Add(playerData);

            UpdateLeaderList();
            SetActiveBots();

            minPlayerPosition = CurrentPlayerPosition;
        }


        private void SetActiveBots()
        {
            if (items.Count == 0)
            {
                return;
            }
            
            int firstGroupCount = Convert.ToInt32(botController.Settings.countBotsPerSession * 0.25f);
            int secondGroupCount = botController.Settings.countBotsPerSession - firstGroupCount;

            List<LeaderBoardItem> tmpList = new List<LeaderBoardItem>(firstGroupCount * 2);
            int playerPosition = GetPosition(PlayerData.Id);

            int topCount = firstGroupCount;
            int bottomCount = firstGroupCount;

            if (playerPosition < topCount)
            {
                topCount = playerPosition;
                bottomCount += firstGroupCount - topCount;
            }
            
            if (playerPosition + bottomCount > competorsSortedDictionary.Count - 1)
            {
                bottomCount = (competorsSortedDictionary.Count - 1) - playerPosition;
                topCount += firstGroupCount - bottomCount;
            }

            string[] botsId = LidersListId;
            
            tmpList.AddRange(GetNearBots(topCount, i => botsId[playerPosition - i]));
            tmpList.AddRange(GetNearBots(bottomCount, i => botsId[playerPosition + i]));
            
            SetActiveRandomBots(tmpList, firstGroupCount);
            SetActiveRandomBots(items, secondGroupCount);
            
            
            List<LeaderBoardItem> GetNearBots(int count, Func<int, string> leaderBotId)
            {
                List<LeaderBoardItem> result = new List<LeaderBoardItem>(count);
               
                for (int i = 1; i <= count; i++)
                {
                    string botId = leaderBotId(i);
                    LeaderBoardItem bot = items.Find(b => b.Id.Equals(botId));

                    if (bot != null)
                    {
                        result.Add(bot);
                    }
                }

                return result;
            }
            
            
            void SetActiveRandomBots(List<LeaderBoardItem> bots, int count)
            {
                int maxIterationCount = count * 5;
                int findCount = 0;

                for (int i = 0; i < maxIterationCount; i++)
                {
                    int index = Random.Range(0, bots.Count - 1);
                    if (bots[index].IsActive || bots[index].ItemType != LeaderBordItemType.Bot)
                    {
                        continue;
                    }
                    bots[index].IsActive = true;
                    findCount++;
                    
                    if (findCount >= count)
                    {
                        break;
                    }
                }
            }
        }

        
        public void AttemptRestoreOldUsersData()
        {
            if (PlayerData.OldUsersSkullsCount <= 0)
            { 
                return;
            }

            botController.UseOldMechanic = true;
            
            playerStatisticService.CurrencyData.TryRemoveCurrency(CurrencyType.Skulls, 
                playerStatisticService.CurrencyData.GetEarnedCurrency(CurrencyType.Skulls));
            
            playerStatisticService.CurrencyData.AddCurrency(CurrencyType.Skulls, PlayerData.OldUsersSkullsCount);

            PlayerData.OldUsersSkullsCount = float.MinValue;
        }
        
        #endregion
    }
}