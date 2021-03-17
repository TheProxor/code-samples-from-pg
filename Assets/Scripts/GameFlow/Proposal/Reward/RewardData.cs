using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Drawmasters.Utils;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Proposal
{
    [Serializable]
    public class RewardData : PolymorphicObject // Must be abstract, but it's not because of serialization problems
    {
        public static RewardType[] SkinsRewardTypes = { RewardType.ShooterSkin, RewardType.WeaponSkin, RewardType.PetSkin };

        public int weight = default;

        public RewardDataReceiveType receiveType = default;
        [ShowIf("receiveType", RewardDataReceiveType.Currency)] public float price = default;
        [ShowIf("receiveType", RewardDataReceiveType.Currency)] public CurrencyType priceType = default;
        
        [ShowIf("receiveType", RewardDataReceiveType.RandomFromPack)] public int weightFromPack = default;

        public RewardData()
        {
        }
        
        public virtual string RewardText { get; }

        public virtual RewardType Type { get; }
        public virtual bool IsAvailableForRewardPack { get; }
        public bool IsPurchasable => receiveType == RewardDataReceiveType.Currency;

        public bool EnoughCurrencyForPurchase => !IsPurchasable || GameServices.Instance.PlayerStatisticService.CurrencyData.GetEarnedCurrency(priceType) >= price;
        public bool IsForVideo => receiveType == RewardDataReceiveType.Video;

        public virtual void Open() { }
        public virtual Sprite GetUiSprite() { throw new NotImplementedException(); }

        public Sprite GetUiShopPriceSprite() => IngameData.Settings.commonRewardSettings.FindShopPriceRewardSprite(priceType);

        public virtual void Apply() { }

        public virtual bool IsEqualsReward(RewardData anotherReward) =>
            false;

        public RewardData Clone() =>
            (RewardData)MemberwiseClone();
    }
}
