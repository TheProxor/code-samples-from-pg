using System;


namespace Drawmasters
{
    public interface ILaserDestroyableCallback
    {
        event Action OnShouldStartLaserDestroy;
    }
}
