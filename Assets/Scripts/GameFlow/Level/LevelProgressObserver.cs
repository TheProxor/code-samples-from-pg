using System;


namespace Drawmasters.Levels
{
    public static class LevelProgressObserver
    {
        #region Fields

        public static event Action<CurrencyType, float> OnRemoveCollectedCurrency;
        public static event Action<CurrencyType, float> OnCollectCurrency;
        public static event Action OnLevelSkip;
        public static event Action OnLevelReload;
        public static event Action<LevelTarget> OnKillEnemy;
        public static event Action<LevelResult> OnLevelStateChanged;
        public static event Action OnBarFillingShown;
        public static event Action OnClaimSkin;
        public static event Action OnSkipSkin;
        public static event Action<CurrencyType> OnClaimCurrencyBonus; // type, multiplier
        public static event Action OnAnotherModeTransition;
        public static event Action OnClaimProgressBonus;

        public static event Action OnShopShown;
        public static event Action OnRouletteShown;
        public static event Action OnPremiumShopShown;
        public static event Action OnForcemeterShown;
        public static event Action<CurrencyType,float> OnSetAdditionalEarnedCurrency;

        public static event Action<PetSkinType> OnPetInvoked;

        #endregion



        #region Public methods

        public static void TriggerRemoveCollectedCurrency(CurrencyType type, float value) => OnRemoveCollectedCurrency?.Invoke(type, value);
        public static void TriggerCurrencyCollect(CurrencyType type, float value) => OnCollectCurrency?.Invoke(type, value);
        public static void TriggerLevelSkip() => OnLevelSkip?.Invoke(); 
        public static void TriggerLevelReload() => OnLevelReload?.Invoke();
        public static void TriggerKillEnemy(LevelTarget target) => OnKillEnemy?.Invoke(target);
        public static void TriggerLevelStateChanged(LevelResult levelResult) => OnLevelStateChanged?.Invoke(levelResult);
        public static void TriggerBarFillingShown() => OnBarFillingShown?.Invoke();
        public static void TriggerClaimSkin() => OnClaimSkin?.Invoke();
        public static void TriggerSkipSkin() => OnSkipSkin?.Invoke();
        public static void TriggerClaimCurrencyBonus(CurrencyType type) => OnClaimCurrencyBonus?.Invoke(type);
        public static void TriggerAnotherModeTransition() => OnAnotherModeTransition?.Invoke();
        public static void TriggerClaimProgressBonus() => OnClaimProgressBonus?.Invoke();
        public static void TriggerShopShown() => OnShopShown?.Invoke();
        public static void TriggerRouletteShown() => OnRouletteShown?.Invoke();
        public static void TriggerPremiumShopShown() => OnPremiumShopShown?.Invoke();
        public static void TriggerForcemeterShown() => OnForcemeterShown?.Invoke();
        public static void TriggerSetAdditionalEarnedCurrency(CurrencyType type,float value) => OnSetAdditionalEarnedCurrency?.Invoke(type,value);

        public static void TriggerPetInvoked(PetSkinType petSkinType) => OnPetInvoked?.Invoke(petSkinType);

        #endregion
    }
}