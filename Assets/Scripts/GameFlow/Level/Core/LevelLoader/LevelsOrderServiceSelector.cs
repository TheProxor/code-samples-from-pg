using Drawmasters.ServiceUtil;


namespace Drawmasters.Levels
{
    public static class LevelsOrderServiceSelector
    {
        public static ILevelOrderService Select(GameMode gameMode)
        {        
            if (gameMode.IsHitmastersLiveOps())
            {
                return GameServices.Instance.ProposalService.HitmastersProposeController.LevelOrderService;
            }
            else
            {
                return GameServices.Instance.LevelOrderService;
            }            
        }
    }
}
