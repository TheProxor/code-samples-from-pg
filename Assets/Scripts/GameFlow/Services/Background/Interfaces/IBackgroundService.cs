using System;


namespace Drawmasters.ServiceUtil.Interfaces
{
    public interface IBackgroundService
    {
        event Action<bool, TimeSpan> OnApplicationSuspend;
    }
}

