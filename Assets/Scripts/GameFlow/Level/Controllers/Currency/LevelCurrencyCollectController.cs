using Drawmasters.Levels.Inerfaces;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Proposal;
using System.Collections.Generic;


namespace Drawmasters.Levels
{
    public class LevelCurrencyCollectController : ILevelController, IInitialStateReturn
    {
        #region Fields

        private readonly List<(CurrencyType, float)> earnedCurrencyValues = new List<(CurrencyType, float)>();
        private LevelTargetController targetController;

        #endregion



        #region Methods

        public void Initialize()
        {
            targetController = GameServices.Instance.LevelControllerService.Target;

            earnedCurrencyValues.Clear();


            targetController.OnTargetHitted += TargetController_OnTargetHitted;
            CoinCollectComponent.OnShouldCollectCoin += CoinCollectComponent_OnShouldCollectCoin;
            RewardCollectComponent.OnRewardDropped += RewardCollectComponent_OnRewardDropped;
            Level.OnLevelStateChanged += Level_OnLevelStateChanged;

            LevelProgressObserver.OnLevelStateChanged += LevelStateObserver_OnLevelStateChanged;
        }


        public void Deinitialize()
        {
            targetController.OnTargetHitted -= TargetController_OnTargetHitted;
            CoinCollectComponent.OnShouldCollectCoin -= CoinCollectComponent_OnShouldCollectCoin;
            RewardCollectComponent.OnRewardDropped -= RewardCollectComponent_OnRewardDropped;
            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;

            LevelProgressObserver.OnLevelStateChanged -= LevelStateObserver_OnLevelStateChanged;
        }


        public void ReturnToInitialState()
        {
            RemoveLeveCollectedCurrency();
        }


        private void RemoveLeveCollectedCurrency()
        {
            foreach (var value in earnedCurrencyValues)
            {
                LevelProgressObserver.TriggerRemoveCollectedCurrency(value.Item1, value.Item2);
            }

            earnedCurrencyValues.Clear();
        }

        #endregion



        #region Events handlers

        private void TargetController_OnTargetHitted(LevelTargetType hittedType)
        {
            if (hittedType == LevelTargetType.Enemy)
            {
                float earnedCurrency = IngameData.Settings.currencySettings.killEnemyReward;
                LevelProgressObserver.TriggerCurrencyCollect(CurrencyType.Simple, earnedCurrency);
            }
        }


        private void CoinCollectComponent_OnShouldCollectCoin(CurrencyLevelObject coin, ICoinCollector collector)
        {
#warning hot fix editor wrong currency setup

            if (coin.CurrencyType == CurrencyType.None)
            {
                CustomDebug.Log($"Trying to collect currency type = {coin.CurrencyType} that is not allow. Collecting Simple type");
            }

            CurrencyType typeToAdd = coin.CurrencyType == CurrencyType.None ? CurrencyType.Simple : coin.CurrencyType;
            float countToAdd = coin.CurrencyCount;

            earnedCurrencyValues.Add((typeToAdd, countToAdd));
            LevelProgressObserver.TriggerCurrencyCollect(typeToAdd, countToAdd);
        }


        private void Level_OnLevelStateChanged(LevelState state)
        {
            if (state == LevelState.EndPlaying)
            {
                ILevelEnvironment environment = GameServices.Instance.LevelEnvironment;

                bool isCompleted = environment.Progress.LevelResultState.IsCompleted();

                if (isCompleted)
                {
                    CurrencyType currencyType = environment.Context.IsBossLevel ? CurrencyType.Premium : CurrencyType.Simple;
                    float value = environment.Context.IsBossLevel ?
                        GameServices.Instance.AbTestService.CommonData.premiumCurrencyPerBossLevelComplete : IngameData.Settings.currencySettings.levelCompleteReward;

                    LevelProgressObserver.TriggerCurrencyCollect(currencyType, value);

                    SeasonEventProposeController seasonEventController = GameServices.Instance.ProposalService.SeasonEventProposeController;

                    if (seasonEventController.ShouldCollectPoints &&
                        !environment.Context.IsBonusLevel &&
                        !environment.Context.IsBossLevel &&
                        environment.Context.IsEndOfLevel)
                    {
                        LevelProgressObserver.TriggerCurrencyCollect(CurrencyType.SeasonEventPoints, seasonEventController.PointsPerLevelEnd);
                    }
                }

                bool isSkipped = environment.Progress.LevelResultState.IsSkipped();

                if (isSkipped)
                {
                    foreach (var v in earnedCurrencyValues)
                    {
                        LevelProgressObserver.TriggerRemoveCollectedCurrency(v.Item1, v.Item2);
                    }

                    earnedCurrencyValues.Clear();
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
        
        
        private void RewardCollectComponent_OnRewardDropped(PhysicalLevelObject from, BonusLevelObjectData bonusLevelObjectData)
        {
            if (bonusLevelObjectData.rewardType != RewardType.Currency)
            {
                return;
            }

            CurrencyType type = bonusLevelObjectData.currencyType;
            float amount = bonusLevelObjectData.currencyAmount;

            earnedCurrencyValues.Add((type, amount));
            
            LevelProgressObserver.TriggerCurrencyCollect(type, amount);
        }

        #endregion
    }
}
