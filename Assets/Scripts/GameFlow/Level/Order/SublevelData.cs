namespace Drawmasters.Levels.Order
{
    public struct SublevelData
    {
        #region Properties

        public string NameId { get; }

        public int ChapterIndex { get; }

        public int LevelLocalIndex { get; }

        public int LevelGlobalIndex { get; }

        public int SublevelLocalIndex { get; private set; }

        public int SublevelsCount { get; private set; }

        public bool IsEndOfLevel { get; private set; }

        public bool IsEndOfChapter { get; private set; }

        #endregion



        #region Ctor

        public SublevelData(string nameId,
                            int chapterIndex,
                            int levelLocalIndex,
                            int levelGlobalIndex,
                            int sublevelGlobalIndex,
                            int sublevelLocalIndex,
                            int sublevelsCount,
                            bool isEndOfLevel,
                            bool isEndOfChapter)
        {
            NameId = nameId;
            ChapterIndex = chapterIndex;
            LevelLocalIndex = levelLocalIndex;
            LevelGlobalIndex = levelGlobalIndex;
            SublevelLocalIndex = sublevelLocalIndex;
            SublevelsCount = sublevelsCount;
            IsEndOfLevel = isEndOfLevel;
            IsEndOfChapter = isEndOfChapter;
        }

        #endregion
    }
}