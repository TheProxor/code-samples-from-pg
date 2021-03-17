
using System.Collections.Generic;


namespace Drawmasters.Levels.Order
{
    public class DataLoader : IAnyLoader<List<SublevelData>>
    {
        #region Fields

        private readonly List<ChapterData> source;
        private readonly int overflowsCount;
        private readonly List<string> uaLevelNames;

        #endregion



        #region IAnyLoader

        public bool IsLoaded => LoadedObject != null;

        public List<SublevelData> LoadedObject { get; private set; }

        public void Load()
        {
            LoadedObject = new List<SublevelData>();

            LoadChapters(source);
        }

        #endregion



        #region Ctor

        public DataLoader(List<ChapterData> _source, int _overflowsCount, List<string> _uaLevelNames)
        {
            source = _source;
            overflowsCount = _overflowsCount;
            uaLevelNames = _uaLevelNames;
        }

        #endregion



        #region Private methods

        private void LoadChapters(List<ChapterData> chapters)
        {
            for (int i = 0; i < chapters.Count; i++)
            {
                ChapterData chapter = chapters[i];

                int prevLevelsCount = default;
                for (int j = 0; j < i; j++)
                {
                    prevLevelsCount += chapters[j].levels.Count;
                }
                
                LoadLevels(chapter.index, chapter.levels, prevLevelsCount);
            }
        }


        private void LoadLevels(int chapterIndex, List<LevelData> levelsData, int prevLevelsCount)
        {
            int levelsCount = levelsData.Count;
            int levelsDelta = levelsCount * overflowsCount + prevLevelsCount;

            for (int i = 0; i < levelsCount; i++)
            {
                int levelIndex = i;
                bool isLastLevel = (levelIndex == levelsCount - 1);

                int levelInfoIndex = levelIndex;

                List<string> ids = levelsData[levelInfoIndex].sublevels;

                if (i == levelsCount - 1 &&
                    chapterIndex == source.Count - 1 &&
                    BuildInfo.IsUaBuild)
                {
                    ids.AddRange(uaLevelNames);
                }

                LoadSublevels(chapterIndex, i, i + levelsDelta, isLastLevel, ids);
            }
        }


        private void LoadSublevels(int chapterIndex,
                                          int levelIndex,
                                          int levelGlobalIndex,
                                          bool isLastLevel,
                                          List<string> sublevelsIdPool)
        {
            for (int i = 0; i < sublevelsIdPool.Count; i++)
            {
                string id = sublevelsIdPool[i];

                LoadSublevel(chapterIndex,
                             levelIndex,
                             levelGlobalIndex,
                             i,
                             isLastLevel,
                             sublevelsIdPool.Count,
                             id);
            }
        }


        private void LoadSublevel(int chapterIndex,
                                  int levelIndex,
                                  int levelGlobalIndex,
                                  int sublevelIndex,
                                  bool isLastLevel,
                                  int sublevelsInLevel,
                                  string id)
        {
            bool isLastSublevel = (sublevelIndex == sublevelsInLevel - 1);
            bool isFinalChapterSublevel = isLastSublevel && isLastLevel;

            LoadedObject.Add(new SublevelData(id,
                                              chapterIndex,
                                              levelIndex,  
                                              levelGlobalIndex,
                                              LoadedObject.Count,
                                              sublevelIndex,
                                              sublevelsInLevel,
                                              isLastSublevel,
                                              isFinalChapterSublevel));
        }

        #endregion
    }
}
