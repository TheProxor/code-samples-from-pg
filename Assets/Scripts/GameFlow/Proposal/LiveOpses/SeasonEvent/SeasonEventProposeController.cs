using System;
using System.Linq;
using System.Collections.Generic;
using Drawmasters.Analytic;
using UnityEngine;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Ui;
using Drawmasters.Utils;
using Drawmasters.Levels;
using Drawmasters.Statistics.Data;
using Modules.General.Abstraction;
using IAbTestService = Drawmasters.ServiceUtil.Interfaces.IAbTestService;


namespace Drawmasters.Proposal
{
    public class SeasonEventProposeController : LiveOpsProposeController
    {
        #region Nested types

        [Serializable]
        private class SaveRewardData
        {
            public bool[] receivedRewards = Array.Empty<bool>();
            public bool wasAnimatedOnce = default;

            public List<int> rewardsToAnimateOpen = new List<int>();
        }

        #endregion



        #region Fields

        public static readonly HashSet<SeasonEventRewardType> UserSeasonEventRewardTypes = new HashSet<SeasonEventRewardType>()
        {
             SeasonEventRewardType.Simple,
             SeasonEventRewardType.Pass,
              SeasonEventRewardType.Main,
              SeasonEventRewardType.Bonus
        };


        public static readonly HashSet<SeasonEventRewardType> UserNotSequenceSeasonEventRewardTypes = new HashSet<SeasonEventRewardType>()
        {
              SeasonEventRewardType.Main,
              SeasonEventRewardType.Bonus
        };

        public event Action<SeasonEventRewardType, int> OnRewardReceived;
        
        private readonly IAbTestService abTestService;
        private readonly IShopService shopService;

        private readonly Dictionary<SeasonEventRewardType, RewardData[]> allGeneratedLiveOpsReward;
        private readonly Dictionary<SeasonEventRewardType, RewardDataSerializationArray> allGeneratedLiveOpsRewardSerialization;

        #endregion



        #region Properties

        public SeasonEventSettings Settings { get; }

        public SeasonEventVisualSettings VisualSettings { get; }

        public AdModule AdModule
        {
            get
            {
                AdModule result;

                if (Enum.TryParse(abTestService.CommonData.seasonEventAbSettings.AdModule, out AdModule parseResult))
                {
                    result = parseResult;
                }
                else
                {
                    CustomDebug.Log($"Wrong parse for {abTestService.CommonData.seasonEventAbSettings.AdModule}. Returned RewardedVideo as default");
                    result = AdModule.RewardedVideo;
                }

                return result;
            }
        }

        public override bool ShouldShowAlert =>
            base.ShouldShowAlert &&
            CanClaimAnyReward(LevelReachIndex()) &&
            ShouldShowLevelFinishAlert;


        public bool IsSeasonPassActive
        {
            get => CustomPlayerPrefs.GetBool(PrefsKeys.Proposal.IsSeasonPassBought, default);
            private set => CustomPlayerPrefs.SetBool(PrefsKeys.Proposal.IsSeasonPassBought, value);
        }


        public bool ShouldShowPreviewScreen
        {
            get => CustomPlayerPrefs.GetBool(PrefsKeys.Proposal.SeasonEventShouldShowPreviewScreen, default);
            set => CustomPlayerPrefs.SetBool(PrefsKeys.Proposal.SeasonEventShouldShowPreviewScreen, value);
        }

        public bool ShouldShowLevelFinishAlert { get; set; }

        public int MaxLevel { get; private set; }

        public int MaxLevelWithoutOpenOnStart { get; private set; }

        public int MaxLevelIndexWithoutBonus =>
            MaxLevel - 1 - GetGeneratedReward(SeasonEventRewardType.Bonus).Length;

        public int MaxLevelIndexWithoutMainAndBonus =>
            MaxLevelIndexWithoutBonus - GetGeneratedReward(SeasonEventRewardType.Main).Length;

        public bool IsPetMainReward =>
            PetMainRewardType != PetSkinType.None;

        public float PointsCountOnPreviousShow { get; set; }

