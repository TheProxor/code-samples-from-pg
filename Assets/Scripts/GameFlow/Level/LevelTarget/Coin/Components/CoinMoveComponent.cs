using DG.Tweening;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Levels
{
    public class CoinMoveComponent : LevelObjectComponentTemplate<CurrencyLevelObject>
    {
        #region Fields

        private readonly CoinLevelObjectSettings settings;
        private readonly CurrencyObjectsLevelController currencyObjectsLevelController;

        #endregion



        #region Ctor

        public CoinMoveComponent()
        {
            settings = IngameData.Settings.coinLevelObjectSettings;
            currencyObjectsLevelController = GameServices.Instance.LevelControllerService.CurrencyObjectsLevelController;
        }

        #endregion



        #region Methods

        public override void Enable()
        {
            currencyObjectsLevelController.OnShouldAbsorbObject += CurrencyObjectsLevelController_OnShouldAbsorbObject;
        }


        public override void Disable()
        {
            currencyObjectsLevelController.OnShouldAbsorbObject -= CurrencyObjectsLevelController_OnShouldAbsorbObject;

            DOTween.Kill(this);
        }

        #endregion



        #region Events handlers

        private void CurrencyObjectsLevelController_OnShouldAbsorbObject(CurrencyLevelObject anotherCurrency, ICurrencyAbsorber absorber)
        {
            if (levelObject != anotherCurrency)
            {
                return;
            }

            settings.absorbAnimation.beginValue = levelObject.transform.position;
            settings.absorbAnimation.endValue = absorber.TargetAbsorbTransform.position;

            settings.absorbAnimation.Play(e => levelObject.transform.position = e, this);
        }

        #endregion
    }
}
