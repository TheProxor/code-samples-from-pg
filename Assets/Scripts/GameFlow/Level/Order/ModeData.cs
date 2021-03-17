using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Drawmasters.Levels.Order
{
    [Serializable]
    public class ModeData
    {
        #region Fields

        public const int MaxLevelsInChapter = 20;

        public GameMode mode = default;
        public List<ChapterData> chapters = default;
        
        [Header("Специальные уровни для UA. Добавятся подуровнями в последний уровень")]
        public List<string> specialUaLevels = default;

        [NonSerialized]
        [JsonIgnore]
        private int? sublevelsCount = null;

        [NonSerialized]
        [JsonIgnore]
        private List<string> ids;

        #endregion



        #region Properties

        [JsonIgnore]
        public int SublevelsCount
        {
            get
            {
                if (sublevelsCount == null)
                {
                    sublevelsCount = default(int);

                    chapters.ForEach(c => c.levels.ForEach(l => sublevelsCount += l.sublevels.Count));

                    if (BuildInfo.IsUaBuild)
                    {
                        sublevelsCount += specialUaLevels.Count;
                    }
                }

                return sublevelsCount.Value;
            }
        }

        #endregion



        #region Methods

        public ModeData Copy()
        {
            List<ChapterData> copiedChapters = new List<ChapterData>(chapters.Count);

            foreach(var c in chapters)
            {
                copiedChapters.Add(c.Copy());
            }

            return new ModeData { mode = mode, chapters = copiedChapters };
        }

        #endregion



        #region Editor

        public void CustomValidate()
        {
            int wasteElements = chapters.Count - MaxLevelsInChapter;
            if (wasteElements > 0)
            {
                chapters.RemoveRange(MaxLevelsInChapter, wasteElements);
            }

            for (int i = 0; i < chapters.Count; i++)
            {
                chapters[i].index = i;
            }
        }

        #endregion
    }
}
