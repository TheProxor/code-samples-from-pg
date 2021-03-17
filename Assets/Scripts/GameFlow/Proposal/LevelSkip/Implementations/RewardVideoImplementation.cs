using Modules.General.Abstraction;
using Drawmasters.Advertising;
using Drawmasters.Proposal.Interfaces;
using Drawmasters.Ui;
using System;
using Modules.Advertising;
using Modules.Analytics;

namespace Drawmasters.Proposal
{
    public class RewardVideoImplementation : IProposeImplementation
    {
        public void Propose(Action<bool> onProposed)
        {
            AdvertisingManager.Instance.TryShowAdByModule(AdModule.RewardedVideo,
                AdsVideoPlaceKeys.SkipLevel, result =>
            {
                bool isSuccessful = result == AdActionResultType.Success;

                switch (result)
                {
                    case AdActionResultType.NoInternet:
                        UiScreenManager.Instance.ShowScreen(ScreenType.NoInternet);
                        break;
                    case AdActionResultType.NotAvailable:
                        UiScreenManager.Instance.ShowPopup(OkPopupType.NoVideo);
                        break;
                }

                if (isSuccessful)
                {
                    CommonEvents.SendAdVideoReward(AdsVideoPlaceKeys.SkipLevel);
                }

                onProposed?.Invoke(isSuccessful);
            });
        }
    }
}