        public PetSkinType PetMainRewardType
        {
            get
            {
                bool isRewardActual = allGeneratedLiveOpsReward != null;

                if (!isRewardActual ||
                    !IsActive)
                {
                    return PetSkinType.None;
                }

                List<RewardData> allReward = new List<RewardData>();

                foreach (var d in allGeneratedLiveOpsReward.Values)
                {
                    allReward.AddRange(d);
                }


                int removed = allReward.RemoveAll(i => i == null);
                if (removed > 0)
                {
                    CustomDebug.Log("One of reward is NULL.");
                }

                RewardData petRewardData = allReward.Where(e => e.Type == RewardType.PetSkin).FirstOrDefault();

                PetSkinType foundSkinType = PetSkinType.None;

                if (petRewardData is PetSkinReward petReward)
                {
                    foundSkinType = petReward.skinType;
                }
                else
                {
                    CustomDebug.Log($"Found reward data isn't a {nameof(PetSkinReward)} instance");
                }

                return foundSkinType;
            }
        }


        public float PointsPerLevelEnd =>
            abTestService.CommonData.seasonEventAbSettings.PointsPerLevel;

        public bool IsGoldenTicketLock =>
            abTestService.CommonData.seasonEventAbSettings.GoldenTicketLock;


        protected override string LiveOpsPrefsMainKey =>
            PrefsKeys.Proposal.SeasonEventMainKey;


        private int[] PointsPerLevel
        {
            get
            {
                int pointsPerStepVariant = abTestService.CommonData.seasonEventAbSettings.PointsPerStepVariantIndex;
                return Settings.GetPointsPerStepVariant(pointsPerStepVariant);
            }
        }

        public bool ShouldCollectPoints =>
            IsActive;

        public bool ShouldPlayTutorialAnimation =>
            ShouldAnimateReward(0, SeasonEventRewardType.Simple) &&
            ShouldAnimateReward(0, SeasonEventRewardType.Pass) &&
            LevelReachIndex() == 0;

        public override string LiveOpsAnalyticName =>
            LiveOpsNames.SeasonEvent.Name;

        public override string LiveOpsAnalyticEventId
        {
            get
            {
                PetSkinType foundSkinType = PetSkinType.None;

                if (allGeneratedLiveOpsReward == null)
                {
                    return LiveOpsNames.SeasonEvent.GetEventName(foundSkinType);
                }

                List<RewardData> allReward = new List<RewardData>();

                foreach (var d in allGeneratedLiveOpsReward.Values)
                {
                    allReward.AddRange(d);
                }

                allReward.RemoveAll(i => i == null);

                RewardData petRewardData = allReward.FirstOrDefault(e => e.Type == RewardType.PetSkin);

                if (petRewardData is PetSkinReward petReward)
                {
                    foundSkinType = petReward.skinType;
                }

                return LiveOpsNames.SeasonEvent.GetEventName(foundSkinType);
            }
        }

        public override string LiveOpsAnalyticPosition =>
            string.Empty;


        public bool ShouldConsiderMainRewardClaimedForOldUsers =>
            !GetGeneratedReward(SeasonEventRewardType.Main).Where(e => e.Type == RewardType.PetSkin).Any();
            


        private bool IsOldSeasonEventRules
        {
            get => CustomPlayerPrefs.GetBool(PrefsKeys.Proposal.WasOldSeasonEventActiveOnStart);
            set => CustomPlayerPrefs.SetBool(PrefsKeys.Proposal.WasOldSeasonEventActiveOnStart, value);
        }


        public int CurrentLevel
        {
            get
            {
                SaveRewardData simpleData = GetClaimRewardData(SeasonEventRewardType.Simple);
                SaveRewardData passData = GetClaimRewardData(SeasonEventRewardType.Pass);

                if (simpleData == null || passData == null)
                {
                    return 0;
                }
                
                int simpleCompletedCount = simpleData.receivedRewards.Count(e => e);
                int passCompletedCount = passData.receivedRewards.Count(e => e);

                int result = Math.Max(simpleCompletedCount, passCompletedCount);

                return result;
            }
        }

        #endregion



        #region Class lifecycle

