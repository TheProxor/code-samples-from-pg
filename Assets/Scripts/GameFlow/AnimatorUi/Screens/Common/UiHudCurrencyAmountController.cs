using DG.Tweening;
using Drawmasters.Levels;


namespace Drawmasters.Ui
{
    public class UiHudCurrencyAmountController : IInitializable, IDeinitializable
    {
        #region Fields

        private readonly UiHudTop uiHudTop;
        private readonly BonusLevelSettings bonusLevelSettings;

        #endregion



        #region Ctor

        public UiHudCurrencyAmountController(UiHudTop _uiHudTop)
        {
            uiHudTop = _uiHudTop;

            bonusLevelSettings = IngameData.Settings.bonusLevelSettings;
        }

        #endregion


        #region IInitializable

        public void Initialize()
        {
            RewardCollectComponent.OnRewardDropped += RewardCollectComponent_OnRewardDropped;
        }

        #endregion



        #region IDeinitializable

        public void Deinitialize()
        {
            RewardCollectComponent.OnRewardDropped -= RewardCollectComponent_OnRewardDropped;

            DOTween.Kill(this);
        }

        #endregion



        #region Events handlers

        private void RewardCollectComponent_OnRewardDropped(PhysicalLevelObject fromObject, BonusLevelObjectData bonusLevelObjectData)
        {
            if (bonusLevelObjectData.rewardType != Proposal.RewardType.Currency)
            {
                return;
            }

            CurrencyType type = bonusLevelObjectData.currencyType;

            DOTween.Sequence()
                .AppendInterval(bonusLevelSettings.delayBeforeCurrencyUpdate)
                .AppendCallback(() => uiHudTop.RefreshCertainCurrencyVisual(type, bonusLevelSettings.currencyUpdateDuration))
                .AppendInterval(bonusLevelSettings.currencyUpdateDuration)
                .OnComplete(() => uiHudTop.RefreshCertainCurrencyVisual(type, 0f))                
                .SetId(this);
        }

        #endregion
    }
}
