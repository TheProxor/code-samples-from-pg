using Drawmasters.Prefs;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.Statistics.Data
{
    public class PlayerChaptersData
    {
        #region Fields

        private readonly ChaptersHolder chapterHolder;        
        private readonly PlayerModesData playerModes;
        private readonly ILevelEnvironment levelEnvironment;

        #endregion



        #region Ctor

        public PlayerChaptersData(PlayerModesData _playersModes,
                                  ILevelEnvironment _levelEnvironment)
        {
            chapterHolder = new ChaptersHolder(PrefsKeys.PlayerInfo.ChaptersInfo);

            playerModes = _playersModes;

            levelEnvironment = _levelEnvironment;
            
            //TODO
            CheckOpenModes();
        }

        #endregion



        #region Private methods               

        private void CheckOpenModes()
        {
            foreach (var m in playerModes.AllModes)
            {
                playerModes.OpenMode(m);
            }
        }

        #endregion
    }
}