        public SeasonEventProposeController(SeasonEventSettings _settings,
                                            SeasonEventVisualSettings _visualSettings,
                                            IAbTestService _abTestService,
                                            ICommonStatisticsService _commonStatisticsService,
                                            IPlayerStatisticService _playerStatisticService,
                                            ITimeValidator _timeValidator,
                                            IShopService _shopService,
                                            params IContentOpen[] contentOpens) :
                                            base(_settings.LiveOpsProposeSettings(_abTestService.CommonData), _commonStatisticsService, _playerStatisticService, _timeValidator)
        {
            Settings = _settings;
            VisualSettings = _visualSettings;
            abTestService = _abTestService;
            shopService = _shopService;

            allGeneratedLiveOpsReward = new Dictionary<SeasonEventRewardType, RewardData[]>();
            allGeneratedLiveOpsRewardSerialization = new Dictionary<SeasonEventRewardType, RewardDataSerializationArray>();

            PointsCountOnPreviousShow = playerStatisticService.CurrencyData.GetEarnedCurrency(CurrencyType.SeasonEventPoints);

            foreach (var rewardType in UserSeasonEventRewardTypes)
            {
                string saveKey = string.Concat(PrefsKeys.Proposal.LastSeasonEventRewardPrefix, rewardType);
                RewardDataSerializationArray serializationArray = new RewardDataSerializationArray(saveKey);
                allGeneratedLiveOpsRewardSerialization.Add(rewardType, serializationArray);
            }

            AttemptRestoreOldUsersData();

            foreach (var contentOpen in contentOpens)
            {
                contentOpen.OnAnyContentOpened += AttemptRefreshRewardData;
            }

            playerStatisticService.CurrencyData.OnCurrencyAdded += CurrencyData_OnCurrencyAdded;
        }

        #endregion



        #region Methods

        public override void Initialize()
        {
            base.Initialize();

            AttemptRefreshRewardData();
            AttemptRefreshReceiveData();
        }


        public void ProposeSeasonPass(Action onProposeHided) =>
            UiScreenManager.Instance.ShowScreen(ScreenType.SeasonPassScreen, isForceHideIfExist: true, onHided: (view) => onProposeHided?.Invoke());


        public override void Propose()
        {
            UiScreenManager.Instance.HideScreen(ScreenType.SeasonEventScreen);
            UiScreenManager.Instance.HideScreen(ScreenType.SeasonPassScreen);

            FromLevelToLevel.PlayTransition(() =>
            {
                UiScreenManager.Instance.HideAll(true);
                LevelsManager.Instance.UnloadLevel();
                UiScreenManager.Instance.ShowScreen(ScreenType.SeasonEventScreen, isForceHideIfExist: true);
            });
        }


        public void MarkSeasonPassPurchased() =>
            IsSeasonPassActive = true;


        public void MarkBonusRewardReceived()
        {
            // a little hack
            int beforeBonusIndex = Mathf.Max(0, MaxLevelIndexWithoutBonus);
            float requiredCurrencyForAllRewardExceptBonus = PointsPerLevel[beforeBonusIndex];
            float requiredCurrencyForBonus = PointsPerLevel.Last();

            float currencyOffsetForBonus = requiredCurrencyForBonus - requiredCurrencyForAllRewardExceptBonus;

            playerStatisticService.CurrencyData.TryRemoveCurrency(CurrencyType.SeasonEventPoints, currencyOffsetForBonus);
            PointsCountOnPreviousShow = requiredCurrencyForBonus;

            CancelBonusRewardClaim();
            
            OnRewardReceived?.Invoke(SeasonEventRewardType.Bonus, 0);
        }


        public void MarkMainRewardReceived()
        {
            SaveRewardData data = GetClaimRewardData(SeasonEventRewardType.Main);
            for (int i = 0; i < data.receivedRewards.Length; i++)
            {
                data.receivedRewards[i] = true;
            }
            SetClaimRewardData(SeasonEventRewardType.Main, data);
            
            IsCurrentLiveOpsTaskCompleted = true;
            AnalyticHelper.SendLiveOpsCompleteEvent(LiveOpsAnalyticName, LiveOpsAnalyticEventId);
            
            OnRewardReceived?.Invoke(SeasonEventRewardType.Main, 0);
        }


