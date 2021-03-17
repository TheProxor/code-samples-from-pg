using System;


namespace Drawmasters.Levels
{
    public interface ILevelFinisher
    {
        void FinishLevel(Action onFinished);
    }
}
