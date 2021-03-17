using System;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Levels;
using Drawmasters.Levels.Data;
using Drawmasters.Ui;
using Drawmasters.Proposal;


namespace Drawmasters.ServiceUtil
{
    public class ProposalsLoader
    {
        #region Fields

        private readonly IProposalService proposalService;

        #endregion



        #region Ctor

        public ProposalsLoader(IProposalService _proposalService)
        {
            proposalService = _proposalService;
        }

        #endregion



        #region IProposalService

        public void PremiumShopInstantShow(GameMode gameMode)
        {
            LoadScene(gameMode, GameMode.PremiumShopScene);

            LevelProgressObserver.TriggerPremiumShopShown();
            LevelProgress progress = GameServices.Instance.LevelEnvironment.Progress;
            LevelContext context = GameServices.Instance.LevelEnvironment.Context;

            proposalService.PremiumShopResultController.SetupAvailableWeaponType(context.WeaponType);
            proposalService.PremiumShopResultController.InstantPropose(OnProposed);
        }


        public void ShopResultInstantShow(GameMode gameMode)
        {
            LoadScene(gameMode, GameMode.ShopScene);

            LevelProgressObserver.TriggerShopShown();
            LevelProgress progress = GameServices.Instance.LevelEnvironment.Progress;
            LevelContext context = GameServices.Instance.LevelEnvironment.Context;

            proposalService.ShopResultController.SetupAvailableWeaponType(context.WeaponType);
            proposalService.ShopResultController.InstantPropose(OnProposed);
        }


        public void RouletteInstantShow(GameMode gameMode)
        {
            LoadScene(gameMode, GameMode.RouletteScene);

            LevelProgressObserver.TriggerRouletteShown();
            LevelContext context = GameServices.Instance.LevelEnvironment.Context;

            proposalService.RouletteRewardController.SetupAvailableWeaponType(context.WeaponType);
            proposalService.RouletteRewardController.InstantPropose(OnProposed);
        }

        public void SpinRouletteRewardDataShow(GameMode gameMode, 
            SpinRouletteSettings sequenceRewardPackSettings, Action callback = default)
        {
            FromLevelToLevel.PlayTransition(() =>
            {
                UiScreenManager.Instance.HideAll(true);

                LoadScene(gameMode, GameMode.PremiumRouletteScene);

                LevelContext context = GameServices.Instance.LevelEnvironment.Context;
                proposalService.SpinRouletteController.Settings.SetupWeaponType(context.WeaponType);

                Action callbackToSet = callback ?? ReturnToMenuScene;
                proposalService.SpinRouletteController.InstantPropose(null, sequenceRewardPackSettings, -1, callbackToSet);
            });
        }
        
        
        public void ForcemeterInstantShow(GameMode gameMode)
        {
            LoadScene(gameMode, GameMode.ForcemeterScene);

            LevelProgressObserver.TriggerForcemeterShown();
            LevelContext context = GameServices.Instance.LevelEnvironment.Context;

            proposalService.ForceMeterController.Settings.SetupWeaponType(context.WeaponType);
            proposalService.ForceMeterController.InstantPropose(OnProposed);
        }


        public void ForcemeterRewardDataShow(GameMode gameMode, SequenceRewardPackSettings sequenceRewardPackSettings, Action callback = default)
        {
            FromLevelToLevel.PlayTransition(() =>
            {
                UiScreenManager.Instance.HideAll(true);

                LoadScene(gameMode, GameMode.PremiumForcemeterScene);

                LevelContext context = GameServices.Instance.LevelEnvironment.Context;
                proposalService.ForceMeterController.Settings.SetupWeaponType(context.WeaponType);

                Action actualCallback = callback ?? ReturnToMenuScene;
                proposalService.ForceMeterController.InstantPropose(null, sequenceRewardPackSettings, -1, actualCallback);
            });
        }

        public void PremiumShopShow(GameMode gameMode, Action onFinished = null)
        {
            onFinished?.Invoke();

            LoadScene(gameMode, GameMode.PremiumShopScene);

            LevelProgressObserver.TriggerPremiumShopShown();

            LevelProgress progress = GameServices.Instance.LevelEnvironment.Progress;
            LevelContext context = GameServices.Instance.LevelEnvironment.Context;

            proposalService.PremiumShopResultController.SetupAvailableWeaponType(context.WeaponType);
            proposalService.PremiumShopResultController.Propose(gameMode, OnProposed);
        }


        public void ShopResultShow(GameMode gameMode, Action onFinished = null)
        {
            onFinished?.Invoke();

            LoadScene(gameMode, GameMode.ShopScene);

            LevelProgressObserver.TriggerShopShown();

            LevelProgress progress = GameServices.Instance.LevelEnvironment.Progress;
            LevelContext context = GameServices.Instance.LevelEnvironment.Context;

            proposalService.ShopResultController.SetupAvailableWeaponType(context.WeaponType);
            proposalService.ShopResultController.Propose(gameMode, OnProposed);
        }


        public void RouletteShow(GameMode gameMode, Action onFinished = null)
        {
            onFinished?.Invoke();

            LoadScene(gameMode, GameMode.RouletteScene);

            LevelProgressObserver.TriggerRouletteShown();

            LevelProgress progress = GameServices.Instance.LevelEnvironment.Progress;
            LevelContext context = GameServices.Instance.LevelEnvironment.Context;

            proposalService.RouletteRewardController.SetupAvailableWeaponType(context.WeaponType);
            proposalService.RouletteRewardController.Propose(gameMode, OnProposed);
        }


        public void ForceMeterShow(GameMode gameMode, Action onFinished = null)
        {
            onFinished?.Invoke();

            LoadScene(gameMode, GameMode.ForcemeterScene);

            LevelProgressObserver.TriggerForcemeterShown();

            LevelProgress progress = GameServices.Instance.LevelEnvironment.Progress;
            LevelContext context = GameServices.Instance.LevelEnvironment.Context;

            proposalService.ForceMeterController.Settings.SetupWeaponType(context.WeaponType);
            proposalService.ForceMeterController.Propose(gameMode, OnProposed);
        }


        private void LoadScene(GameMode gameMode, GameMode sceneMode)
        {
            LevelsManager.Instance.UnloadLevel();
            LevelsManager.Instance.LoadScene(gameMode, sceneMode);
        }


        private void OnProposed() =>
            LevelsManager.Instance.CompleteLevel(LevelResult.ProposalEnd);

        
        private void ReturnToMenuScene()
        {
            FromLevelToLevel.PlayTransition(() =>
            {
                UiScreenManager.Instance.HideAll();

                LevelsManager.Instance.UnloadLevel();
                LevelsManager.Instance.LoadScene(GameServices.Instance.PlayerStatisticService.PlayerData.LastPlayedMode, GameMode.MenuScene);
                UiScreenManager.Instance.ShowScreen(ScreenType.MainMenu, isForceHideIfExist: true);
            });
        }
        
        #endregion
    }
}

