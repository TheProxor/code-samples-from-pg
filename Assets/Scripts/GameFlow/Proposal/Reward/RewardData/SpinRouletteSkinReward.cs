using System;


namespace Drawmasters.Proposal
{
    [Serializable]
    public class SpinRouletteSkinReward : SpinRouletteReward
    {
        #region Properties

        public override RewardType Type =>
            RewardType.SpinRouletteSkin;
        
        #endregion
    }
}