        protected override void StartLiveOps()
        {
            IsOldSeasonEventRules = false;

            PointsCountOnPreviousShow = 0.0f;
            float currentValue = playerStatisticService.CurrencyData.GetEarnedCurrency(CurrencyType.SeasonEventPoints);
            playerStatisticService.CurrencyData.TryRemoveCurrency(CurrencyType.SeasonEventPoints, currentValue);

            int rewardDataIndex = abTestService.CommonData.seasonEventAbSettings.RewardsVariantIndex;
            SeasonEventSettings.Data data = Settings.GetRewardsData(ShowsCount, rewardDataIndex);

            foreach (var rewardType in UserSeasonEventRewardTypes)
            {
                allGeneratedLiveOpsRewardSerialization[rewardType].Data = data.GetRewardData(rewardType);
            }

            foreach (var serializationData in allGeneratedLiveOpsRewardSerialization)
            {
                SaveRewardData rewardDataReset = new SaveRewardData
                {
                    receivedRewards = new bool[serializationData.Value.Data.Length]
                };
                SetClaimRewardData(serializationData.Key, rewardDataReset);
            }

            IsSeasonPassActive = false;
            ShouldShowPreviewScreen = true;

            RefreshRewardData();

            AddElementsToAnimate(0, SeasonEventRewardType.Simple, SeasonEventRewardType.Pass);

            base.StartLiveOps();
        }


        protected override void FinishLiveOps()
        {
            IsSeasonPassActive = false;

            base.FinishLiveOps();
        }


        public RewardData[] GetGeneratedReward(SeasonEventRewardType rewardType)
        {
            if (allGeneratedLiveOpsReward.TryGetValue(rewardType, out RewardData[] result))
            {
                return result;
            }

            return Array.Empty<RewardData>();
        }


        public bool IsAllRewardWithoutBonusReached() =>
            LevelReachIndex() >= MaxLevelIndexWithoutBonus;


        public bool IsRewardReached(int rewardIndex, SeasonEventRewardType type)
        {
            bool result = default;

            if (type == SeasonEventRewardType.Bonus)
            {
                int bonusRewardLocalLength = MaxLevelIndexWithoutBonus + rewardIndex + 1;
                result = LevelReachIndex() >= bonusRewardLocalLength;
            }
            else if (type == SeasonEventRewardType.Main)
            {
                int mainRewardLocalLength = MaxLevelIndexWithoutMainAndBonus + rewardIndex + 1;
                result = LevelReachIndex() >= mainRewardLocalLength;
            }
            else
            {
                result = LevelReachIndex() >= rewardIndex;
            }

            return result;
        }


        public bool CanClaimAnyReward(int levelReachIndex, params SeasonEventRewardType[] seasonEventRewardType)
        {
            bool result = false;

            foreach (var rewardType in seasonEventRewardType)
            {
                if (rewardType != SeasonEventRewardType.None)
                {
                    // TODO: dirty bonus logic
                    if (rewardType == SeasonEventRewardType.Bonus &&
                        !IsRewardReached(0, rewardType))
                    {
                        continue;
                    }

                    SaveRewardData data = GetClaimRewardData(rewardType);

                    bool isAnyRewardUnclaimed = data.receivedRewards
                                                        .Take(levelReachIndex + 1)
                                                        .ToList()
                                                        .Contains(e => !e);

                    result |= isAnyRewardUnclaimed;
                }
            }

            return result;
        }


        public bool CanClaimAnyReward(int levelReachIndex)
        {
            SeasonEventRewardType[] seasonEventRewardTypes = (SeasonEventRewardType[])Enum.GetValues(typeof(SeasonEventRewardType));
            seasonEventRewardTypes = seasonEventRewardTypes.Where(e => e != SeasonEventRewardType.None).ToArray();

            return CanClaimAnyReward(levelReachIndex, seasonEventRewardTypes);
        }


        public bool ShouldAnimateReward(int rewardIndex, SeasonEventRewardType type)
        {
            bool isBonusReward = type == SeasonEventRewardType.Bonus;
            bool isMainReward = type == SeasonEventRewardType.Main;
            int bonusRewardLocalLength = MaxLevelIndexWithoutBonus + rewardIndex + 1;

            SaveRewardData data = GetClaimRewardData(type);
            return data.rewardsToAnimateOpen.Contains(predicate);


            bool predicate(int e)
            {
                if (isMainReward)
                {
                    return e == bonusRewardLocalLength - 1;
                }
                else if (isBonusReward)
                {
                    return e == bonusRewardLocalLength;
                }
                else
                {
                    return e == rewardIndex;
                }
            }
    }


