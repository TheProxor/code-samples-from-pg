using UnityEngine;
using System;
using Spine.Unity;
using Drawmasters.Utils;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Proposal
{
    [CreateAssetMenu(fileName = "CommonRewardSettings",
                 menuName = NamingUtility.MenuItems.ProposalSettings + "CommonRewardSettings")]
    public class CommonRewardSettings : ScriptableObject
    {
        #region Nested types

        [Serializable]
        public class CurrencyVisualSettings : CurrencySpritesSettings
        {
            [Enum(typeof(EffectKeys))]
            public string fxTrail = default;
        }


        [Serializable]
        public class CurrencySpritesSettings
        {
            public CurrencyType type = default;
            public Sprite sprite = default;
        }

        #endregion



        #region Fields

        public string nothingReceivedSkipKey = default;
        public string somethingReceivedSkipKey = default;

        [SerializeField] private FactorAnimation skipRootAlphaAnimation = default;
        [SerializeField] private FactorAnimation forcemeterSkipRootAlphaAnimation = default;

        [SerializeField] private CurrencyVisualSettings[] currencySpritesSettings = default;
        [SerializeField] private CurrencySpritesSettings[] priceSpritesSettings = default;
      
        [Header("Reward Screen")]
        public VectorAnimation moneyTrailAnimation = default;

        public VectorAnimation scaleInCurrencyAnimation = default;
        public VectorAnimation scaleOutCurrencyAnimation = default;

        [Header("Next mode")]
        public float forceSwipeDelay = default;
        public float forceSwipesBetweenDelay = default;

        [Header("Timer")]
        public SkeletonDataAsset timer = default;

        [SpineSkin(dataField = "timer")] public string eventDisabledTimerSkin = default;
        [SpineSkin(dataField = "timer")] public string eventActiveTimerSkin = default;

        [Header("Live opses")]
        public PressButtonUtility.Data liveOpsesButtonPressData = default;

        [Header("Offers")]
        public PressButtonUtility.Data offersButtonPressData = default;
        public float offerAppearSoundDelay = default;
        public float offerRefreshRootsAfterHideAnimationDelay = default;

        [Header("Rate us")]
        public RewardDataInspectorSerialization rateUsRewardData = default;

        #endregion



        #region Methods

        public FactorAnimation SkipRootAlphaAnimation
        {
            get
            {
                skipRootAlphaAnimation.SetDelay(GameServices.Instance.AbTestService.CommonData.proposalSkipShowDelay);
                return skipRootAlphaAnimation;
            }
        }

        public FactorAnimation ForcemeterSkipRootAlphaAnimation
        {
            get
            {
                skipRootAlphaAnimation.SetDelay(GameServices.Instance.AbTestService.CommonData.forcemeterSkipShowDelay);
                return forcemeterSkipRootAlphaAnimation;
            }
        }


        public Sprite FindShopPriceRewardSprite(CurrencyType type)
        {
            CurrencySpritesSettings settings = Array.Find(priceSpritesSettings, e => e.type == type);
            return settings == null ? default : settings.sprite;
        }

        public Sprite FindCurrencyRewardSprite(CurrencyType type)
        {
            CurrencyVisualSettings settings = Array.Find(currencySpritesSettings, e => e.type == type);
            return settings == null ? default : settings.sprite;
        }


        public string FindCurrencyTrailFx(CurrencyType type)
        {
            CurrencyVisualSettings settings = Array.Find(currencySpritesSettings, e => e.type == type);
            return settings == null ? default : settings.fxTrail;
        }

        #endregion
    }
}
