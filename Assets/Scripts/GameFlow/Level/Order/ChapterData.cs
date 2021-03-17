using System;
using System.Collections.Generic;
using Drawmasters.LevelsRepository;
using Newtonsoft.Json;
using Sirenix.OdinInspector;


namespace Drawmasters.Levels.Order
{
    [Serializable]
    public class ChapterData
    {
        #region Fields
        
        public List<LevelData> levels = default;
        [DisableInPrefabs]
        public int index = default;

        #endregion



        #region Properties

        [JsonIgnore]
        public int Count
        {
            get
            {
                int count = default;

                levels.ForEach(d => count += d.sublevels.Count);

                return count;
            }

        }


        [JsonIgnore] // without boss and bonus levels 
        public List<string> CommonIds
        {
            get
            {
                List<string> ids = new List<string>(Count);
                
                for (int i = 0; i < levels.Count; i++)
                {
                    for (int j = 0; j < levels[i].sublevels.Count; j++)
                    {
                        string levelId = levels[i].sublevels[j];

                        #warning hotfix
                        LevelType levelType = LevelsContainer.GetLevelType(GameMode.Draw, levelId);
                        if (levelType.IsCommonLevel())
                        {
                            ids.Add(levelId);
                        }
                    }
                }

                return ids;
            }
        }

        #endregion



        #region Methods

        public ChapterData Copy()
        {
            List<LevelData> copiedLevels = new List<LevelData>(levels.Count);

            foreach(var oldLevel in levels)
            {
                copiedLevels.Add(oldLevel.Copy());
            }

            return new ChapterData() { levels = copiedLevels };
        }

        #endregion
    }
}