        public void MarkAllReachedRewardAnimated()
        {
            foreach (var rewardType in UserSeasonEventRewardTypes)
            {
                SaveRewardData data = GetClaimRewardData(rewardType);

                bool wasAnimated = data.rewardsToAnimateOpen.Count > 0;
                if (wasAnimated)
                {
                    data.wasAnimatedOnce = true;
                }

                data.rewardsToAnimateOpen.Clear();

                SetClaimRewardData(rewardType, data);
            }
        }


        public bool IsLockedByGoldenTicket(SeasonEventRewardType rewardType, int localIndex)
        {
            bool result = IsGoldenTicketLock &&
                          rewardType == SeasonEventRewardType.Pass &&
                          !IsSeasonPassActive &&
                          IsRewardReached(localIndex, rewardType);

            return result;
        }


        public float GetLevelReachProgress(int currentPointsCount)
        {
            int levelReachIndex = LevelReachIndex(currentPointsCount);
            return MaxLevel == 0 ? default : levelReachIndex / MaxLevel;
        }


        public int LevelReachIndex() =>
            LevelReachIndex(playerStatisticService.CurrencyData.GetEarnedCurrency(CurrencyType.SeasonEventPoints));


        public int LevelReachIndex(float currentPointsCount)
        {
            int firstUnreachedIndex = Array.FindIndex(PointsPerLevel, e => currentPointsCount < e);
            
            int result = firstUnreachedIndex == -1 ? MaxLevel : firstUnreachedIndex;
            result -= 1;

            result = Mathf.Clamp(result, Settings.liveOpsStartRewardsCountOpen - 1, MaxLevel - 1);

            if (result > MaxLevelIndexWithoutBonus && !IsRewardClaimed(SeasonEventRewardType.Main, 0))
            {
                result = MaxLevelIndexWithoutBonus;
            }
            
            return result;
        }

        public (int, int) LevelMinMaxPoints(int level)
        {
            int currentLevel = Mathf.Clamp(level, 0, PointsPerLevel.Length - 1);
            int nextLevel = Mathf.Clamp(level + 1, 0, PointsPerLevel.Length - 1);

            return (PointsPerLevel[currentLevel], PointsPerLevel[nextLevel]);
        }

        public bool IsRewardClaimed(int i) =>
            Array.Exists(UserSeasonEventRewardTypes.ToArray(), e => IsRewardClaimed(e, i));


        public bool IsNextLevelReached(float previousValue, float currentValue)
        {
            int previousReachIndex = LevelReachIndex(previousValue);
            int currentReachIndex = LevelReachIndex(currentValue);

            return currentReachIndex > previousReachIndex;
        }


        public bool IsRewardClaimed(SeasonEventRewardType seasonEventRewardType, int i)
        {
            SaveRewardData data = GetClaimRewardData(seasonEventRewardType);

            if (data.receivedRewards.Length <= i)
            {
                CustomDebug.Log($"Bad logic found. Attempt to get data for index {i} while total length is {data.receivedRewards.Length}");
                return false;
            }

            return data.receivedRewards[i];
        }


        public bool WasRewardAnimatedOnce(SeasonEventRewardType rewardType)
        {
            SaveRewardData data = GetClaimRewardData(rewardType);
            return data.wasAnimatedOnce;
        }


        public void MarkRewardReceived(SeasonEventRewardType seasonEventRewardType, int rewardIndex)
        {
            SaveRewardData data = GetClaimRewardData(seasonEventRewardType);
            data.receivedRewards[rewardIndex] = true;
            SetClaimRewardData(seasonEventRewardType, data);
            
            OnRewardReceived?.Invoke(seasonEventRewardType, rewardIndex);
        }


        public bool IsAllRewardClaimed(SeasonEventRewardType seasonEventRewardType)
        {
            int count = GetClaimRewardData(seasonEventRewardType).receivedRewards.Where(e => e).Count();
            RewardData[] data = GetGeneratedReward(seasonEventRewardType);

            return count >= data.Length;
        }


