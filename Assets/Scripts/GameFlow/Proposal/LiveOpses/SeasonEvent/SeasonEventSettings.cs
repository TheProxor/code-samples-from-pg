using System;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.Announcer;
using UnityEngine;


namespace Drawmasters.Proposal
{
    [CreateAssetMenu(fileName = "SeasonEventSettings",
                  menuName = NamingUtility.MenuItems.ProposalSettings + "SeasonEventSettings")]
    public class SeasonEventSettings : SingleRewardPackSettings
    {
        #region Nested types

        [Serializable]
        public class Data
        {
            public RewardDataInspectorSerialization[] simpleRewards = default;
            public RewardDataInspectorSerialization[] passRewards = default;

            public RewardDataInspectorSerialization mainReward = default;
            public RewardDataInspectorSerialization finishReward = default;

            [Tooltip("Если true - то показывается только последовательно (по индексу массива) и " +
                     "после показа всей секвенции даты, не участвует в рандоме")]
            public bool isOnlyForSequenceShow = default;

            public RewardData[] GetRewardData(SeasonEventRewardType rewardType)
            {
                RewardDataInspectorSerialization[] serializations = default;

                switch (rewardType)
                {
                    case SeasonEventRewardType.Simple:
                        serializations = simpleRewards;
                        break;

                    case SeasonEventRewardType.Pass:
                        serializations = passRewards;
                        break;

                    case SeasonEventRewardType.Bonus:
                        serializations = new RewardDataInspectorSerialization[] { finishReward };
                        break;

                    case SeasonEventRewardType.Main:
                        serializations = new RewardDataInspectorSerialization[] { mainReward };
                        break;

                    default:
                        CustomDebug.Log("Not Implemented");
                        Debug.Break();
                        return default;
                }

                return serializations.Select(e => e.RewardData).ToArray();
            }
        }

        [Serializable]
        private class AbTestRewardData
        {
            [Header("Reward")]
            public Data[] rewardData = default;
            [Header("Old Users")]
            public RewardDataInspectorSerialization oldUserPetsReplaceReward = default;
        }

        [Serializable]
        private class AbTestPointsPerStepData
        {
            [Header("Points offsets per steps")]
            public int[] pointsOffsetsPerStepVariant = default;
        }

        #endregion



        #region Fields

        public int liveOpsStartRewardsCountOpen = default;

        [Header("Rewards Data")]
        [SerializeField] private AbTestRewardData[] abTestRewardData = default;
        [SerializeField] private AbTestPointsPerStepData[] abTestPointsPerStepData = default;

        [Header("Reward Replace Data")]
        [Tooltip("В случае если уже получена непотребляемая награда (скины, пет). То она заменяется одной из нижеперечисленных")]
        public CurrencyReward petsReplaceReward = default;
        public CurrencyReward skinsReplaceReward = default;

        [Header("Season Pass Data")]
        public CurrencyReward[] passRewardData = default;

        [Header("Tutorial")]
        public FactorAnimation tutorialFadeAnimation = default;

        [Header("Announcer")]
        public MonopolyCurrencyAnnouncer canNotClaimAnnouncerPrefab = default;
        public string canNotClaimAnnouncerKey = default;
        public string buyGoldenTicketAnnouncerKey = default;

        #endregion



        #region Methods

        public LiveOpsProposeSettings LiveOpsProposeSettings(AbTestData abTestData)
        {
            LiveOpsProposeSettings result = new LiveOpsProposeSettings
            {
                IsAvailable = abTestData.isSeasonEventAvailable,
                DurationTime = abTestData.seasonEventDurationSeconds,
                MinLevelForLiveOps = abTestData.minLevelForSeasonEvent,
                IsReloadTimeUsed = abTestData.seasonEventUseReloadTimer,
                ReloadTime = abTestData.seasonEventReloadTime.ReloadSeconds,
                NotificationSecondsBeforeLiveOpsFinish = abTestData.seasonEventNotificationSecondsBeforeLiveOpsFinish,
                AvailabilitySettings = abTestData.seasonEventAvailabilitySettings
            };

            return result;
        }


