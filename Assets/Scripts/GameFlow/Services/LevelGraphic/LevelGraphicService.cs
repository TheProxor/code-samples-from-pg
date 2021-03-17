using System.Collections.Generic;
using Drawmasters.ServiceUtil.Interfaces;


namespace Drawmasters.ServiceUtil
{
    public class LevelGraphicService : ILevelGraphicService
    {
        #region Fields

        private readonly LevelGraphicSettings settings;

        private readonly List<LevelGraphicSettings.Data> data;

        #endregion



        #region Ctor

        public LevelGraphicService()
        {
            settings = IngameData.Settings.levelGraphicSettings;
            data = settings.graphicsData;
        }

        #endregion



        #region ILevelGraphicService

        public int GetLevelProfileIndex(GameMode mode, int chapterIndex, int levelIndex)
        {
            int result = default;

            LevelGraphicSettings.Data foundData = data.Find(i => i.mode == mode &&
                                                                 i.chapterIndex == chapterIndex);
            if (foundData == null)
            {
                CustomDebug.Log($"Cannot find profile data. Mode = {mode}, chapter = {chapterIndex}");

                result = default;
            }
            else
            {
                result = foundData.colorProfileIndex;
            }

            return result;
        }


        public bool IsLevelProfileExists(GameMode mode, int chapterIndex, int levelIndex)
        {
            LevelGraphicSettings.Data foundData = data.Find(i => i.mode == mode &&
                                                         i.chapterIndex == chapterIndex);
            return foundData != null;
        }


        public int RandomRewardProposalSceneIndex =>
            settings.RandomForcemeterRewardIndex;

        #endregion
    }
}
