using System;

namespace Drawmasters.Levels
{
    public interface ILevelLoader
    {
        void LoadLevel(Action onLoaded);
    }
}
