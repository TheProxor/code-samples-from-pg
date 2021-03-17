using System;
using System.Linq;
using Drawmasters.Levels;
using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.Statistics.Data
{
    public class PlayerCurrencyData
    {
        #region Fields

        public static readonly CurrencyType[] PlayerTypes = Enum.GetValues(typeof(CurrencyType))
                                                                .Cast<CurrencyType>()
                                                                .Where(e => e != CurrencyType.None &&
                                                                            e != CurrencyType.MansionKeys_Legacy)
                                                                .ToArray();

        public static readonly CurrencyType[] MansionCurrencyTypes = { CurrencyType.MansionHammers };
        public static readonly CurrencyType[] MonopolyCurrencyTypes = { CurrencyType.RollBones };
        public static readonly CurrencyType[] LeagueCurrencyTypes = { CurrencyType.Skulls };
        
        public event Action<CurrencyType, float> OnCurrencyAdded;
        public event Action OnAnyCurrencyCountChanged;

        private readonly ILevelEnvironment levelEnvironment;

        #endregion



        #region Ctor

        public PlayerCurrencyData(ILevelEnvironment _levelEnvironment)
        {
            levelEnvironment = _levelEnvironment;

            Level.OnLevelStateChanged += Level_OnLevelStateChanged;
        }

        #endregion



        #region Public methods

        public float GetEarnedCurrency(CurrencyType type)
        {
            string prefsKey = GetSavedPrefsKey(type);
            return CustomPlayerPrefs.GetFloat(prefsKey, 0);
        }

        public void AddCurrency(CurrencyType type, float addedCurrencyAmount)
        {
            float currentValue = GetEarnedCurrency(type);
            float valueToSet = currentValue + addedCurrencyAmount;
            
            SetEarnedCurrency(type, valueToSet);

            OnCurrencyAdded?.Invoke(type, addedCurrencyAmount);
        }


        public bool TryRemoveCurrency(CurrencyType type, float value)
        {
            float currentValue = GetEarnedCurrency(type);
            
            bool canRemoveCurrency = currentValue >= value;
            if (canRemoveCurrency)
            {
                float valueToSet = currentValue - value;
                
                SetEarnedCurrency(type, valueToSet);
            }

            return canRemoveCurrency;
        }

        #endregion



        #region Private methods

        private void SetEarnedCurrency(CurrencyType type, float value)
        {
            string prefsKey = GetSavedPrefsKey(type);

            CustomPlayerPrefs.SetFloat(prefsKey, value);
            
            OnAnyCurrencyCountChanged?.Invoke();
        }


        private static string GetSavedPrefsKey(CurrencyType type)
        {
            string result = default;

            switch (type)
            {
                case CurrencyType.None:
                    CustomDebug.LogError($"Trying to find prefs key for {CurrencyType.None} currency type");
                    break;

                case CurrencyType.Simple: // for old users
                    result = PrefsKeys.PlayerInfo.CurrencyCount;
                    break;

                default:
                    result = string.Concat(PrefsKeys.PlayerInfo.CurrencyCountPrefix, type);
                    break;
            }

            return result;
        }

        #endregion



        #region Events handlers

        private void Level_OnLevelStateChanged(LevelState levelState)
        {
            if (levelState == LevelState.Finished)
            {
                LevelProgress progress = levelEnvironment.Progress;
                LevelContext context = levelEnvironment.Context;

                //TODO hotfix
                if (progress.LevelResultState == LevelResult.ProposalEnd &&
                    context.Mode.IsHitmastersLiveOps())
                {
                    return;
                }

                bool isAccomplished = progress.LevelResultState.IsLevelAccomplished() && context.IsEndOfLevel;

                if (isAccomplished || progress.LevelResultState.IsProposalAccomplished())
                {
                    foreach (var type in PlayerTypes)
                    {
                        float currencyEarnedPerLevel = progress.CurrencyPerLevelEnd(type);
                        
                        AddCurrency(type, currencyEarnedPerLevel);
                    }
                }
            }
        }

        #endregion
    }
}
