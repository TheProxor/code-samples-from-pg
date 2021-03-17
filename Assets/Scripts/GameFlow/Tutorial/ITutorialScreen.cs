using System;


namespace Drawmasters.Tutorial
{
    public interface ITutorialScreen
    {
        void Initialize(TutorialType type, Action _completeTutorialCallback);
    }
}