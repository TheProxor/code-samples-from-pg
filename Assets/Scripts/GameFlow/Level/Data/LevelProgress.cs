using Drawmasters.Helpers;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;

namespace Drawmasters.Levels.Data
{
    public class LevelProgress
    {
        #region Fields

        private const string SaveDataKey = PrefsKeys.PlayerInfo.LevelProgressData;

        private LevelProgressData data;

        #endregion



        #region Properties

        public bool CanShowPropose => data.CanShowPropose;

        public bool WasPetInvoked => data.invokedPetSkinType != PetSkinType.None;

        public PetSkinType InvokedPetSkinType => data.invokedPetSkinType;

        public bool IsLevelSkipped { get; private set; }

        public bool IsLevelReloaded { get; private set; }

        public int SublevelKilledEnemiesCount => data.sublevelKilledEnemiesCount;

        public LevelResult LevelResultState { get; private set; }

        public bool WasBarFillingShown => data.wasBarFillingShown;

        public bool WasSkinClaimed => data.wasSkinClaimed;

        public bool WasSkinSkipped => data.wasSkinSkipped;

        public bool WasAnotherModeTransition => data.wasAnotherModeTransition;

        public bool WasProgressBonusClaimed => data.wasProgressBonusClaimed;

        public bool WasCurrencyBonusClaimed => data.wasCurrencyBonusClaimed;

        public int KilledEnemiesCount => data.killedEnemiesCount;

        #endregion



        #region Progress lifecycle methods

        public void StartNewProgress()
        {
            data = new LevelProgressData();
            
            SaveData();
        }


        public void LoadProgress()
        {
            data = CustomPlayerPrefs.GetObjectValue<LevelProgressData>(SaveDataKey);

            if (data == null)
            {
                data = new LevelProgressData();
                
                SaveData();
            }
        }


        public void SaveData() =>
            CustomPlayerPrefs.SetObjectValue(SaveDataKey, data);

        #endregion



        #region Ui methods

        public string UiTotalCurrencyPerLevelEnd(CurrencyType type) =>
            ToUiView(data.TotalCurrencyPerLevelEnd(type));

        public string UiTotalCurrencyPerLevelEndWithoutBonus(CurrencyType type) =>
            ToUiView(data.TotalCurrencyPerLevelEndWithoutBonus(type));

        public string UiTotalCurrencyPerLevelEndWithBonus(CurrencyType type, int passedSeconds) =>
            ToUiView(data.TotalCurrencyPerLevelEndWithBonus(type, passedSeconds));

        public string ToUiView(float value) =>
            string.Concat("+", value.ToShortFormat());

        #endregion



        #region Liveops Ui methods

        #warning maybe ToUiView? to Yurii.S
        public string UiLiveOpsCurrencyPerLevelEnd(CurrencyType type, bool withBonusIsPossible = false, bool forceMultiplied = false) =>
            $"+{(int)data.LiveOpsCurrencyPerLevelEnd(type, withBonusIsPossible, forceMultiplied)}"; 

        #endregion



        #region Currency values

        public float TotalCurrencyPerLevelEnd(CurrencyType type) =>
            data.TotalCurrencyPerLevelEnd(type);


        public float TotalCurrencyPerLevelEndWithoutBonus(CurrencyType type) =>
            data.TotalCurrencyPerLevelEndWithoutBonus(type);


        public bool ShouldShowCurrencyOnResult(CurrencyType type) =>
            data.TotalCurrencyPerLevelEnd(type) > 0.0f;


        public float CurrencyPerLevelEnd(CurrencyType type)
        {
            GameMode mode = GameServices.Instance.LevelEnvironment.Context.Mode;

            float currency = mode.IsHitmastersLiveOps() ? 
               data.LiveOpsCurrencyPerLevelEnd(type, true) : 
               data.CurrencyPerLevelEnd(LevelResultState, type);

            return currency;
        }

        #endregion



        #region Lifecycle

