using Drawmasters.Levels.Inerfaces;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Levels
{
    public class LevelSkullCollectController : ILevelController, IInitialStateReturn
    {
        #region Fields

        private float earned;

        #endregion



        #region Methods

        public void Initialize()
        {
            earned = 0.0f;

            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
            LevelProgressObserver.OnLevelStateChanged += LevelStateObserver_OnLevelStateChanged;
            LevelProgressObserver.OnKillEnemy += LevelStateObserver_OnKillEnemy;
        }


        public void Deinitialize()
        {
            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;
            LevelProgressObserver.OnLevelStateChanged -= LevelStateObserver_OnLevelStateChanged;
            LevelProgressObserver.OnKillEnemy -= LevelStateObserver_OnKillEnemy;
        }


        public void ReturnToInitialState() =>
            RemoveLeveCollectedCurrency();


        private void RemoveLeveCollectedCurrency()
        {
            LevelProgressObserver.TriggerRemoveCollectedCurrency(CurrencyType.Skulls, earned);

            earned = 0.0f;
        }

        #endregion



        #region Events handlers
        
        private void LevelStateObserver_OnKillEnemy(LevelTarget hitTarget)
        {
            earned++;
            if (GameServices.Instance.ProposalService.LeagueProposeController.IsSkullsCollectAvailable)
            {
                LevelProgressObserver.TriggerCurrencyCollect(CurrencyType.Skulls, 1);    
            }
        }
        
        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (state == LevelState.EndPlaying)
            {
                bool isSkipped = GameServices.Instance.LevelEnvironment.Progress.LevelResultState.IsSkipped();
                if (isSkipped)
                {
                    LevelProgressObserver.TriggerRemoveCollectedCurrency(CurrencyType.Skulls, earned);
                    earned = 0f;
                }
            }
        }
        
        private void LevelStateObserver_OnLevelStateChanged(LevelResult result)
        {
            if (result == LevelResult.Reload)
            {
                RemoveLeveCollectedCurrency();
            }
        }

        #endregion
    }
}