        public UiSeasonEventRewardElement.State DefineElementInitialState(SeasonEventRewardType rewardType, int localIndex)
        {
            UiSeasonEventRewardElement.State state;

            bool isNextElementToAnimate = ShouldAnimateReward(localIndex, rewardType);

            bool shouldLockReward = !IsRewardReached(localIndex, rewardType) ||
                                    isNextElementToAnimate ||
                                    IsLockedByGoldenTicket(rewardType, localIndex);

            if (shouldLockReward)
            {
                state = UiSeasonEventRewardElement.State.NotReached;
                return state;
            }

            bool isClaimed = IsRewardClaimed(rewardType, localIndex);
            bool isForAds = (rewardType == SeasonEventRewardType.Pass || rewardType == SeasonEventRewardType.Main) &&
                            !IsSeasonPassActive;

            // hack crunch for bug with bonus, which can occures only on devies
            if (isClaimed && rewardType == SeasonEventRewardType.Bonus)
            {
                CancelBonusRewardClaim();
                isClaimed = false;
            }

            if (isClaimed)
            {
                state = UiSeasonEventRewardElement.State.Claimed;
            }
            else
            {
                bool shouldLockIfPreviousUnclaimed;
                bool lockIfPreviousUnclaimedAbTest = !IsOldSeasonEventRules &&
                                                     abTestService.CommonData.seasonEventAbSettings.LockIfPreviousUnclaimed &&
                                                     !IsGoldenTicketLock;

                if (rewardType == SeasonEventRewardType.Bonus)
                {
                    bool isAllRewardClaimed = IsAllRewardClaimed(SeasonEventRewardType.Main);

                    shouldLockIfPreviousUnclaimed = lockIfPreviousUnclaimedAbTest &&
                                                    !isAllRewardClaimed;
                }
                else if (rewardType == SeasonEventRewardType.Main)
                {
                    bool isAllRewardClaimed = IsAllRewardClaimed(SeasonEventRewardType.Pass);

                    shouldLockIfPreviousUnclaimed = lockIfPreviousUnclaimedAbTest &&
                                                    !isAllRewardClaimed;
                }
                else
                {
                    shouldLockIfPreviousUnclaimed = lockIfPreviousUnclaimedAbTest &&
                                                    localIndex > 0 &&
                                                    !IsRewardClaimed(rewardType, localIndex - 1);
                }

                if (shouldLockIfPreviousUnclaimed)
                {
                    state = isForAds ?
                        UiSeasonEventRewardElement.State.ReachedCanNotClaimForAds : UiSeasonEventRewardElement.State.ReachedCanNotClaim;
                }
                else
                {
                    state = isForAds ?
                        UiSeasonEventRewardElement.State.CanClaimForAds : UiSeasonEventRewardElement.State.ReadyToClaim;
                }
            }

            return state;
        }


        public bool IsNotSequenceSeasonEventRewardType(SeasonEventRewardType rewardType) =>
            UserNotSequenceSeasonEventRewardTypes.Contains(rewardType);


        private SaveRewardData GetClaimRewardData(SeasonEventRewardType seasonEventRewardType) =>
            CustomPlayerPrefs.GetObjectValue<SaveRewardData>(string.Concat(PrefsKeys.Proposal.SeasonEventRewardSaveData, seasonEventRewardType), default);


        private void SetClaimRewardData(SeasonEventRewardType seasonEventRewardType, SaveRewardData data) =>
            CustomPlayerPrefs.SetObjectValue(string.Concat(PrefsKeys.Proposal.SeasonEventRewardSaveData, seasonEventRewardType), data);


        private void AttemptRefreshRewardData()
        {
            if (IsMechanicAvailable && IsActive)
            {
                RefreshRewardData();
            }
        }


        private void RefreshRewardData()
        {
            allGeneratedLiveOpsReward.Clear();

            foreach (var data in allGeneratedLiveOpsRewardSerialization)
            {
                RewardData[] actualizedData = ActualizeRewardData(data.Value.Data);
                allGeneratedLiveOpsReward.Add(data.Key, actualizedData);
            }

            allGeneratedLiveOpsRewardSerialization.Values.Max(e => e.Data.Length);

            MaxLevel = allGeneratedLiveOpsRewardSerialization.Values.Max(e => e.Data.Length) +
                       GetGeneratedReward(SeasonEventRewardType.Main).Length +
                       GetGeneratedReward(SeasonEventRewardType.Bonus).Length;
            MaxLevelWithoutOpenOnStart = MaxLevel - Settings.liveOpsStartRewardsCountOpen;
        }


