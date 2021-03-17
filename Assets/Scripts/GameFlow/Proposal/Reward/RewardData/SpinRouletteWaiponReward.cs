using System;


namespace Drawmasters.Proposal
{
    [Serializable]
    public class SpinRouletteWaiponReward : SpinRouletteReward
    {
        #region Properties

        public override RewardType Type =>
            RewardType.SpinRouletteWaipon;

        #endregion
    }
}
