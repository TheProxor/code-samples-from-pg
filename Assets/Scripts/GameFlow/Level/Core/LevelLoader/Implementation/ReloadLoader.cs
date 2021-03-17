using System;

namespace Drawmasters.Levels
{
    public class ReloadLoader : ILevelLoader
    {
        public void LoadLevel(Action onLoaded) => onLoaded?.Invoke();
    }
}

