using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.Levels
{
    public class CommonAnimatedLevelFinisher : AnimatedLevelFinisher
    {
        #region Fields

        private readonly IPlayerStatisticService playerStatistic;

        #endregion



        #region Ctor

        public CommonAnimatedLevelFinisher()
        {
            playerStatistic = GameServices.Instance.PlayerStatisticService;
        }

        #endregion



        #region Abstraction

        protected override void LoadNextLevelAction()
        {
            GameMode mode = playerStatistic.PlayerData.LastPlayedMode;
            int index = mode.GetCurrentLevelIndex();

            LevelsManager.Instance.LoadLevel(mode, index);
            LevelsManager.Instance.PlayLevel();
        }

        #endregion
    }
}

