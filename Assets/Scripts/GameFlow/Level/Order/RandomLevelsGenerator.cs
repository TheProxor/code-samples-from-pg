using System;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.LevelsRepository;
using Drawmasters.Ua;


namespace Drawmasters.Levels.Order
{
    public class RandomLevelsGenerator
    {
        #region Fields

        private readonly Dictionary<GameMode, List<ChapterData>> randomedData;

        #endregion



        #region Ctor

        public RandomLevelsGenerator()
        {
            if (CustomPlayerPrefs.HasKey(PrefsKeys.PlayerInfo.RandomedLevelsData))
            {
                randomedData = CustomPlayerPrefs.GetObjectValue<Dictionary<GameMode, List<ChapterData>>>(PrefsKeys.PlayerInfo.RandomedLevelsData);
            }
            else
            {
                randomedData = new Dictionary<GameMode, List<ChapterData>>();
            }
        }

        #endregion



        #region Methods

        public List<ChapterData> RandomedData(GameMode mode)
        {
            List<ChapterData> randomed = default;

            if (randomedData.TryGetValue(mode, out List<ChapterData> holdedRandomedData))
            {
                randomed = holdedRandomedData;
            }
            else
            {
                CustomDebug.Log("Not randomed levels yet. Mode: " + mode);
            }

            return randomed;
        }


        public List<ChapterData> RegenerateData(List<ChapterData> pool, GameMode forMode)
        {
            GenerateLevels(pool, forMode);

            return RandomedData(forMode);
        }


        public void GenerateLevels(List<ChapterData> sourceChapters, GameMode forMode)
        {
            List<ChapterData> randomedChapters = Shuffle(sourceChapters, forMode);

            if (randomedData.TryGetValue(forMode, out List<ChapterData> data))
            {
                randomedData[forMode] = randomedChapters;
            }
            else
            {
                randomedData.Add(forMode, randomedChapters);
            }

            CustomPlayerPrefs.SetObjectValue(PrefsKeys.PlayerInfo.RandomedLevelsData, randomedData);            
        }


        private static List<ChapterData> Shuffle(IReadOnlyCollection<ChapterData> source, GameMode forMode)
        {
            if (forMode == GameMode.UaLandscapeMode)
            {
                return new List<ChapterData>(source);
            }
            
            List<ChapterData> randomedChapters = new List<ChapterData>(source);

            // shuffle chapters order         
            randomedChapters = randomedChapters.OrderBy(c => Guid.NewGuid()).ToList();

            //shuffle levels order in chapter
            randomedChapters.ForEach(ShuffleLevelsInChapter);

            //shuffle sublevels
            var ids = randomedChapters.SelectMany(c => c.CommonIds).ToList();

            foreach(var chapter in randomedChapters)
            {
                for (int i = 0; i < chapter.levels.Count; i++)
                {
                    LevelData levelData = chapter.levels[i];

                    for (int j = 0; j < levelData.sublevels.Count; j++)
                    {
                        string previousId = levelData.sublevels[j];

                        LevelType levelType = LevelsContainer.GetLevelType(GameMode.Draw, previousId);
                        if (!levelType.IsCommonLevel())
                        {
                            continue;
                        }
                        
                        string id = ids.RandomObject();

                        ids.Remove(id);

                        levelData.sublevels[j] = id;
                    }
                }
            }

            return randomedChapters;
        }


        private static void ShuffleLevelsInChapter(ChapterData chapter)
        {
            chapter.levels.Sort((x, y) =>
            {
                if (IsCommonLevel(x) && IsCommonLevel(y))
                {
                    return UnityEngine.Random.Range(0, 2) * 2 - 1; // to return -1 or 1 
                }
                else
                {
                    return 0;
                }
            });
        }

        private static bool IsCommonLevel(LevelData levelData)
        {
            bool result = true;

            string levelId = levelData.sublevels[0];
            LevelType levelType = LevelsContainer.GetLevelType(GameMode.Draw, levelId);

            result = levelType.IsCommonLevel();
            
            return result;
        }

        #endregion
    }
}
