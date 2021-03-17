using System;


namespace Drawmasters.Interfaces
{
    public interface IGameLauncher 
    {
        void Launch(Action onLaunched);
    }
}

