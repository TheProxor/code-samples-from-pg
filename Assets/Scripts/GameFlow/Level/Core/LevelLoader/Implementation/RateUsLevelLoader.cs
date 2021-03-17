using System;
using Drawmasters.ServiceUtil;
using Drawmasters.Ui;


namespace Drawmasters.Levels
{
    public class RateUsLevelLoader : ILevelLoader
    {
        #region ILevelLoader

        public void LoadLevel(Action onLoaded)
        {
            // TODO: hot fix.Vladislav.k
            bool isBossLevel = GameServices.Instance.LevelEnvironment.Context.IsBossLevel;

            if (!isBossLevel)
            {
                TouchManager.Instance.IsEnabled = false;
            }

            GameServices.Instance.ProposalService.RateUsProposal.Propose(() =>
            {
                if (!isBossLevel)
                {
                    TouchManager.Instance.IsEnabled = true;
                }

                UiScreenManager.Instance.ShowScreen(ScreenType.Ingame, view => onLoaded?.Invoke());
            });
        }

        #endregion
    }
}

