using System;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Utils;
using GameFlow.Proposal.League;
using UnityEngine;
using Random = UnityEngine.Random;


// ReSharper disable All
namespace Drawmasters
{
    public class BotController : BaseDataSaveHolder<BotController.Data>
    {
        #region Helpers

        [Serializable]
        public class Data : BaseDataSaveHolderData
        {
            public float receivedScorePerTime = default;
            public float receivedScoreFullTime = default;
            public DateTime lastReceivedScoreDay = default;
            public bool useOldMechanic = default;
        }

        #endregion
        
        
        
        #region Fields

        private LeagueProposeController controller;
        private List<LeaderBoardItem> bots;
        private int playerSkullsInSession;

        private LoopedInvokeTimer updateBotSkullsTimer;
        private readonly ITimeValidator timeValidator;
        private int currentBotUpdate;
        
        #endregion


        
        #region Properties
        
        public float ReceivedScorePerTime
        {
            get => data.receivedScorePerTime;
            private set
            {
                data.receivedScorePerTime = value;
                SaveData();
            }
        }

        
        public float ReceivedScoreFullTime
        {
            get => data.receivedScoreFullTime;
            private set
            {
                data.receivedScoreFullTime = value;
                SaveData();
            }
        }

        
        public DateTime LastReceivedScoreDay
        {
            get => data.lastReceivedScoreDay;
            private set
            {
                data.lastReceivedScoreDay = value;
                SaveData();
            }
        }

        
        public bool UseOldMechanic
        {
            get => data.useOldMechanic;
            set
            {
                data.useOldMechanic = value;
                SaveData();
            }
        }

        
        protected override string SaveKey =>
            PrefsKeys.Proposal.League.LeagueBotsLiveOpsData;
        
        
        public BotSettings Settings { get; }

        
        public LeagueLeaderBoard LeaderBoard { get; }

        
        public List<LeaderBoardItem> Bots
        {
            get
            {
                if (bots == null || bots.Count == 0)
                {
                    bots = new List<LeaderBoardItem>();

                    string[] jsonArray =
                        CustomPlayerPrefs.GetObjectValue<string[]>(PrefsKeys.Proposal.League.LeagueBotsKey,
                            new string[] { });

                    foreach (var i in jsonArray)
                    {
                        if (string.IsNullOrEmpty(i))
                        {
                            CustomDebug.Log("Empty or NULL value");
                        }
                        else
                        {
                            BotLiveOpsLeagueData data = BotLiveOpsLeagueData.FromJson<BotLiveOpsLeagueData>(i);
        
                            if (data == null)
                            {
                                CustomDebug.Log("Deseralized object NULL");
                            }
                            else
                            {
                                data.controller = controller;
                                data.ReceivedScorePerTime = ReceivedScorePerTime;
                                data.BlokSkullCount = Random.Range(Settings.blockSkullMin, Settings.blockSkullMax);
                                data.UnblokSkullCount = Settings.unblockBotSkull;
                                data.OfflineSkullsMin = Settings.offlineSkullMin;
                                data.OfflineSkullsMax = Settings.offlineBotSkullMax;
                                data.TimeValidator = timeValidator;
                                bots.Add(data);
                            }
                        }
                    }
                }

                return bots;
            }

            private set
            {
                bots = value;
                
                SaveBotsData();
            }
        }
        
        #endregion

        

        #region Class lifecycle

        public BotController(LeagueProposeController _controller, BotSettings _settings, LeagueLeaderBoard leaderBoard, 
            ITimeValidator _timeValidator)
        {
            controller = _controller;
            Settings = _settings;
            LeaderBoard = leaderBoard;
            timeValidator = _timeValidator;
        }
        
        #endregion

        

        #region Public methods

