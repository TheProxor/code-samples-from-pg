using System;
using DG.Tweening;
using Drawmasters.Effects;
using Drawmasters.ServiceUtil;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.Proposal
{
    public class SpineRouletteElement : MonoBehaviour
    {
        #region Nested types

        [Serializable]
        private class RewardVisual
        {
            public RewardType type = default;

            public GameObject root = default;

            public Image activeIcon = default;
            public Image claimedIcon = default;

            public TMP_Text textInfo = default;

            public GameObject availableRoot = default;
            public GameObject claimedRoot = default;
        }

        #endregion



        #region Fields

        [SerializeField] private Image back = default;

        [SerializeField] private RewardVisual[] rewardVisuals = default;

        [Header("Effects")]
        [SerializeField] private IdleEffect idleRewardEffect = default;
        [SerializeField] private Transform idleRewardEffectWorldRoot = default; // hack to change fx draw order
        [SerializeField] private FactorAnimation idleRewardFadeAnimation = default;

        [SerializeField] private Transform iconCurrencyTransform = default;

        #endregion



        #region Properties

        public Transform IconCurrencyTransform => iconCurrencyTransform;

        public RewardData RewardData { get; private set; }

        #endregion



        #region Methods

        public void SetupReward(RewardData rewardData) =>
            RewardData = rewardData;


        public void RefreshVisual()
        {
            foreach (var rewardVisual in rewardVisuals)
            {
                CommonUtility.SetObjectActive(rewardVisual.root, rewardVisual.type == RewardData.Type);
            }

            RewardVisual visual = Array.Find(rewardVisuals, e => e.type == RewardData.Type);

            bool isRewardAvailable = default;
            string infoText = string.Empty;
            Sprite activeIconSprite = default;
            Sprite claimedIconSprite = default;

            SpinRouletteSettings settings = IngameData.Settings.spinRouletteSettings;

            switch (RewardData)
            {
                case CurrencyReward currency:
                    isRewardAvailable = true;
                    infoText = currency.UiRewardText;

                    if (currency is CurrencyReward currencyReward)
                    {
                        activeIconSprite = settings.FindCurrencyIcon(currency.currencyType, currency.value);
                    }
                    break;

                case ShooterSkinReward shooterSkinReward:
                    isRewardAvailable = !GameServices.Instance.ShopService.ShooterSkins.IsBought(shooterSkinReward.skinType);
                    activeIconSprite = settings.FindShooterSkinActiveIcon(shooterSkinReward.skinType);
                    claimedIconSprite = settings.FindShooterSkinClaimedIcon(shooterSkinReward.skinType);
                    break;

                case WeaponSkinReward weaponSkinReward:
                    isRewardAvailable = !GameServices.Instance.ShopService.WeaponSkins.IsBought(weaponSkinReward.skinType);
                    activeIconSprite = settings.FindWeaponSkinActiveIcon(weaponSkinReward.skinType);
                    claimedIconSprite = settings.FindWeaponSkinClaimedIcon(weaponSkinReward.skinType);
                    break;

                default:
                    CustomDebug.Log($"Not implemented logic for {RewardData.Type}");
                    break;
            }

            if (visual != null)
            {
                CommonUtility.SetObjectActive(visual.availableRoot, isRewardAvailable);
                CommonUtility.SetObjectActive(visual.claimedRoot, !isRewardAvailable);

                if (visual.textInfo != null)
                {
                    visual.textInfo.text = infoText;
                }

                if (visual.activeIcon != null)
                {
                    visual.activeIcon.sprite = activeIconSprite;
                    visual.activeIcon.SetNativeSize();
                }

                if (visual.claimedIcon != null)
                {
                    visual.claimedIcon.sprite = claimedIconSprite;
                    visual.claimedIcon.SetNativeSize();
                }
            }
        }


        public void Deinitialize()
        {
            DOTween.Kill(this);
        }


        public void ShowWinEffect()
        {
            idleRewardEffect.CreateAndPlayEffect();
            idleRewardEffect.SetParent(idleRewardEffectWorldRoot);
            idleRewardEffect.SetAlpha(1.0f);
        }


        public void HideWinEffect()
        {
            idleRewardFadeAnimation.Play(value => idleRewardEffect.SetAlpha(value), this, StopWinEffect);
        }


        public void StopWinEffect() =>
            idleRewardEffect.StopEffect();
        

        public void SetBackColor(Color color) =>
            back.color = color;

        #endregion
    }
}
