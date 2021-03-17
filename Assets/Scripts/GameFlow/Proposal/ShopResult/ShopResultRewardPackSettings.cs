using System.Collections.Generic;
using System.Linq;
using Drawmasters.Geometry;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Proposal
{
    [CreateAssetMenu(fileName = "ShopResultRewardPackSettings",
                  menuName = NamingUtility.MenuItems.ProposalSettings + "ShopResultRewardPackSettings")]
    public class ShopResultRewardPackSettings : SingleRewardPackSettings
    {
        #region Nested Types

        private enum AvailableFillType
        {
            None = 0,
            OnlyVideo = 1,
            ExpectVideo = 2
        }

        #endregion



        #region Fields

        [Tooltip("Награда из премиум листа идет идет в крайний левый слот, если все скины скуплены.")]
        [SerializeField] private int premiumRewardCountInPack = default;

        [Tooltip("Преимущественно генерируем награду (за валюту) в пределах " +
            "от (текущее - lessCurrencyDifference) до (текущее + moreCurrencyDifference)")]
        [SerializeField] private float lessCurrencyDifference = default;
        [SerializeField] private float moreCurrencyDifference = default;

        [Header("Optional settings")]
        public bool shouldProposeCloseToPlayerCurrency = default;

        [Header("Первый показ")]
        public ShooterSkinReward[] firstShowShooterSkinRewards = default;
        public WeaponSkinReward[] firstShowWeaponSkinRewards = default;

        private AvailableFillType availableFillType;

        #endregion



        #region Properties

        private RewardData[] GetAvailableShooterSkinsReward(bool isFirstShow, IList<RewardData> exceptedReward)
        {
            bool isAnyFirstShowSkinAvailable = SelectAvailableRewardData(firstShowShooterSkinRewards)
                                                .Where(e => !exceptedReward.Contains(e)).Count() > 0;

            return (isFirstShow && isAnyFirstShowSkinAvailable) ? firstShowShooterSkinRewards : AvailableShooterSkinsRewardData;
        }

        private RewardData[] GetAvailableWeaponSkinsReward(bool isFirstShow, IList<RewardData> exceptedReward)
        {
            bool isAnyFirstShowSkinAvailable = SelectAvailableRewardData(firstShowWeaponSkinRewards)
                                                .Where(e => !exceptedReward.Contains(e)).Count() > 0;

            return (isFirstShow && isAnyFirstShowSkinAvailable) ? firstShowWeaponSkinRewards : AvailableWeaponSkinsRewardData;
        }

        private RewardData[] GetAvailableSKinsReward(bool isFirstShow, IList<RewardData> exceptedReward)
        {
            RewardData[] sequenceData = GetSequenceData();
            List<RewardData> ignoreShooterSkinRewards = sequenceData.Where(e => e.Type == RewardType.ShooterSkin).ToList();
            List<RewardData> ignoreWeaponSkinReward = sequenceData.Where(e => e.Type == RewardType.WeaponSkin).ToList();

            exceptedReward = exceptedReward.Concat(ignoreShooterSkinRewards).Concat(ignoreWeaponSkinReward).ToList();

            List<RewardData> result = new List<RewardData>();
            result.AddRange(GetAvailableShooterSkinsReward(isFirstShow, exceptedReward));
            result.AddRange(GetAvailableWeaponSkinsReward(isFirstShow, exceptedReward));
            return result.ToArray();
        }

        #endregion



        #region Methods

        protected override bool IsRewardDataAvailable(RewardData data)
        {
            bool result = base.IsRewardDataAvailable(data);

            if (data.Type != RewardType.Currency)
            {
                if (availableFillType == AvailableFillType.ExpectVideo)
                {
                    result &= data.receiveType != RewardDataReceiveType.Video;
                }
                else if (availableFillType == AvailableFillType.OnlyVideo)
                {
                    result &= data.receiveType == RewardDataReceiveType.Video;
                }
            }

            return result;
        }


        public override RewardData[] GetCommonShowRewardPack() =>
            SelectRewardPack(false);


        private RewardData[] SelectRewardPack(bool isFirstShow)
        {
            List<RewardData> result = new List<RewardData>();
            
            bool wasAllSkinsClaimed = GetAvailableSKinsReward(isFirstShow, result).All(e => result.Contains(e));

            for (int i = 0; i < rewardCountInPack; i++)
            {
                availableFillType = AvailableFillType.None;

                bool isSkinForVideoInPack = result.Contains(e => e.receiveType == RewardDataReceiveType.Video);
                availableFillType = isSkinForVideoInPack ? AvailableFillType.ExpectVideo : AvailableFillType.OnlyVideo;

                bool isAnySkinRest = GetAvailableSKinsReward(isFirstShow, result).Any(e => !result.Contains(e));

                bool isFullPremiumRewardFilled = result.Count(e => e is CurrencyReward cr && cr.currencyType == CurrencyType.Premium) >= premiumRewardCountInPack;
                bool isAnyPremiumRewardRest = AvailablePremiumCurrencyRewardData.Any(e => !result.Contains(e));

                if (isSkinForVideoInPack)
                {
                    isAnySkinRest &= GetAvailableSKinsReward(isFirstShow, result).Any(e => e.receiveType != RewardDataReceiveType.Video);
                }

                RewardData[] rewardData = default;

                if (i < skinsCountInPack && isAnySkinRest)
                {
                    float playerCurrency = GameServices.Instance.PlayerStatisticService.CurrencyData.GetEarnedCurrency(CurrencyType.Simple);

                    RewardData[] skinsForCurrency = GetAvailableSKinsReward(isFirstShow, result);

                    if (shouldProposeCloseToPlayerCurrency)
                    {
                        skinsForCurrency = skinsForCurrency
                            .Where(e => e.receiveType == RewardDataReceiveType.Currency &&
                                    MathUtility.InRange(playerCurrency - lessCurrencyDifference, playerCurrency + moreCurrencyDifference, e.price) &&
                                    !result.Contains(e))
                            .ToArray();
                    }

                    skinsForCurrency = skinsForCurrency.Where(e => !result.Contains(e)).ToArray();
                    rewardData = skinsForCurrency.Length > 0 ? skinsForCurrency : GetAvailableSKinsReward(isFirstShow, result);
                }
                else if (wasAllSkinsClaimed && !isFullPremiumRewardFilled && isAnyPremiumRewardRest)
                {
                    rewardData = AvailablePremiumCurrencyRewardData;
                }
                else
                {
                    rewardData = AvailableCurrencyRewardData;
                }

                rewardData = rewardData
                    .Where(e => !result.Contains(e))
                    .ToArray();

                if (rewardData.Length == 0)
                {
                    CustomDebug.Log($"Wrong reward select in {this}");
                    Debug.Break();
                }

                RewardData data = RewardDataUtility.GetRandomReward(rewardData);

                if (data != null)
                {
                    result.Add(data);
                }
            }

            return result.OrderBy(e => e.receiveType).ToArray();
        }

        #endregion



        #region Editor methods

        private void OnValidate()
        {
            List<RewardData> allReward = new List<RewardData>();
            allReward.AddRange(currencyRewards);
            allReward.AddRange(shooterSkinRewards);
            allReward.AddRange(weaponSkinRewards);

            foreach (var reward in allReward)
            {
                if (reward.receiveType == RewardDataReceiveType.Default ||
                    reward.Type == RewardType.Currency)
                {
                    reward.receiveType = RewardDataReceiveType.Video;
                }
            }
        }


        #endregion
    }
}