        public void Initialize()
        {
            if (ReceivedScorePerTime <= 0)
            {
                ReceivedScorePerTime = Settings.defaultPlayerTimeFactor;
            }

            if (Bots != null && Bots.Count > 0)
            {
                LoadBots();
                if (UseOldMechanic)
                {
                    UpdateBotsSkullsOldMechanic(true);
                }
                else
                {
                    UpdateBotsSkulls(true);
                }
            }

            currentBotUpdate = 0;
            
            updateBotSkullsTimer = updateBotSkullsTimer ?? new LoopedInvokeTimer(RefreshTimeLeft, Settings.botsRefreshPeriod);
            updateBotSkullsTimer.Start();
        }
        
        
        public void Deinitialize()
        {
            updateBotSkullsTimer.Stop();
        }
        
        
        public void UpdateBotsSkullsOldMechanic(bool all, float skullsMultiplier = 1.0f)
        {
            if (all)
            {
                foreach (LeaderBoardItem item in Bots)
                {
                    if ((item is BotLiveOpsLeagueData bot) && (DateTime.FromBinary(bot.lastWorkDay).Date != timeValidator.ValidNow.Date))
                    {
                        bot.UpdateSkullsOldMechanic(ReceivedScoreFullTime - bot.receivedScoreTimePerLastDay, skullsMultiplier);
                        bot.receivedScoreTimePerLastDay = 0;
                    }
                }
            }
            else
            {
                IEnumerable<LeaderBoardItem> activeItems = Bots.Where(x => x.IsActive);
                foreach (LeaderBoardItem activeItem in activeItems)
                {
                    if (activeItem is BotLiveOpsLeagueData bot)
                    {
                        bot.UpdateSkullsOldMechanic(Time.time - ReceivedScoreFullTime, skullsMultiplier);    
                    }
                };    
            }
            
            SaveBotsData();
        }

        
        private void UpdateBotsSkulls(bool all, float skullsMultiplier = 1.0f)
        {
            if (all)
            {
                List<LeaderBoardItem> list =
                    RandomExtentions.GetRandomItems<LeaderBoardItem>(Bots.Where(x => !x.IsBloked).ToList(), 
                        Settings.countBotsPerSession);
                
                foreach (BotLiveOpsLeagueData bot in list)
                {
                    bot.UpdateOfflineSkulls(skullsMultiplier, LeaderBoard.PlayerData.SkullsCount);
                }

                foreach (BotLiveOpsLeagueData bot in Bots)
                {
                    bot.lastWorkDay = timeValidator.ValidNow.ToBinary();
                }
            }
            else
            {
                List<LeaderBoardItem> activeBots = Bots.Where(x => x.IsActive).ToList();

                activeBots.ForEach(x =>
                {
                    UpdateSkulls(x);
                });    
            }
            SaveBotsData();


            void UpdateSkulls(LeaderBoardItem item)
            {
                if (item is BotLiveOpsLeagueData bot)
                {
                    bot.UpdateOnlineSkulls(skullsMultiplier, LeaderBoard.PlayerData.SkullsCount);    
                }
            }
        }
        
