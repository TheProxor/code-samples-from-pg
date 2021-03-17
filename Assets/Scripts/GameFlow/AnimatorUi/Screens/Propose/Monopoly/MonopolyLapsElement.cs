using System;
using System.Collections.Generic;
using Drawmasters.Effects;
using Drawmasters.Utils.Ui;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Drawmasters.Proposal
{
    public class MonopolyLapsElement : MonopolyElement
    {
        #region Fields

        private const float ScaleIconSkinsMultiplier = 0.76f;

        [SerializeField] private IdleEffect idleRewardEffect = default;
        [SerializeField] private FactorAnimation idleRewardFadeAnimation = default;

        [SerializeField] private BlendImage backBlendImage = default;
        [SerializeField] private BlendImage lapsInfoBlendImage = default;
        [SerializeField] private TMP_Text lapsTextInfo = default;
        [SerializeField] private GameObject claimDoneIcon = default;

        [Header("Animation")]
        [SerializeField] private VectorAnimation claimDoneAnimation = default;

        #endregion



        #region Properties

        protected override List<BlendImage> BlendImages
        {
            get
            {
                List<BlendImage> result = base.BlendImages;

                result.Add(backBlendImage);
                result.Add(lapsInfoBlendImage);

                return result;
            }
        }

        protected override List<TMP_Text> TextToAnimateMaterial
        {
            get
            {
                List<TMP_Text> result = base.TextToAnimateMaterial;

                result.Add(lapsTextInfo);

                return result;
            }
        }

        #endregion



        #region Methods

        public override void ShowWinEffect() { }


        public void SetLapsesInfo(int lapsesCount) =>
            lapsTextInfo.text = lapsesCount.ToString();


        public void ShowIdleEffect()
        {
            idleRewardEffect.CreateAndPlayEffect();
            idleRewardEffect.SetAlpha(1.0f);
        }


        public void HideIdleEffect()
        {
            if (idleRewardEffect.IsCreated)
            {
                idleRewardFadeAnimation.Play((value) => idleRewardEffect.SetAlpha(value), this, StopIdleEffect);
            }
        }


        public void StopIdleEffect()
        {
            if (idleRewardEffect.IsCreated)
            {
                idleRewardEffect.StopEffect();
            }
        }


        public override void RefreshVisual()
        {
            base.RefreshVisual();

            bool isCurrencyReward = RewardData is CurrencyReward;

            icon.transform.localScale = isCurrencyReward ? Vector3.one : Vector3.one * ScaleIconSkinsMultiplier;
        }


        protected override Sprite GetRewardActiveSprite(RewardData data)
        {
            Sprite result = default;

            if (data is CurrencyReward currencyReward)
            {
                result = IngameData.Settings.monopoly.visualSettings.FindLapsCurrencyActiveIcon(currencyReward.currencyType);
            }
            else if (data is WeaponSkinReward weaponSkinReward)
            {
                result = IngameData.Settings.monopoly.visualSettings.FindLapsWeaponSkinActiveIcon(weaponSkinReward.skinType);
            }
            else if (data is ShooterSkinReward shooterSkinReward)
            {
                result = IngameData.Settings.monopoly.visualSettings.FindLapsShooterSkinActiveIcon(shooterSkinReward.skinType);
            }
            else
            {
                CustomDebug.Log($"Not implemented logic for {RewardData.Type}");
            }

            return result;
        }

        protected override Texture GetRewardClaimedTexture(RewardData data)
        {
            Texture result = default;

            if (data is CurrencyReward currencyReward)
            {
                result = IngameData.Settings.monopoly.visualSettings.FindLapsCurrencyClaimedIcon(currencyReward.currencyType);
            }
            else if (data is WeaponSkinReward weaponSkinReward)
            {
                result = IngameData.Settings.monopoly.visualSettings.FindLapsWeaponSkinClaimedIcon(weaponSkinReward.skinType);
            }
            else if (data is ShooterSkinReward shooterSkinReward)
            {
                result = IngameData.Settings.monopoly.visualSettings.FindLapsShooterSkinClaimedIcon(shooterSkinReward.skinType);
            }
            else
            {
                CustomDebug.Log($"Not implemented logic for {RewardData.Type}");
            }

            return result;
        }

        public override void SetClaimed(bool claimed, bool isImmediately)
        {
            base.SetClaimed(claimed, isImmediately);

            if (isImmediately)
            {
                CommonUtility.SetObjectActive(claimDoneIcon, claimed);
            }
            else
            {
                Action claimDoneCallback = claimed ? (Action)null : () => CommonUtility.SetObjectActive(claimDoneIcon, false);

                CommonUtility.SetObjectActive(claimDoneIcon, true);
                claimDoneAnimation.Play(
                       (value) => claimDoneIcon.transform.localScale = value,
                       this,
                       claimDoneCallback,
                       !claimed);
            }
        }

        #endregion
    }
}
