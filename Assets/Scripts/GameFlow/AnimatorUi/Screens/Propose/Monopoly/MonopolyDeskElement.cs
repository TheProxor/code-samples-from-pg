using System.Collections.Generic;
using Drawmasters.Utils.Ui;
using Drawmasters.Effects;
using UnityEngine;


namespace Drawmasters.Proposal
{
    public class MonopolyDeskElement : MonopolyElement
    {
        #region Nested types

        public enum ColorType
        {
            None = 0,
            Yellow = 1,
            Blue = 2
        }

        #endregion



        #region Fields

        [SerializeField] private ColorType type = default;
        [SerializeField] private RectTransform backUpRectTransform = default;
        [SerializeField] private BlendImage backUpBlendImage = default;
        [SerializeField] private BlendImage backDownBlendImage = default;

        private VectorAnimation bumpAnimation;

        #endregion



        #region Properties

        protected override List<BlendImage> BlendImages
        {
            get
            {
                List<BlendImage> result = base.BlendImages;

                BlendImage[] backBlendImages = { backUpBlendImage, backDownBlendImage };
                result.AddRange(backBlendImages);

                return result;
            }
        }

        #endregion



        #region Methods

        public void PlayBumpAnimation(bool isBumped, bool isImmediately)
        {
            if (bumpAnimation == null)
            {
                bumpAnimation = new VectorAnimation();
                bumpAnimation.SetupData(IngameData.Settings.monopoly.visualSettings.deskElementBumpAnimation);
            }

            if (isImmediately)
            {
                backUpRectTransform.anchoredPosition3D = isBumped ? bumpAnimation.endValue : bumpAnimation.beginValue;
            }
            else
            {
                bumpAnimation.Play((value) => backUpRectTransform.anchoredPosition3D = value, this, null, !isBumped);
            }
        }


        public override void ShowWinEffect() =>
            EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIMonopolyStep, 
                transform.position, 
                transform.rotation);


        public void ShowLapsFinishEffect() =>
            EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIMonopolyStepFullCycle, 
                transform.position, 
                transform.rotation);
        

        protected override Sprite GetRewardActiveSprite(RewardData data)
        {
            Sprite result = default;

            if (data is CurrencyReward currencyReward)
            {
                result = IngameData.Settings.monopoly.visualSettings.FindDeskCurrencyActiveIcon(type, currencyReward.currencyType);
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
                result = IngameData.Settings.monopoly.visualSettings.FindDeskCurrencyClaimedIcon(type, currencyReward.currencyType);
            }
            else
            {
                CustomDebug.Log($"Not implemented logic for {RewardData.Type}");
            }

            return result;
        }

        #endregion
    }
}
