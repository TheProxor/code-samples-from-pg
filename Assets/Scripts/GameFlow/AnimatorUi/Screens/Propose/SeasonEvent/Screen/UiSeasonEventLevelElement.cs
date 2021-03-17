using UnityEngine;
using TMPro;
using Drawmasters.Effects;
using Drawmasters.Utils.Ui;
using DG.Tweening;


namespace Drawmasters.Proposal
{
    public class UiSeasonEventLevelElement : MonoBehaviour
    {
        #region Fields

        [SerializeField] private BlendImage levelBackImage = default;
        [SerializeField] private BlendImage iconImage = default;
        [SerializeField] private TMP_Text levelNumberText = default;
        [SerializeField] private GameObject separator = default;

        private BlendImage[] blendImages;

        private int levelIndex;

        #endregion



        #region Methods

        public void SetupNumber(int _levelIndex, bool shouldShowSeparator)
        {
            levelIndex = _levelIndex;

            levelNumberText.text = levelIndex.ToString();

            bool shouldShowText = levelIndex > 0;
            CommonUtility.SetObjectActive(levelNumberText.gameObject, shouldShowText);
            CommonUtility.SetObjectActive(iconImage.gameObject, !shouldShowText);

            CommonUtility.SetObjectActive(separator, shouldShowSeparator);
        }


        public void Initialize()
        {
            SeasonEventVisualSettings settings = IngameData.Settings.seasonEvent.seasonEventVisualSettings;

            levelBackImage.sprite = settings.FindLevelElementSprite(levelIndex);

            Color outlineColor = settings.FindLevelElementOutlineColor(levelIndex);
            levelNumberText.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, outlineColor);

            blendImages = new BlendImage[] { levelBackImage, iconImage };
            FactorAnimation blendAnimation = settings.materialBlendAnimation;

            foreach (var blendImage in blendImages)
            {
                blendImage.CreateTextureComponent(blendAnimation);
                blendImage.BlendTextureComponent.Initialize();
            }

            levelBackImage.BlendTextureComponent.SetupFirstTexture(levelBackImage.sprite.texture);
        }


        public void Deinitialize()
        {
            foreach (var blendImage in blendImages)
            {
                blendImage.BlendTextureComponent.Deinitialize();
            }
        }


        public void SetState(bool isReached, bool isImmediately)
        {
            foreach (var blendImage in blendImages)
            {
                blendImage.BlendTextureComponent.BlendToFirst(true);
            }
        }


        public void PlayShineAnimation() =>
                EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUISeasonPassNumberRhombBarShine, transform.position, transform.rotation, transform);

        #endregion
    }
}