        public override RewardData[] GetCommonShowRewardPack() =>
            Array.Empty<RewardData>();

        public Data GetRewardsData(int eventIndex, int dataIndex)
        {
            Data[] currentRewardData = GetCurrentAbTestRewardData(dataIndex).rewardData;
            Data result = default;

            bool useSequence = eventIndex < currentRewardData.Length;

            if (useSequence)
            {
                result = currentRewardData[eventIndex];
            }
            else
            {
                List<Data> incompletedData = new List<Data>();

                IEnumerable<Data> repeatData = currentRewardData.Where(e => !e.isOnlyForSequenceShow);
                foreach (var d in repeatData)
                {
                    bool isAnySimpleDataIncomplete = d.simpleRewards
                        .Where(e => e.rewardType != RewardType.Currency)
                        .Select(e => e.RewardData)
                        .Any(e => e.IsAvailableForRewardPack);

                    bool isAnyPassDataIncomplete = d.passRewards
                        .Where(e => e.rewardType != RewardType.Currency)
                        .Select(e => e.RewardData)
                        .Any(e => e.IsAvailableForRewardPack);

                    bool isIncompleted = isAnySimpleDataIncomplete || isAnyPassDataIncomplete;

                    if (isIncompleted)
                    {
                        incompletedData.Add(d);
                    }
                }

                if (incompletedData.Any())
                {
                    result = incompletedData.RandomObject();
                }
                else
                {
                    //TODO may be result
                    Data completedData = currentRewardData.RandomObject();
                }
            }

            return result;
        }


        public int[] GetPointsPerStepVariant(int dataIndex)
        {
            int[] offsets = GetCurrentAbTestPointsPerStepData(dataIndex).pointsOffsetsPerStepVariant;

            int[] result = new int[offsets.Length];

            for (int i = 0; i < offsets.Length; i++)
            {
                int pointsPerStep = default;

                for (int j = i; j >= 0; j--)
                {
                    pointsPerStep += offsets[j];
                }

                result[i] = pointsPerStep;
            }

            return result;
        }


        public RewardData GetOldUserPetReplaceRewardData(int dataIndex) =>
            GetCurrentAbTestRewardData(dataIndex).oldUserPetsReplaceReward.RewardData;
            

        private AbTestRewardData GetCurrentAbTestRewardData(int index)
        {
            bool isDataIndexCorrect = index >= 0 && index < abTestRewardData.Length;
            if (!isDataIndexCorrect)
            {
                CustomDebug.Log("Wrong index");
            }

            return isDataIndexCorrect ?
                abTestRewardData[index] : abTestRewardData.First();
        }

        private AbTestPointsPerStepData GetCurrentAbTestPointsPerStepData(int index)
        {
            bool isDataIndexCorrect = index >= 0 && index < abTestRewardData.Length;
            if (!isDataIndexCorrect)
            {
                CustomDebug.Log("Wrong index");
            }

            return isDataIndexCorrect ?
                abTestPointsPerStepData[index] : abTestPointsPerStepData.First();
        }

        #endregion


#if UNITY_EDITOR
        #region Editor methods

        private void OnValidate()
        {
            List<RewardData> allReward = new List<RewardData>();
            allReward.AddRange(currencyRewards);
            allReward.AddRange(shooterSkinRewards);
            allReward.AddRange(weaponSkinRewards);
            allReward.AddRange(premiumCurrencyRewards);

            foreach (var r in sequenceData)
            {
                allReward.AddRange(r.AllRewardData);
            }

            foreach (var reward in allReward)
            {
                reward.receiveType = RewardDataReceiveType.RandomFromPack;
            }
        }

        #endregion
#endif
    }
}
