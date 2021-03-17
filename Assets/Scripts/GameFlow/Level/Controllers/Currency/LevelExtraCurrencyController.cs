using System.Collections.Generic;
using Drawmasters.Levels.Inerfaces;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Statistics.Data;


namespace Drawmasters.Levels
{
    /// <summary>
    ///  This class check any currency received from level except collected to show and bonus 'em on result screen.</para>
    /// </summary>
    public class LevelExtraCurrencyController : ILevelController, IInitialStateReturn
    {
        #region Fields

        private readonly List<(CurrencyType, float)> initialCurrencyValues = new List<(CurrencyType, float)>();

        private readonly List<(CurrencyType, float)> earnedCurrencyValues = new List<(CurrencyType, float)>();

        private readonly IPlayerStatisticService playerStatisticService;

        #endregion



        #region Ctor

        public LevelExtraCurrencyController(IPlayerStatisticService _playerStatisticService)
        {
            playerStatisticService = _playerStatisticService;
        }

        #endregion



        #region Methods

        public void Initialize()
        {
            ResetCurrencyValues();

            playerStatisticService.CurrencyData.OnCurrencyAdded += PlayerCurrencyInfo_OnCurrencyCountAdded;
        }


        public void Deinitialize()
        {
            playerStatisticService.CurrencyData.OnCurrencyAdded -= PlayerCurrencyInfo_OnCurrencyCountAdded;
        }


        private void ResetCurrencyValues()
        {
            initialCurrencyValues.Clear();
            earnedCurrencyValues.Clear();

            foreach (var type in PlayerCurrencyData.PlayerTypes)
            {
                float totalCurrency = playerStatisticService.CurrencyData.GetEarnedCurrency(type);
                initialCurrencyValues.Add((type, totalCurrency));
                earnedCurrencyValues.Add((type, default));

                LevelProgressObserver.TriggerSetAdditionalEarnedCurrency(type, default);
            }
        }

        #endregion



        #region Events handlers

        private void PlayerCurrencyInfo_OnCurrencyCountAdded(CurrencyType type, float value)
        {
            var element = initialCurrencyValues.Find(e => e.Item1 == type);

            float currentCurrency = playerStatisticService.CurrencyData.GetEarnedCurrency(type);
            
            int earnedElementIndex = earnedCurrencyValues.FindIndex(e => e.Item1 == type);

            if (earnedElementIndex == -1)
            {
                CustomDebug.Log($"No data in {this} for {type}");
                return;
            }

            var earnedElement = earnedCurrencyValues[earnedElementIndex];
            earnedElement.Item2 += value;
            earnedCurrencyValues[earnedElementIndex] = earnedElement;
            
            if (value >= 0.0f)
            {
                LevelProgressObserver.TriggerSetAdditionalEarnedCurrency(type, earnedElement.Item2);
            }
        }


        public void ReturnToInitialState() =>
            ResetCurrencyValues();

        #endregion
    }
}
