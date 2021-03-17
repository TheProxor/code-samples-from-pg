using System;
using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Statistics.Data;
using Drawmasters.Ui;
using Drawmasters.Ui.Mansion;
using Drawmasters.Proposal;
using Modules.Advertising;
using Modules.General;
using Modules.Sound;
using Modules.General.Abstraction;
using Drawmasters.Ui.Hitmasters;
using Drawmasters.Mansion;


namespace Drawmasters.Levels
{
    public abstract class SucceedLevelFinisher : ILevelFinisher
    {
        #region Fields

        protected Action onFinished;
        protected ILevelEnvironment levelEnvironment;
        protected IPlayerStatisticService playerStatistic;
        protected IProposalService proposalService;

        #endregion



        #region Properties

        private bool ShouldReturnToMansion(out int indexToScroll, 
                                           out CurrencyType currencyType, 
                                           out MansionRoomObjectType mansionRoomObjectType)
        {
            bool result = false;
            indexToScroll = default;
            currencyType = CurrencyType.None;
            mansionRoomObjectType = MansionRoomObjectType.None;

            MansionProposeController mansionPropose = GameServices.Instance.ProposalService.MansionProposeController;

            bool allowReturn = mansionPropose.IsMechanicAvailable &&
                               mansionPropose.IsEnoughLevelsFinished &&
                               !mansionPropose.CanForcePropose;

            if (!allowReturn)
            {
                return false;
            }

            PlayerMansionData mansionData = GameServices.Instance.PlayerStatisticService.PlayerMansionData;
            PlayerCurrencyData currencyData = GameServices.Instance.PlayerStatisticService.CurrencyData;

            for (int i = 0; i < PlayerMansionData.MansionRoomsCount; i++)
            {
                foreach (var type in mansionData.FindMansionRoomData(i).GetAvailableObjectsTypes())
                {
                    int currentUpdates = mansionData.FindMansionRoomObjectUpgrades(i, type);
                    int neededUpdates = IngameData.Settings.mansionRewardPackSettings.FindObjectTotalUpgrades(type);

                    int hammersToCompleteUpgrade = neededUpdates - currentUpdates;

                    bool canUpgrade = mansionData.WasRoomOpened(i) &&
                                     !mansionData.WasRoomCompleted(i) &&
                                     (neededUpdates - currentUpdates) > 0 &&
                                     !mansionData.FindMansionRoomData(i).WasObjectUpgradeHardEntered(type) &&
                                      currencyData.GetEarnedCurrency(CurrencyType.MansionHammers) >= hammersToCompleteUpgrade;

                    if (canUpgrade)
                    {
                        result = true;
                        mansionRoomObjectType = type;
                        currencyType = CurrencyType.MansionHammers;
                        break;
                    }
                }

                if (result)
                {
                    indexToScroll = i;

                    break;
                }
            }

            return result;
        }


        private bool ShouldReturnToMenu
        {
            get
            {
                bool result = false;

                LevelProgress progress = levelEnvironment.Progress;
                LevelContext context = levelEnvironment.Context;

                if (progress.WasAnotherModeTransition)
                {
                    return true;
                }

                MenuReturnType currentReturnType = GameServices.Instance.AbTestService.CommonData.menuReturnType;

                if (context.IsEndOfLevel || progress.LevelResultState.IsLevelOrProposalAccomplished())
                {
                    result |= (currentReturnType == MenuReturnType.ReturnAfterEveryLevel);
                }

                bool isBossLevel = levelEnvironment.Context.IsBossLevel;

                if (isBossLevel)
                {
                    result |= (currentReturnType == MenuReturnType.ReturnAfterBoss);
                }

                bool isFromProposalScene = context.SceneMode.IsProposalSceneMode();

                if (isFromProposalScene)
                {
                    result |= (currentReturnType == MenuReturnType.ReturnAfterIngameProposal);
                }

                return result;
            }
        }


        private bool ShouldReturnToMap =>
            levelEnvironment.Context?.Mode.IsHitmastersLiveOps() ?? false;


        private bool ShouldReturnToLeagueLeaderBoardScreen
        {
            get
            {
                LeagueProposeController controller = GameServices.Instance.ProposalService.LeagueProposeController;

                if(!controller.IsMechanicAvailable || !controller.IsActive)
                {
                    return false;
                }

                return controller.LeaderBoard.CanPropose;
            }
        }


        private bool ShouldReturnToMenuForPropose
        {
            get
            {
                LeagueProposeController controller = GameServices.Instance.ProposalService.LeagueProposeController;

                bool result = controller.IsMechanicAvailable &&
                              controller.SkullsCountCollectOnLastLevel > 0;

                return result;
            }
        }


