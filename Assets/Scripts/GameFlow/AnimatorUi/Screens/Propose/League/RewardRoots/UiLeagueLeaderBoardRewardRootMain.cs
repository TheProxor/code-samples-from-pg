using DG.Tweening;
using Drawmasters.Proposal;
using I2.Loc;
using UnityEngine;
using SelectorKey = Drawmasters.Ui.UiLeagueLeaderBoardElementSelectorKey;


namespace Drawmasters.Ui
{
    public class UiLeagueLeaderBoardRewardRootMain : UiLeagueLeaderBoardRewardRootBase
    {
        #region Fields

        [SerializeField] private RectTransform rectTransform = default;
        [SerializeField] private Localize rewardDescription = default;

        [SerializeField] private CanvasGroup canvasGroup = default;
        [SerializeField] private UiLeagueLeaderBoardElementSelectorColor[] selectorColors = default;

        #endregion



        #region Properties

        public RectTransform RectTransform =>
            rectTransform;

        #endregion



        #region Methods

        public override void Deintiailize()
        {
            DOTween.Kill(this);

            base.Deintiailize();
        }


        public void PlayGraphicChangeAnimation()
        {
            controller.VisualSettings.mainRewardFadeAnimation.Play(e => canvasGroup.alpha = e, this, () =>
            {
                RefreshVisual();
                controller.VisualSettings.mainRewardFadeAnimation.Play(e => canvasGroup.alpha = e, this, isReversed: false);
            }, true);
        }


        protected override void OnRefreshVisual()
        {
            base.OnRefreshVisual();

            string term = rewardData.Type == RewardType.PetSkin ?
                controller.VisualSettings.petRewardDescriptionKey : controller.VisualSettings.gemRewardDescriptionKey;
            rewardDescription.SetTerm(term);

            SelectorKey selectorKey = new SelectorKey(currentOwnerType, true);

            foreach (var selector in selectorColors)
            {
                selector.Select(selectorKey);
            }
        }


        protected override (Sprite, float) GetRewardSpriteAndScaleMultiplier()
        {
            SelectorKey selectorKey = new SelectorKey(LeaderBordItemType.Player, true);

            (Sprite, float) result;

            if (rewardData is CurrencyReward currencyReward &&
                     currencyReward.currencyType == CurrencyType.Premium)
            {
                result.Item1 = controller.VisualSettings.gemsMainRewardSprite;
                result.Item2 = 0.8f;
            }
            else
            {
                result.Item1 = controller.VisualSettings.FindRewardIconBaseSprite(selectorKey, rewardData, out float scaleMultiplier);
                result.Item2 = scaleMultiplier;
            }

            return result;
        }

        #endregion
    }
}