using System;
using Drawmasters.Proposal;
using Drawmasters.Ui;

namespace Drawmasters
{
    public class MonopolyCurrencyRewardApplyHelper : CommonCurrencyRewardApplyHelper
    {
        protected readonly Func<CurrencyReward, bool> canPlayTrailFunc;

        public MonopolyCurrencyRewardApplyHelper(RewardReceiveScreen withScreen, 
            Func<CurrencyReward, bool> canPlayTrail) : 
            base(withScreen)
        {
            canPlayTrailFunc = canPlayTrail;
        }


        protected override bool CanPlayTrail(CurrencyReward reward) =>
            canPlayTrailFunc?.Invoke(reward) ?? false;
    }
}