        private bool ShouldReturnToSeasonEventScreen
        {
            get
            {
                bool result = false;

                LevelProgress progress = levelEnvironment.Progress;
                LevelContext context = levelEnvironment.Context;

                SeasonEventProposeController controller = GameServices.Instance.ProposalService.SeasonEventProposeController;

                if (!controller.IsMechanicAvailable ||
                    !controller.IsActive ||
                    progress.WasAnotherModeTransition)
                {
                    return false;
                }

                SeasonEventReturnType currentReturnType = GameServices.Instance.AbTestService.CommonData.seasonEventAbSettings.ReturnType;

                if (context.IsEndOfLevel || progress.LevelResultState.IsLevelOrProposalAccomplished())
                {
                    result |= currentReturnType == SeasonEventReturnType.ReturnAfterEveryLevel;
                }

                bool isProposal = context.SceneMode.IsProposalSceneMode();

                float totalCurrencyPerLevel = progress.TotalCurrencyPerLevelEnd(CurrencyType.SeasonEventPoints);

                IPlayerStatisticService playerStatisticService = GameServices.Instance.PlayerStatisticService;
                float currencyCountBeforeLevel = playerStatisticService.CurrencyData.GetEarnedCurrency(CurrencyType.SeasonEventPoints);
                // Because on proposal we've already have earned currency in player prefs
                currencyCountBeforeLevel = isProposal ? currencyCountBeforeLevel - totalCurrencyPerLevel : currencyCountBeforeLevel;

                float currencyCountAfterLevel = currencyCountBeforeLevel + totalCurrencyPerLevel;

                int levelReachIndexAfterLevel = controller.LevelReachIndex(currencyCountAfterLevel);
                bool canClaimAnyReward = controller.CanClaimAnyReward(levelReachIndexAfterLevel, SeasonEventRewardType.Simple, SeasonEventRewardType.Bonus);
                if (canClaimAnyReward)
                {
                    result |= currentReturnType == SeasonEventReturnType.ReturnIfCanClaimReward;
                }

                bool isNextLevelReached = controller.IsNextLevelReached(currencyCountBeforeLevel, currencyCountAfterLevel);
                if (isNextLevelReached)
                {
                    result |= currentReturnType == SeasonEventReturnType.ReturnWhenRewardReached;
                }

                return result;
            }
        }

        #endregion



        #region Ctor

        public SucceedLevelFinisher()
        {
            levelEnvironment = GameServices.Instance.LevelEnvironment;
            playerStatistic = GameServices.Instance.PlayerStatisticService;
            proposalService = GameServices.Instance.ProposalService;
        }

        #endregion



        #region ILevelFinisher

        public virtual void FinishLevel(Action _onFinished)
        {
            onFinished = _onFinished;
        }

        #endregion



        #region Private methods
        
