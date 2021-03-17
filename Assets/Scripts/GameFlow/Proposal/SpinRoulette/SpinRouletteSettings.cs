using System;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using UnityEngine;


namespace Drawmasters.Proposal
{
    [CreateAssetMenu(fileName = "SpinRouletteSettings",
                  menuName = NamingUtility.MenuItems.ProposalSettings + "SpinRouletteSettings")]
    public class SpinRouletteSettings : SingleRewardPackSettings
    {
        #region Nested types

        [Serializable]
        public class SpinSettings
        {
            [Header("Roullete")]
            public float rotateDuration = default;
            public AnimationCurve spinCurve = default;

            [Header("Fx")]
            public float fxShowDelay = default;
            public float fxShowDuration = default;

            public float fxHideDelay = default;
            public float fxHideDuration = default;
        }


        [Serializable]
        private class CurrencyIconData
        {
            public CurrencyType type = default;

            [SerializeField] private Sprite smallCurrencyRewardIcon = default;
            [SerializeField] private Sprite mediumCurrencyRewardIcon = default;
            [SerializeField] private Sprite bigCurrencyRewardIcon = default;

            [SerializeField] private float minCurrencyForMediumPack = default;
            [SerializeField] private float maxCurrencyForMediumPack = default;

            public Sprite FindCurrencyIcon(float currencyValue)
            {
                Sprite result;

                if (currencyValue < minCurrencyForMediumPack)
                {
                    result = smallCurrencyRewardIcon;
                }
                else if (currencyValue > maxCurrencyForMediumPack)
                {
                    result = bigCurrencyRewardIcon;
                }
                else
                {
                    result = mediumCurrencyRewardIcon;
                }

                return result;
            }
        }

        [Serializable]
        public class TutorialPropose
        {
            public FactorAnimation fadeAnimation = default;
            public float enablingButtonDelay = default;
            public float pressingButtonDelay = default;
            public float proposingDelay = default;
        }

        #endregion



        #region Fields

        [Header("Spin Roulette Common Settings")]
        public int shooterSkinsCount = default;
        public int weaponSkinsCount = default;
        public float gemsForSpin = default;

        public int minPremiumRewardCount = default;
        public int maxPremiumRewardCount = default;

        [Header("Visual")]
        [SerializeField] private Color[] alternationSegmentColors = default;

        [SerializeField] private CurrencyIconData[] currencyIconData = default;
        public float refreshDelayForSpinningState = default;
        public float refreshRewardDelayForSpinningState = default;
        public Sprite rouletteWheelSprite = default;

        [Header("Animation")]
        [Tooltip("Лимит углов для сегментов")]
        [SerializeField] private float anglesLimitForSegment = default;
        [Tooltip("Добавляем круги в анимацию, чтобы создать иллюзию рандома")]
        [SerializeField] private int additionalRoundsForSpin = default;

        public SpinSettings rewardSpinSettings = default;
        public SpinSettings refreshSpinSettings = default;

        [Header("Tutorial")]
        [SerializeField] private TutorialPropose tutorialPropose = default;

        [Header("Timer")]
        public double secondsRestToStartAnimateTimer = default;
        public VectorAnimation timerBounceAnimation = default;

        [Header("Sequence")]
        [Tooltip("Хардкод по запросу гд - при генерации награды со фри спина падает определенная пушка. Данный скин должен быть в пуле наград")]
        [SerializeField] private WeaponSkinType[] firstSequenceWeaponSkinRewards = default;
        [Tooltip("Хардкод по запросу гд - при генерации награды с первого рв спина падает определенная пушка. Данный скин должен быть в пуле наград")]
        [SerializeField] private ShooterSkinType[] firstSequenceShooterSkinRewards = default;

        public FactorAnimation buttonsBlendAnimation = default;

        public float hideRewardSceneDelay = default;

        #endregion



        #region Properties

        public TutorialPropose TutorialProposeSettings => tutorialPropose;

        protected override bool IsRewardDataAvailable(RewardData data)
        {
            bool result = base.IsRewardDataAvailable(data);

            if (data is WeaponSkinReward weaponSkinReward)
            {
                WeaponType rewardWeaponType = IngameData.Settings.weaponSkinSettings.GetWeaponType(weaponSkinReward.skinType);

                IPlayerStatisticService service = GameServices.Instance.PlayerStatisticService;
                bool isWeaponOpen = service.ModesData.IsWeaponOpen(rewardWeaponType);

                result &= isWeaponOpen;
            }

            return result;
        }


        #endregion



        #region Methods

