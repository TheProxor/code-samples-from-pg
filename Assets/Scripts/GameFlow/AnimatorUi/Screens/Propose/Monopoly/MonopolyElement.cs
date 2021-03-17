using TMPro;
using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;
using Drawmasters.Utils.Ui;


namespace Drawmasters.Proposal
{
    public abstract class MonopolyElement : MonoBehaviour
    {
        #region Fields

        [SerializeField] private TMP_Text textInfo = default;

        [SerializeField] protected BlendImage icon = default;

        [Header("Effects")]

        [SerializeField] private Transform iconCurrencyTransform = default;

        #endregion



        #region Properties

        public Transform IconCurrencyTransform => iconCurrencyTransform;

        public RewardData RewardData { get; private set; }

        public bool IsClaimed { get; private set; }

        protected virtual List<BlendImage> BlendImages => new List<BlendImage> { icon };

        protected virtual List<TMP_Text> TextToAnimateMaterial => new List<TMP_Text> { textInfo };

        #endregion



        #region Methods

        public void SetupReward(RewardData rewardData)
        {
            RewardData = rewardData;

            MonopolyVisualSettings settings = IngameData.Settings.monopoly.visualSettings;

            foreach (var bi in BlendImages)
            {
                bi.CreateTextureComponent(settings.materialBlendAnimation);
                bi.BlendTextureComponent.Initialize();
            }

            Sprite activeSprite = GetRewardActiveSprite(RewardData);
            Texture activeTexture = activeSprite == null ? default : activeSprite.texture;

            icon.BlendTextureComponent.SetupTextures(activeTexture, GetRewardClaimedTexture(RewardData));
        }


        public virtual void SetClaimed(bool claimed, bool isImmediately)
        {
            IsClaimed = claimed;

            foreach (var bi in BlendImages)
            {
                if (claimed)
                {
                    bi.BlendTextureComponent.BlendToSecond(isImmediately);
                }
                else
                {
                    bi.BlendTextureComponent.BlendToFirst(isImmediately);
                }
            }

            ColorAnimation materialOutlineAnimation = IngameData.Settings.monopoly.visualSettings.materialTextOutlineAnimation;

            ColorAnimation materialTextAnimation = IngameData.Settings.monopoly.visualSettings.materialTextAnimation;

            List<TMP_Text> textToAnimateMaterial = TextToAnimateMaterial;

            if (isImmediately)
            {
                foreach (var text in textToAnimateMaterial)
                {
                    text.outlineColor = claimed ? materialOutlineAnimation.endValue : materialOutlineAnimation.beginValue;
                    text.color = claimed ? materialTextAnimation.endValue : materialTextAnimation.beginValue;
                }
            }
            else
            {
                materialOutlineAnimation.Play((value) =>
                {
                    foreach (var text in textToAnimateMaterial)
                    {
                        text.outlineColor = value;
                    }
                }, this, null, !claimed);

                materialTextAnimation.Play((value) =>
                {
                    foreach (var text in textToAnimateMaterial)
                    {
                        text.color = value;
                    }
                }, this, null, !claimed);
            }
        }


        public virtual void RefreshVisual()
        {
            string infoText = string.Empty;

            switch (RewardData)
            {
                case CurrencyReward currency:
                    infoText = currency.UiRewardText;
                    break;

                case WeaponSkinReward weapon:
                    infoText = string.Empty;
                    break;


                case ShooterSkinReward shooter:
                    infoText = string.Empty;
                    break;

                default:
                    CustomDebug.Log($"Not implemented logic for {RewardData}");
                    break;
            }

            icon.sprite = GetRewardActiveSprite(RewardData);
            textInfo.text = infoText;
            icon.SetNativeSize();

        }


        public void Deinitialize()
        {
            foreach (var bi in BlendImages)
            {
                bi.BlendTextureComponent.Deinitialize();
            }

            DOTween.Kill(this);
        }


        public abstract void ShowWinEffect();
        
        protected abstract Sprite GetRewardActiveSprite(RewardData data);

        protected abstract Texture GetRewardClaimedTexture(RewardData data);

        #endregion
    }
}