        private RewardData[] ActualizeRewardData(RewardData[] rawData)
        {
            List<RewardData> result = new List<RewardData>(rawData);

            RewardData[] skinsRewards = result.Where(e => (e.Type == RewardType.ShooterSkin || e.Type == RewardType.WeaponSkin) && !e.IsAvailableForRewardPack)
                                            .ToArray();

            for (int i = 0; i < skinsRewards.Length; i++)
            {
                if (!IsRewardClaimed(i))
                {
                    int index = result.IndexOf(skinsRewards[i]);
                    result[index] = Settings.skinsReplaceReward;
                }
            }

            RewardData[] petsRewards = result.Where(e => e.Type == RewardType.PetSkin && !e.IsAvailableForRewardPack).ToArray();

            for (int i = 0; i < petsRewards.Length; i++)
            {
                if (!IsRewardClaimed(SeasonEventRewardType.Main, i))
                {
                    int index = result.IndexOf(petsRewards[i]);
                    result[index] = Settings.petsReplaceReward;
                }
            }


            CurrencyReward[] currencyRewardsToReplace = result.Where(e => e.Type == RewardType.Currency)
                                                                         .Cast<CurrencyReward>()
                                                                         .Where(c => !c.currencyType.IsAvailableForShow())
                                                                         .ToArray();
            foreach (var data in currencyRewardsToReplace)
            {
                CurrencyReward savedData = data;
                int index = result.IndexOf(savedData);
                savedData.value = savedData.currencyType.ToPremium(savedData.value);
                savedData.currencyType = CurrencyType.Premium;

                result[index] = savedData;
            }

            return result.ToArray();
        }


        private void AttemptRefreshReceiveData()
        {
            if (!IsMechanicAvailable || !IsActive)
            {
                return;
            }

            foreach (var rewardType in UserSeasonEventRewardTypes)
            {
                SaveRewardData data = GetClaimRewardData(rewardType);

                if (data == null)
                {
                    data = new SaveRewardData();
                }

                int targetLength = GetGeneratedReward(rewardType).Length;
                int missedLength = targetLength - data.receivedRewards.Length;

                for (int i = data.receivedRewards.Length; i <= missedLength; i++)
                {
                    data.receivedRewards = data.receivedRewards.Add(false);
                }

                SetClaimRewardData(rewardType, data);
            }
        }


        private void AddElementsToAnimate(int index, params SeasonEventRewardType[] typesToAnimate)
        {
            foreach (var rewardType in typesToAnimate)
            {
                SaveRewardData saveRewardData = GetClaimRewardData(rewardType);
                saveRewardData.rewardsToAnimateOpen.Add(index);

                SetClaimRewardData(rewardType, saveRewardData);
            }
        }


        private void CancelBonusRewardClaim()
        {
            SaveRewardData data = GetClaimRewardData(SeasonEventRewardType.Bonus);

            for (int i = 0; i < data.receivedRewards.Length; i++)
            {
                data.receivedRewards[i] = false;
            }

            SetClaimRewardData(SeasonEventRewardType.Bonus, data);
        }


