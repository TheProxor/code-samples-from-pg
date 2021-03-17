using System;


namespace Drawmasters.Levels
{
    public interface ILevelTimer
    {
        event Action OnTimeLeft;

        float Time { get; }

        int SecondsLeft { get; }

        void StartTimer();

        void PauseTimer();

        void UnpauseTimer();

        void AddSeconds(float addedSeconds);
    }
}
