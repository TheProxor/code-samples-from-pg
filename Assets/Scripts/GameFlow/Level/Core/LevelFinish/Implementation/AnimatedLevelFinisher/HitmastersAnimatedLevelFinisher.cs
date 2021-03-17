using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.Levels
{
    public class HitmastersAnimatedLevelFinisher : AnimatedLevelFinisher
    {
        #region Fields

        private readonly IProposalService  proposalService;

        #endregion



        #region Ctor

        public HitmastersAnimatedLevelFinisher()
        {
            proposalService = GameServices.Instance.ProposalService;
        }

        #endregion



        #region Abstraction

        protected override void LoadNextLevelAction()
        {
            (GameMode mode, int index) = proposalService.HitmastersProposeController.CurrentModeAndIndexToPlay;

            LevelsManager.Instance.LoadLevel(mode, index);

            LevelsManager.Instance.PlayLevel();
        }

        #endregion
    }
}

