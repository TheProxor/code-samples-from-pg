using System;
using Drawmasters.Effects;
using Drawmasters.ServiceUtil;
using Drawmasters.Statistics.Data;
using Drawmasters.Levels.Data;
using Drawmasters.Proposal;
using DG.Tweening;
using Modules.Sound;
using Modules.General;
using CurrencyData = Drawmasters.Ui.ResultCommonCompleteBehaviour.CurrencyData;


namespace Drawmasters.Ui
{
    public class UiResultHappyHoursCurrencyHelper
    {
        #region Fields

        private readonly CurrencyData[] currencyData;
        private readonly string sfxKey;

        #endregion



        #region Class lifecycle

        public UiResultHappyHoursCurrencyHelper(CurrencyData[] _currencyData, string _sfxKey)
        {
            currencyData = _currencyData;
            sfxKey = _sfxKey;
        }

        #endregion



        #region Methods

        public void Deinitialize()
        {
            DOTween.Kill(this);
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }


        public void PlayAnimation(LiveOpsEventController controller, HappyHoursVisualSettings visualSettings, CurrencyType currencyType)
        {
            CurrencyData foundData = Array.Find(currencyData, e => e.currencyType == currencyType);
            LevelProgress progress = GameServices.Instance.LevelEnvironment.Progress;
            LevelContext context = GameServices.Instance.LevelEnvironment.Context;

            if (foundData == null ||
                !controller.WasActiveBeforeLevelStart ||
                !progress.ShouldShowCurrencyOnResult(currencyType))
            {
                return;
            }

            NumberAnimation numberAnimation = visualSettings.numberAnimation;

            float beginValue = progress.TotalCurrencyPerLevelEndWithoutBonus(currencyType);
            numberAnimation.SetupBeginValue(beginValue);

            float endValue = progress.TotalCurrencyPerLevelEnd(currencyType);
            numberAnimation.SetupEndValue(endValue);

            foundData.currencyCounter.text = progress.ToUiView(CurrencyTypeExtensions.ToIntCurrency(beginValue));
            numberAnimation.Play((value) => foundData.currencyCounter.text = progress.ToUiView(CurrencyTypeExtensions.ToIntCurrency(value)), this);

            visualSettings.colorAnimation.Play((value) =>
            {
                foreach (var g in foundData.graphics)
                {
                    g.color = value;
                }
            }, this);

            visualSettings.bounceInAnimation.Play((value) => foundData.showRoot.transform.localScale = value, this, () =>
                visualSettings.bounceOutAnimation.Play((value) => foundData.showRoot.transform.localScale = value, this));

            EffectManager.Instance.PlaySystemOnce(EffectKeys.FxGUIHappyHoursResult, default, default, foundData.showRoot.transform, transformMode: TransformMode.Local);
            Scheduler.Instance.CallMethodWithDelay(this, () => SoundManager.Instance.PlayOneShot(sfxKey), visualSettings.soundDelay);
        }

        #endregion
    }
}
