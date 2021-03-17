using System;


namespace Drawmasters.Advertising
{
    public interface IBannerPlacement : IPlacement
    {
        event Action<IBannerPlacement> OnPlacementReady;
        event Action<IBannerPlacement> OnPlacementShouldHide;
    }
}