        public void Subscribe()
        {
            LevelProgressObserver.OnRemoveCollectedCurrency += OnRemoveCollectedCurrency;
            LevelProgressObserver.OnCollectCurrency += LevelStateObserver_OnCollectCurrency;
            LevelProgressObserver.OnLevelSkip += OnLevelSkip;
            LevelProgressObserver.OnLevelReload += OnLevelReload;
            LevelProgressObserver.OnKillEnemy += OnKillEnemy;
            LevelProgressObserver.OnLevelStateChanged += OnLevelStateChanged;
            LevelProgressObserver.OnBarFillingShown += OnBarFillingShown;
            LevelProgressObserver.OnClaimSkin += OnClaimSkin;
            LevelProgressObserver.OnSkipSkin += OnSkipSkin;
            LevelProgressObserver.OnClaimCurrencyBonus += OnClaimCurrencyBonus;
            LevelProgressObserver.OnAnotherModeTransition += OnAnotherModeTransition;
            LevelProgressObserver.OnClaimProgressBonus += OnClaimProgressBonus;
            LevelProgressObserver.OnShopShown += OnShopShown;
            LevelProgressObserver.OnRouletteShown += OnRouletteShown;
            LevelProgressObserver.OnPremiumShopShown += OnPremiumShopShown;
            LevelProgressObserver.OnForcemeterShown += OnForcemeterShown;
            LevelProgressObserver.OnSetAdditionalEarnedCurrency += OnSetAdditionalEarnedCurrency;
            LevelProgressObserver.OnPetInvoked += OnPetInvoked;
        }


        public void Unsubscribe()
        {
            LevelProgressObserver.OnRemoveCollectedCurrency -= OnRemoveCollectedCurrency;
            LevelProgressObserver.OnCollectCurrency -= LevelStateObserver_OnCollectCurrency;
            LevelProgressObserver.OnLevelSkip -= OnLevelSkip;
            LevelProgressObserver.OnLevelReload -= OnLevelReload;
            LevelProgressObserver.OnKillEnemy -= OnKillEnemy;
            LevelProgressObserver.OnLevelStateChanged -= OnLevelStateChanged;
            LevelProgressObserver.OnBarFillingShown -= OnBarFillingShown;
            LevelProgressObserver.OnClaimSkin -= OnClaimSkin;
            LevelProgressObserver.OnSkipSkin -= OnSkipSkin;
            LevelProgressObserver.OnClaimCurrencyBonus -= OnClaimCurrencyBonus;
            LevelProgressObserver.OnAnotherModeTransition -= OnAnotherModeTransition;
            LevelProgressObserver.OnClaimProgressBonus -= OnClaimProgressBonus;
            LevelProgressObserver.OnShopShown -= OnShopShown;
            LevelProgressObserver.OnRouletteShown -= OnRouletteShown;
            LevelProgressObserver.OnPremiumShopShown -= OnPremiumShopShown;
            LevelProgressObserver.OnForcemeterShown -= OnForcemeterShown;
            LevelProgressObserver.OnSetAdditionalEarnedCurrency -= OnSetAdditionalEarnedCurrency;
            LevelProgressObserver.OnPetInvoked -= OnPetInvoked;
        }

        #endregion



        #region Level State Observer

        private void OnPetInvoked(PetSkinType petSkinType) =>
            data.invokedPetSkinType = petSkinType;


        private void OnLevelSkip() =>
            IsLevelSkipped = true;


        private void OnLevelReload() =>
            IsLevelReloaded = true;


        private void OnKillEnemy(LevelTarget hitTarget)
        {
            data.sublevelKilledEnemiesCount++;
            data.killedEnemiesCount++;
        }


        private void OnLevelStateChanged(LevelResult result) =>
            LevelResultState = result;


        private void OnBarFillingShown() =>
            data.wasBarFillingShown = true;


        private void OnClaimSkin() =>
            data.wasSkinClaimed = true;

        private void OnSkipSkin() =>
            data.wasSkinSkipped = true;


        private void OnClaimCurrencyBonus(CurrencyType type)
        {
            data.wasCurrencyBonusClaimed = true;
            data.multipliedCurrencyType = type;
        }


        private void OnAnotherModeTransition() =>
            data.wasAnotherModeTransition = true;


        private void OnClaimProgressBonus() =>
            data.wasProgressBonusClaimed = true;


        private void OnShopShown() =>
            data.wasShopShown = true;


        private void OnRouletteShown() =>
            data.wasRouletteShown = true;


        private void OnPremiumShopShown() =>
            data.wasPremiumShopShown = true;


        private void OnForcemeterShown() =>
            data.wasForcemeterShown = true;


        private void OnSetAdditionalEarnedCurrency(CurrencyType type, float value) =>
            data.SetExtraCurrency(type, value);
        

        private void LevelStateObserver_OnCollectCurrency(CurrencyType currencyType, float value)
        {
            float currentValue = data.GetCollectedCurrency(currencyType);
            data.SetCollectedCurrency(currencyType, currentValue + value);
        }


        private void OnRemoveCollectedCurrency(CurrencyType currencyType, float value)
        {
            float currentValue = data.GetCollectedCurrency(currencyType);
            data.SetCollectedCurrency(currencyType, currentValue - value);
        }

        #endregion
    }
}
