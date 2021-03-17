using System;


namespace Drawmasters.Proposal
{
    [Serializable]
    public class SpinRouletteCashReward : SpinRouletteReward
    {
        #region Properties

        public override RewardType Type =>
            RewardType.SpinRouletteCash;
        
        #endregion
    }
}
