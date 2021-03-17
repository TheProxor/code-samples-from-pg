using System;
using Drawmasters.Ui;


namespace Drawmasters.Levels
{
    public class HitmastersLeaveLevelFinisher : ILevelFinisher
    {
        public void FinishLevel(Action onFinished)
        {
            onFinished?.Invoke();

            UiScreenManager.Instance.ShowScreen(ScreenType.HitmastersMap, isForceHideIfExist: true);
        }
    }
}