        protected void OnScreenHided()
        {
            bool shouldReturnToLiveOpsMap = ShouldReturnToMap && GameServices.Instance.ProposalService.HitmastersProposeController.IsActive;
            bool shouldReturnToMenu = ShouldReturnToMenu;
            bool shouldReturnToMansion = ShouldReturnToMansion(out int indexToScroll, out CurrencyType currencyType, out MansionRoomObjectType mansionRoomObjectType);
            bool shouldReturnToSeasonEventScreen = ShouldReturnToSeasonEventScreen;
            bool shouldReturnToLeagueLeaderBoardScreen = ShouldReturnToLeagueLeaderBoardScreen;
            bool shouldReturnToMenuForPropose = ShouldReturnToMenuForPropose;

            SeasonEventProposeController seasonEventProposeController = GameServices.Instance.ProposalService.SeasonEventProposeController;
            if (seasonEventProposeController.IsActive)
            {
                bool canClaimAnyReward = seasonEventProposeController.CanClaimAnyReward(seasonEventProposeController.LevelReachIndex(), SeasonEventRewardType.Simple);
                seasonEventProposeController.ShouldShowLevelFinishAlert = canClaimAnyReward;
            }

            onFinished?.Invoke();

            LeagueProposeController leagueProposeController = GameServices.Instance.ProposalService.LeagueProposeController;

            // todo refactor. It's better to separate hitmasters live ops logic from common
            Action proposeCallback = null;

            if (shouldReturnToLeagueLeaderBoardScreen)
            {
                proposeCallback = () => LoadLeaderboard(shouldReturnToLiveOpsMap ? LoadHitmastersMapScreen : (Action)null);
            }
            else if (shouldReturnToSeasonEventScreen)
            {
                proposeCallback = () => LoadSeasonEvent(shouldReturnToLiveOpsMap ? LoadHitmastersMapScreen : (Action)LoadMenu);
            }
            else if (shouldReturnToMansion)
            {
                proposeCallback = () => LoadMansion();
            }

            bool shouldProposeAnyLiveOps = proposeCallback != null;

            if (shouldProposeAnyLiveOps)
            {
                if (shouldReturnToMenuForPropose)
                { 
                    leagueProposeController.AddProposeListener(() => ProposeLiveOps());

                    LoadMenu();
                }
                else 
                {
                    proposeCallback.Invoke();
                }
            }        
            else if (shouldReturnToMenu)
            {
                if (shouldReturnToLiveOpsMap)
                {
                    LoadHitmastersMapScreen();
                }
                else
                {
                    LoadMenu();
                }
            }
            else
            {
                LoadNextLevel();
            }


            void LoadMenu()
            {
                GameMode modeToPropose = playerStatistic.PlayerData.LastPlayedMode;

                LevelsManager.Instance.LoadScene(modeToPropose, GameMode.MenuScene);
                UiScreenManager.Instance.ShowScreen(ScreenType.MainMenu);
            }


            void LoadSeasonEvent(Action closeScreenCallback = default)
            {
                UiSeasonEventScreen screen = UiScreenManager.Instance.ShowScreen(ScreenType.SeasonEventScreen, isForceHideIfExist: true) as UiSeasonEventScreen;

                screen.onCloseScreen = () =>
                {
                    closeScreenCallback?.Invoke();
                };

                SoundManager.Instance.PlaySound(AudioKeys.Music.MUSIC_MENU, isLooping: true);
            }


            void LoadLeaderboard(Action closeScreenCallback = default)
            {
                GameServices.Instance.ProposalService.LeagueProposeController.ProposeLeaderBoard(closeScreenCallback);
            }


            void LoadMansion()
            {
                PlayerMansionData mansionData = GameServices.Instance.PlayerStatisticService.PlayerMansionData;
                mansionData.FindMansionRoomData(indexToScroll).MarkObjectUpgradeHardEntered(mansionRoomObjectType);

                UiMansion screen = UiScreenManager.Instance.ShowScreen(ScreenType.Mansion, isForceHideIfExist: true) as UiMansion;
                screen.MarkHardEnter(indexToScroll, currencyType);
            }


            void LoadHitmastersMapScreen()
            {
                int completedLevel = GameServices.Instance.ProposalService.HitmastersProposeController.LiveOpsLevelCounter - 1;

                UiHitmastersMapScreen screen = UiScreenManager.Instance.ShowScreen(ScreenType.HitmastersMap, isForceHideIfExist: true) as UiHitmastersMapScreen;
                screen.PlayLevelCompleteAnimation(completedLevel);
            }


            void LoadNextLevel()
            {
                GameMode mode = playerStatistic.PlayerData.LastPlayedMode;
                int index = playerStatistic.PlayerData.GetModeCurrentIndex(mode);

                LevelsManager.Instance.LoadLevel(mode, index);
                LevelsManager.Instance.PlayLevel();
            }


            void ProposeLiveOps()
            {
                if (proposeCallback != null)
                {
                    FromLevelToLevel.PlayTransition(() =>
                    {
                        UiScreenManager.Instance.HideAll(true);
                        LevelsManager.Instance.UnloadLevel();

                        proposeCallback?.Invoke();

                        leagueProposeController.MarkAsProposed();
                    });
                }
                else
                {
                    leagueProposeController.MarkAsProposed();
                }
            }
        }
       
        #endregion
        
        
        
        #region Protected methods

        protected void ShowResult()
        {
            //HACK
            bool isResultActive = 
                UiScreenManager.Instance.IsScreenActive(ScreenType.Result);
                        
            if (isResultActive)
            {
                CustomDebug.Log("Result screen was twice showed");
                
                return;
            }
            
            AdvertisingManager.Instance.TryShowAdByModule(AdModule.Interstitial, 
                AdPlacementType.BeforeResult, 
                result =>
                    Scheduler.Instance.CallMethodWithDelay(this, () =>
                    {
                        UiScreenManager.Instance.HideAll(true);
                        
                        UiScreenManager.Instance.ShowScreen(ScreenType.Result,
                            onHideBegin: view => OnScreenHided(), isForceHideIfExist : false);
                        
                    }, CommonUtility.OneFrameDelay));
        }

        #endregion
    }
}