        private void AttemptRestoreOldUsersData()
        {
            bool isOldUser = PrefsUtility.TryDefineRestoreNeedInt("season_event_finished_levels_count_on_start", out int oldUsersFinishedLevelsCountOnStart);

            if (!isOldUser || !IsActive)
            {
                return;
            }

            IsOldSeasonEventRules = true;

            RewardDataSerializationArray oldUsersSimpleReward = new RewardDataSerializationArray("last_season_event_reward");
            allGeneratedLiveOpsRewardSerialization[SeasonEventRewardType.Simple].Data = oldUsersSimpleReward.Data;

            RewardDataSerializationArray oldUsersPassReward = new RewardDataSerializationArray("last_season_event_pass_reward");
            allGeneratedLiveOpsRewardSerialization[SeasonEventRewardType.Pass].Data = oldUsersPassReward.Data;

            // TODO: restore step
            int oldUsersLevelReachIndex = Mathf.Clamp(OldUsersLevelReachIndex(), 0, PointsPerLevel.Length - 1);
            int pointsCountToRestore = PointsPerLevel[oldUsersLevelReachIndex];
            playerStatisticService.CurrencyData.AddCurrency(CurrencyType.SeasonEventPoints, pointsCountToRestore);

            foreach (var rewardType in UserSeasonEventRewardTypes)
            {
                if (allGeneratedLiveOpsRewardSerialization[rewardType].Data == null ||
                    allGeneratedLiveOpsRewardSerialization[rewardType].Data.Length == 0)
                {
                    int rewardDataIndex = abTestService.CommonData.seasonEventAbSettings.RewardsVariantIndex;
                    SeasonEventSettings.Data data = Settings.GetRewardsData(ShowsCount - 1, rewardDataIndex);
                    allGeneratedLiveOpsRewardSerialization[rewardType].Data = data.GetRewardData(rewardType);
                }

                if (rewardType == SeasonEventRewardType.Pass)
                {
                    RewardData[] actualizedReward = allGeneratedLiveOpsRewardSerialization[rewardType].Data;
                    for (int i = 0; i < actualizedReward.Length; i++)
                    {
                        if (actualizedReward[i] is PetSkinReward petSkinReward)
                        {
                            if (!shopService.PetSkins.IsBought(petSkinReward.skinType))
                            {
                                int rewardVariantIndex = abTestService.CommonData.seasonEventAbSettings.RewardsVariantIndex;
                                RewardData replaceRewardData = Settings.GetOldUserPetReplaceRewardData(rewardVariantIndex);

                                actualizedReward[i] = replaceRewardData;
                            }
                            else
                            {
                                allGeneratedLiveOpsRewardSerialization[SeasonEventRewardType.Main].Data = allGeneratedLiveOpsRewardSerialization[SeasonEventRewardType.Bonus].Data;
                            }
                        }
                    }

                    allGeneratedLiveOpsRewardSerialization[rewardType].Data = actualizedReward;
                }
            }


            int OldUsersLevelReachIndex()
            {
                int seasonEventLevelsCountForLevelUp = 1;
                int OpenOnStartFinishedLevelsDelta = Settings.liveOpsStartRewardsCountOpen * seasonEventLevelsCountForLevelUp;
                int levelsDelta = commonStatisticsService.LevelsFinishedCount - oldUsersFinishedLevelsCountOnStart + OpenOnStartFinishedLevelsDelta;

                int result = seasonEventLevelsCountForLevelUp == 0 ?
                    int.MaxValue : (levelsDelta / seasonEventLevelsCountForLevelUp);

                return result - 1;
            }
        }

        #endregion



        #region Events handlers

        private void CurrencyData_OnCurrencyAdded(CurrencyType currencyType, float value)
        {
            if (currencyType != CurrencyType.SeasonEventPoints || !IsActive)
            {
                return;
            }

            float currentValue = playerStatisticService.CurrencyData.GetEarnedCurrency(currencyType);
            float currencyBeforeAdd = currentValue - value;
            
            bool isNextLevelReached = IsNextLevelReached(currencyBeforeAdd, currentValue);

            if (isNextLevelReached)
            {
                int previousReachIndex = LevelReachIndex(currencyBeforeAdd);
                int currentReachIndex = LevelReachIndex(currentValue);

                for (int i = previousReachIndex + 1; i <= currentReachIndex; i++)
                {
                    List<SeasonEventRewardType> types = new List<SeasonEventRewardType>(UserSeasonEventRewardTypes);

                    bool isBonus = i > MaxLevelIndexWithoutBonus;
                    if (!isBonus)
                    {
                        types.Remove(SeasonEventRewardType.Bonus);
                    }

                    bool isMain = i > MaxLevelIndexWithoutMainAndBonus;
                    if (!isMain)
                    {
                        types.Remove(SeasonEventRewardType.Main);
                    }

                    AddElementsToAnimate(i, types.ToArray());
                }
            }
        }

        #endregion
    }
}
