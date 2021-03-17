using I2.Loc;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Drawmasters.Helpers;

namespace Drawmasters.Ui
{
    public class MonopolyLapsBar : MonoBehaviour, IInitializable, IDeinitializable
    {
        [SerializeField] private RectTransform scaleLapsNumberRoot = default;
        [SerializeField] private Localize lapsNumberText = default;

        [SerializeField] private Image barFillImage = default;

        private float lapsFillDuration;
        private VectorAnimation lapsNumberScaleAnimation;


        public void Initialize()
        {
            lapsNumberScaleAnimation = new VectorAnimation();
            lapsNumberScaleAnimation.SetupData(IngameData.Settings.monopoly.visualSettings.laspScaleAnimation);

            lapsFillDuration = IngameData.Settings.monopoly.visualSettings.lapsFillDuration;
        }


        public void Deinitialize()
        {
            DOTween.Kill(this);
        }


        public void PlayAnimation(int lapsNumber, bool isImmediately)
        {
            float fillValue = IngameData.Settings.monopoly.settings.DescMovementsForLaps == 0 ?
                0 : (float)lapsNumber / IngameData.Settings.monopoly.settings.CountsLapsForReward.LastObject();

            if (isImmediately)
            {
                barFillImage.fillAmount = fillValue;
                RefreshText(lapsNumber);
            }
            else
            {
                lapsNumberScaleAnimation.Play((value) => scaleLapsNumberRoot.localScale = value, 
                    this,
                    () =>
                    {
                        lapsNumberScaleAnimation.Play(value => 
                            scaleLapsNumberRoot.localScale = value, this, null, true);
                        RefreshText(lapsNumber);
                    });

                barFillImage
                    .DOFillAmount(fillValue, lapsFillDuration)
                    .SetId(this);
            }
        }


        private void RefreshText(int lapsNumber)
        {
            lapsNumberText.SetStringParams(lapsNumber, IngameData.Settings.monopoly.settings.CountsLapsForReward.LastObject());
        }
    }
}