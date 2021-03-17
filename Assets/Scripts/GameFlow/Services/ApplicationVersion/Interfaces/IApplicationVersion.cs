using System;


namespace Drawmasters.ServiceUtil.Interfaces
{
    public interface IApplicationVersion
    {
        event Action<string> OnApplicationVersionChanged;
    }
}