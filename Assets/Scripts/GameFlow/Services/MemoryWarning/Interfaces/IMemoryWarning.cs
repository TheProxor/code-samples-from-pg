using System;

namespace Drawmasters.ServiceUtil.Interfaces
{
    public interface IMemoryWarning
    {
        event Action OnApplicationMemoryWarning;
    }
}