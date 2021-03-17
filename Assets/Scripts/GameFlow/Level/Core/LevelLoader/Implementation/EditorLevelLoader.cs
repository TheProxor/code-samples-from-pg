using System;


namespace Drawmasters.Levels
{
    public class EditorLevelLoader : ILevelLoader
    {
        public void LoadLevel(Action onLoaded) => onLoaded?.Invoke();
    }
}