        private void UpdateOneBotSkulls(float skullsMultiplier = 1.0f)
        {
            List<LeaderBoardItem> activeBots = Bots.Where(x => x.IsActive).ToList();

            if (activeBots.Count == 0)
            {
                return;
            }
            
            if (currentBotUpdate >= activeBots.Count)
            {
                currentBotUpdate = 0;
            }
            
            if (activeBots[currentBotUpdate] is BotLiveOpsLeagueData bot)
            {
                bot.UpdateOnlineSkulls(skullsMultiplier, LeaderBoard.PlayerData.SkullsCount);    
            }

            currentBotUpdate++; 
            
            SaveBotsData();
        }
        
        
        private void SaveBotsData()
        {
            List<string> jsonList = new List<string>();
            foreach (var i in Bots)
            {
                if (i == null)
                {
                    CustomDebug.Log("Try serialize NULL value");
                }
                else
                {
                    string json = JsonUtility.ToJson(i);
        
                    if (string.IsNullOrEmpty(json))
                    {
                        CustomDebug.Log("Json is NULL or empty");
                    }
                    else
                    {
                        jsonList.Add(json);
                    }
                }
            }
        
            CustomPlayerPrefs.SetObjectValue(PrefsKeys.Proposal.League.LeagueBotsKey, jsonList.ToArray());
        }
        
        
        public void AddSkulls(float skullsMultiplier = 1.0f)
        {
            if (UseOldMechanic)
            {
                UpdateBotsSkullsOldMechanic(false, skullsMultiplier);
            }
            else
            {
                UpdateBotsSkulls(false, skullsMultiplier);    
            }
        }

        
        public void RecalculatePlayerFactor(int playerEarnedSkulls)
        {
            LastReceivedScoreDay = timeValidator.ValidNow;
            ReceivedScoreFullTime = Time.time;
            playerSkullsInSession += playerEarnedSkulls;
            ReceivedScorePerTime = playerSkullsInSession /
                                   (ReceivedScoreFullTime / IngameData.Settings.league.botSettings.сycleTimeSec);
        }
        
        
        public void GeneratedBots(LeagueType leagueType, float playerSkullCount)
        {
            List<string> usedNames = Bots.Select(x => x.NickName).ToList();

            BotSettings.BotData[] botsData = Settings.GetBots(LeaderBoard.LeagueType);
            List<LeaderBoardItem> bots = new List<LeaderBoardItem>(botsData.Length);
            for (int i = 0; i < botsData.Length; i++)
            {
                string botName = Settings.RandomNickname(usedNames);

                bots.Add(new BotLiveOpsLeagueData(botsData[i].id, botName , 
                    Settings.RandomBotSkinType(LeaderBoard.LeagueType), botsData[i].type, controller, timeValidator)
                {
                    SkullsCount = playerSkullCount,
                    lastWorkDay = timeValidator.ValidNow.ToBinary()
                });

                usedNames.Add(botName);
            }

            Bots = bots;

            LoadBots();
        }
        

        public void LoadBots()
        {
            ReceivedScoreFullTime = 0;
            LastReceivedScoreDay = timeValidator.ValidNow;

            float playerTimeFactor = ReceivedScorePerTime;
            Bots.ForEach(x =>
            {
                x.IsActive = false;

                if (x is BotLiveOpsLeagueData bot)
                {
                    bot.ReceivedScorePerTime = playerTimeFactor;
                    bot.BlokSkullCount = Random.Range(Settings.blockSkullMin, Settings.blockSkullMax);
                    bot.UnblokSkullCount = Settings.unblockBotSkull;
                    bot.OfflineSkullsMin = Settings.offlineSkullMin;
                    bot.OfflineSkullsMax = Settings.offlineBotSkullMax;
                    bot.TimeValidator = timeValidator;
                }
            });

            LeaderBoard.UpdateLeaderList();
        }

                
        public void Reset()
        {
            ReceivedScorePerTime = 0;
            ReceivedScoreFullTime = 0;
            LastReceivedScoreDay = DateTime.MinValue;
            UseOldMechanic = false;
        }


        private void RefreshTimeLeft()
        {
            if (UseOldMechanic)
            {
                return;
            }
            
            HappyHoursLeagueProposeController happyHoursController = 
                GameServices.Instance.ProposalService.HappyHoursLeagueProposeController;

            ILevelEnvironment levelEnvironment = GameServices.Instance.LevelEnvironment;
            bool wasLiveOpsEventActivedBeforeLevel = happyHoursController.WasActiveBeforeLevelStart;
            
            UpdateOneBotSkulls(wasLiveOpsEventActivedBeforeLevel ? happyHoursController.BotsSkullsMultiplier : 1.0f);
            controller.LeaderBoard.UpdateLeaderList();
        }
        
        #endregion
    }
}