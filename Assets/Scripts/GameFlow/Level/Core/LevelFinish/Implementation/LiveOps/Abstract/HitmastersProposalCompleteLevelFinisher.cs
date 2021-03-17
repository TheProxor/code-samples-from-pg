using System;
using Modules.Advertising;
using Modules.General;
using Modules.General.Abstraction;

namespace Drawmasters.Levels
{
    public class HitmastersProposalCompleteLevelFinisher : SucceedLevelFinisher
    {
        public override void FinishLevel(Action _onFinished)
        {
            base.FinishLevel(_onFinished);
            
            AdvertisingManager.Instance.TryShowAdByModule(AdModule.Interstitial, 
                AdPlacementType.BeforeResult, 
                result => 
                    Scheduler.Instance.CallMethodWithDelay(this, OnScreenHided, CommonUtility.OneFrameDelay));
        }
    }
}