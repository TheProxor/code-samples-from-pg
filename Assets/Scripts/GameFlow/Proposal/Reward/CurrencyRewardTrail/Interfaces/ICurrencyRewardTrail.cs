using System;
using Drawmasters;
using Drawmasters.Proposal;
using UnityEngine;

namespace Drawmasters.Interfaces
{
    public interface ICurrencyRewardTrail : IDeinitializable
    {
        void PlayTrailFx(CurrencyReward currencyReward, Vector3 startPosition, Action callback = null);
        
        void PlayTrailFx(CurrencyType currencyRewardType, Vector3 startPosition, Action callback = null);
    }
}