using System;


namespace Drawmasters.Levels
{
    public class HitmastersCompleteLevelFinisher : SucceedLevelFinisher
    {
        #region Overrided methods

        public override void FinishLevel(Action _onFinished)
        {
            base.FinishLevel(_onFinished);

            ShowResult();
        }

        #endregion
    }
}
