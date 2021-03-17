using UnityEngine;
using Drawmasters.Proposal;
using SelectorKey = Drawmasters.Ui.UiLeagueLeaderBoardElementSelectorKey;


namespace Drawmasters.Ui
{
    public class UiLeagueLeaderBoardRewardsRootElement : UiLeagueLeaderBoardRewardRootBase
    {
        #region Fields

        protected bool isNextLeagueAchived;

        #endregion



        #region Properties

        protected override string IdleFxkey
        {
            get
            {
                string result = string.Empty;

                if (rewardData.Type == RewardType.ShooterSkin ||
                    RewardData.Type == RewardType.PetSkin)
                {
                    result = EffectKeys.FxGUILeagueShineIdle;
                }

                return result;
            }
        }

        #endregion



        #region Methods

        public void SetupIsNextLeagueAchived(bool _isNextLeagueAchived) =>
            isNextLeagueAchived = _isNextLeagueAchived;


        protected override (Sprite, float) GetRewardSpriteAndScaleMultiplier()
        {
            SelectorKey selectorKey = new SelectorKey(currentOwnerType, isNextLeagueAchived);
            Sprite rewardSprite = controller.VisualSettings.FindRewardIconBaseSprite(selectorKey, rewardData, out float scaleMultiplier);

            return (rewardSprite, scaleMultiplier);
        }

        #endregion
    }
}