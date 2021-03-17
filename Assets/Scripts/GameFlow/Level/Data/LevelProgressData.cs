using System;
using System.Collections.Generic;
using Drawmasters.Proposal;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Levels.Data
{
    [Serializable]
    public class LevelProgressData
    {
        #region Fields

        public PetSkinType invokedPetSkinType = default;

        public int sublevelKilledEnemiesCount = default;
        public int killedEnemiesCount = default;

        public bool wasBarFillingShown = default;
        public bool wasSkinClaimed = default;
        public bool wasSkinSkipped = default;

        public bool wasAnotherModeTransition = default;

        public bool wasProgressBonusClaimed = default;

        public bool wasCurrencyBonusClaimed = default;
        public int multipliedCurrencyPenalty = default;
        public CurrencyType multipliedCurrencyType = default;

        public bool wasPremiumShopShown = default;

        public bool wasShopShown = default;

        public bool wasRouletteShown = default;

        public bool wasForcemeterShown = default;

        public bool wasBonusLevelProposed = default;

        public List<(CurrencyType type, float value)> levelCollectedCurrency = default;

        // Contains currency already added in PlayerPrefs (from proposals etc.) that can be bonused
        public List<(CurrencyType type, float value)> levelExtraCurrency = default;

        #endregion



        #region Properties

        public bool CanShowPropose
        {
            get
            {
                bool result = true;

                result &= !wasShopShown;
                result &= !wasPremiumShopShown;
                result &= !wasRouletteShown;
                result &= !wasForcemeterShown;
                result &= !wasBonusLevelProposed;

                return result;
            }
        }

        #endregion



        #region Lifecycle

        public LevelProgressData()
        {
            levelCollectedCurrency = levelCollectedCurrency ?? new List<(CurrencyType type, float value)>();
            levelExtraCurrency = levelExtraCurrency ?? new List<(CurrencyType type, float value)>();
        }

        #endregion



        #region Methods

        public float LiveOpsCurrencyPerLevelEnd(CurrencyType type, bool withBonusIsPossible = false, bool forceMultiplied = false)
        {
            float earnedCurrency = default;

            if (GameServices.Instance.LevelEnvironment.Context.IsBossLevel && type == CurrencyType.Premium)
            {
                earnedCurrency = GameServices.Instance.AbTestService.CommonData.premiumCurrencyPerBossLevelComplete;
            }
            else if (!GameServices.Instance.LevelEnvironment.Context.IsBossLevel && type == CurrencyType.Simple)
            {
                earnedCurrency = killedEnemiesCount * IngameData.Settings.hitmasters.settings.currencyForKilledEnemy;
            }
            else if (type == CurrencyType.Skulls)
            { 
                earnedCurrency = GetCollectedCurrency(type);
                
                earnedCurrency = HappyHoursMultipliedValue(earnedCurrency,
                    GameServices.Instance.LevelEnvironment.Context, 
                    GameServices.Instance.ProposalService.HappyHoursLeagueProposeController);
            }
            else if (type == CurrencyType.SeasonEventPoints)
            {
                earnedCurrency = GetCollectedCurrency(type);
                earnedCurrency = HappyHoursMultipliedValue(earnedCurrency,
                    GameServices.Instance.LevelEnvironment.Context, 
                    GameServices.Instance.ProposalService.HappyHoursSeasonEventProposeController);
            }
            else
            {
                return earnedCurrency;
            }
            
            bool canClaimBonus = CanMultiply(type) && withBonusIsPossible;

            if (canClaimBonus || forceMultiplied)
            {
                earnedCurrency *= GameServices.Instance.ProposalService.IngameCurrencyMultiplier.CurrencyMultiplier(multipliedCurrencyPenalty);
            }
            
            return earnedCurrency;
        }


        public float CurrencyPerLevelEnd(LevelResult levelResult, CurrencyType type)
        {
            float earnedCurrency = default;
            
            earnedCurrency += GetCollectedCurrency(type);

            if (CanMultiply(type))
            {
                earnedCurrency *= GameServices.Instance.ProposalService.IngameCurrencyMultiplier.CurrencyMultiplier(multipliedCurrencyPenalty);
            }

            LevelContext context = GameServices.Instance.LevelEnvironment.Context;

            bool needSubExistedCurrency = levelResult == LevelResult.ProposalEnd;
            if (context != null)
            {
                needSubExistedCurrency |= context.IsBonusLevel;

                if (!needSubExistedCurrency)
                {
                    if (type == CurrencyType.Skulls)
                    {
                        earnedCurrency = HappyHoursMultipliedValue(earnedCurrency, context, 
                            GameServices.Instance.ProposalService.HappyHoursLeagueProposeController);
                    }

                    if (type == CurrencyType.SeasonEventPoints)
                    {
                        earnedCurrency = HappyHoursMultipliedValue(earnedCurrency, context,
                            GameServices.Instance.ProposalService.HappyHoursSeasonEventProposeController);
                    }
                }
            }

            if (needSubExistedCurrency)
            {
                earnedCurrency -= GetCollectedCurrency(type); // cuz we already add that before on level end
            }

            return earnedCurrency;
        }


        // returns all earned currency, including that already has been added in PlayerInfo        
        public float TotalCurrencyPerLevelEnd(CurrencyType type)
        {
            float earnedCurrency = default;

            earnedCurrency += GetCollectedCurrency(type);

            if (wasCurrencyBonusClaimed && type == multipliedCurrencyType)
            {
                earnedCurrency *= GameServices.Instance.ProposalService.IngameCurrencyMultiplier.CurrencyMultiplier(multipliedCurrencyPenalty);
            }


            if (type == CurrencyType.Skulls)
            {
                earnedCurrency = HappyHoursMultipliedValue(earnedCurrency, 
                    GameServices.Instance.LevelEnvironment.Context, 
                    GameServices.Instance.ProposalService.HappyHoursLeagueProposeController);
            }

            
            if (type == CurrencyType.SeasonEventPoints)
            {
                earnedCurrency = HappyHoursMultipliedValue(earnedCurrency, 
                    GameServices.Instance.LevelEnvironment.Context,
                    GameServices.Instance.ProposalService.HappyHoursSeasonEventProposeController);
            }

            return earnedCurrency;
        }

        
        private float HappyHoursMultipliedValue(float value, LevelContext context, LiveOpsEventController controller) =>
            controller.WasActiveBeforeLevelStart ? 
                controller.GetPlayerMultipliedValue(value) : 
                value;
        
        
        public float TotalCurrencyPerLevelEndWithoutBonus(CurrencyType type) =>
            GetCollectedCurrency(type);


        public float TotalCurrencyPerLevelEndWithBonus(CurrencyType type, int passedSeconds)
        {
            float earnedCurrency = default;
            multipliedCurrencyPenalty = passedSeconds;
            
            earnedCurrency += GetCollectedCurrency(type);
            earnedCurrency *= GameServices.Instance.ProposalService.IngameCurrencyMultiplier.CurrencyMultiplier(multipliedCurrencyPenalty);

            return earnedCurrency;
        }


        public float GetExtraCurrency(CurrencyType type) =>
            GetCurrencyInfo(ref levelExtraCurrency, type);


        public void SetExtraCurrency(CurrencyType type, float value) =>
            SetCurrencyInfo(ref levelExtraCurrency, type, value);


        public void ResetExtraCurrency() =>
            levelExtraCurrency.Clear();


        public float GetCollectedCurrency(CurrencyType type) =>
            GetCurrencyInfo(ref levelCollectedCurrency, type);


        public void SetCollectedCurrency(CurrencyType type, float value) =>
            SetCurrencyInfo(ref levelCollectedCurrency, type, value);


        public void ResetCollectedCurrency() =>
            levelCollectedCurrency.Clear();


        private float GetCurrencyInfo(ref List<(CurrencyType type, float value)> collection, CurrencyType type)
        {
            (CurrencyType type, float value) data = collection.Find(e => e.type == type);
            return data.value;
        }


        private void SetCurrencyInfo(ref List<(CurrencyType type, float value)> collection, CurrencyType type, float value)
        {
            bool isElementExists = collection.Exists(e => e.type == type);

            if (isElementExists)
            {
                int foundIndex = collection.FindIndex(e => e.type == type);

                var collectedElement = collection[foundIndex];
                collectedElement.value = value;
                collection[foundIndex] = collectedElement;
            }
            else
            {
                collection.Add((type, value));
            }
        }


        private bool CanMultiply(CurrencyType type) =>
            wasCurrencyBonusClaimed && type == multipliedCurrencyType;

        #endregion
    }
}