        public override RewardData[] GetCommonShowRewardPack()
        {
            List<RewardData> result = new List<RewardData>();

            int premiumRewardsCount = UnityEngine.Random.Range(minPremiumRewardCount, maxPremiumRewardCount + 1);

            for (int i = 0; i < rewardCountInPack; i++)
            {
                bool isFullShooterSkinsFilled = result.Count(e => e is ShooterSkinReward) >= shooterSkinsCount;
                bool isAnyShooterSkinAvailable = AvailableShooterSkinsRewardData.Count(e => !result.Contains(e)) > 0;

                bool isFullWeaponSkinsFilled = result.Count(e => e is WeaponSkinReward) >= weaponSkinsCount;
                bool isAnyWeaponSkinAvailable = AvailableWeaponSkinsRewardData.Any(e => !result.Contains(e));

                bool isFullPremiumRewardFilled = result.Count(e => e is CurrencyReward cr && cr.currencyType == CurrencyType.Premium) >= premiumRewardsCount;
                bool isAnyPremiumRewardAvailable = AvailablePremiumCurrencyRewardData.Any(e => !result.Contains(e));

                RewardData[] rewardData = default;

                if (isAnyShooterSkinAvailable && !isFullShooterSkinsFilled)
                {
                    rewardData = AvailableShooterSkinsRewardData;
                }
                else if (isAnyWeaponSkinAvailable && !isFullWeaponSkinsFilled)
                {
                    rewardData = AvailableWeaponSkinsRewardData;
                }
                else if (isAnyPremiumRewardAvailable && !isFullPremiumRewardFilled)
                {
                    rewardData = AvailablePremiumCurrencyRewardData;
                }
                else
                {
                    rewardData = AvailableCurrencyRewardData;
                }

                rewardData = rewardData.ToList().Where(e => !result.Contains(e)).ToArray();
                RewardData data = RewardDataUtility.GetRandomReward(rewardData);

                if (data != null)
                {
                    result.Add(data);
                }
            }

            result.Shuffle();

            return result.ToArray();
        }


        public Color FindSegmentColor(int index)
        {
            return alternationSegmentColors.Length > 0 ?
                  alternationSegmentColors[index % alternationSegmentColors.Length] : Color.white;
        }


        public Sprite FindCurrencyIcon(CurrencyType type, float currencyValue)
        {
            CurrencyIconData data = Array.Find(currencyIconData, e => e.type == type);

            if (data == null)
            {
                CustomDebug.Log($"No icon data fount for currency type {type} in {this}");
                return null;
            }

            Sprite result = data.FindCurrencyIcon(currencyValue);

            return result;
        }


        public Sprite FindShooterSkinActiveIcon(ShooterSkinType type) =>
            IngameData.Settings.shooterSkinsSettings.GetUiOutlineSprite(type);


        public Sprite FindShooterSkinClaimedIcon(ShooterSkinType type) =>
            IngameData.Settings.shooterSkinsSettings.GetUiOutlineDisabledSprite(type);


        public Sprite FindWeaponSkinActiveIcon(WeaponSkinType type) =>
            IngameData.Settings.weaponSkinSettings.GetUiOutlineSprite(type);


        public Sprite FindWeaponSkinClaimedIcon(WeaponSkinType type) =>
            IngameData.Settings.weaponSkinSettings.GetUiOutlineDisabledSprite(type);


        public RewardData GetRandomSpinReward(int showIndex, RewardData[] data, bool isFreeSpin)
        {
            RewardData result = default;

            bool canProposeSequenceWeapon = showIndex < firstSequenceWeaponSkinRewards.Length;
            bool canProposeSequenceShooter = showIndex < firstSequenceShooterSkinRewards.Length;

            if (canProposeSequenceWeapon || canProposeSequenceShooter)
            {
                Func<RewardData, bool> findPredicate = isFreeSpin ?
                    new Func<RewardData, bool>(e => canProposeSequenceWeapon && e is WeaponSkinReward weaponSkinReward && weaponSkinReward.skinType == firstSequenceWeaponSkinRewards[showIndex]) :
                    e => canProposeSequenceShooter && e is ShooterSkinReward shooterSkinReward && shooterSkinReward.skinType == firstSequenceShooterSkinRewards[showIndex];

                result = data.Find(findPredicate);
            }

            result = result ?? RewardDataUtility.GetRandomReward(data);

            return result;
        }


        public float AngleForSegment(int index)
        {
            float startValue = index * anglesLimitForSegment;
            float angleOffset = anglesLimitForSegment * 0.5f;

            return startValue + angleOffset + 360.0f * additionalRoundsForSpin;
        }

        #endregion


#if UNITY_EDITOR
        #region Editor methods

        private void OnValidate()
        {
            if (UnityEditor.EditorApplication.isPlaying)
            {
                return;
            }

            const int rewardsCount = 8; // number of segments in spin roulette

            rewardCountInPack = rewardsCount;
            skinsCountInPack = shooterSkinsCount + weaponSkinsCount;

            if ((currencyRewards.Length + minPremiumRewardCount) < rewardCountInPack)
            {
                CustomDebug.Log($"<color=red> Currency reward must be more then {rewardsCount} in {this} </color>");
            }

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
