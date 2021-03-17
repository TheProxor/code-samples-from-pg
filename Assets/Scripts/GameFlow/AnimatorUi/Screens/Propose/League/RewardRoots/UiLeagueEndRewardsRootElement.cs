using UnityEngine;
using SelectorKey = Drawmasters.Ui.UiLeagueLeaderBoardElementSelectorKey;


namespace Drawmasters.Ui
{
    public class UiLeagueEndRewardsRootElement : UiLeagueLeaderBoardRewardsRootElement
    {
        #region Methods

        protected override (Sprite, float) GetRewardSpriteAndScaleMultiplier()
        {
            SelectorKey selectorKey = new SelectorKey(currentOwnerType, isNextLeagueAchived);
            Sprite rewardSprite = controller.VisualSettings.FindRewardIconEndScreenSprite(selectorKey, rewardData, out float scaleMultiplier);

            return (rewardSprite, scaleMultiplier);
        }

        #endregion
    }
}