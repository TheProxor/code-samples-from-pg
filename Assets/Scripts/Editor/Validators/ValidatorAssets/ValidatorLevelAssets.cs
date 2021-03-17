using System;
using System.Collections.Generic;
using Drawmasters.Levels.Order;
using Drawmasters.LevelsRepository;
using UnityEngine;


namespace Drawmasters.Editor
{
    public class ValidatorLevelAssets : ValidatorAssets
    {
        #region Properties

        protected override string AssetName => string.Concat("Type of ",nameof(LevelsOrder)); 

        #endregion



        #region Protected Methods

        protected override bool ValidateAsset(string name)
        {
            LevelsContainer.LoadData();

            List<ModeData> dataToValidate = new List<ModeData>();

            LevelsOrder.AbTestReplaceData[] abTestReplaceData = Array.Empty<LevelsOrder.AbTestReplaceData>();

            dataToValidate.AddRange(IngameData.Settings.levelsOrder.LoadModesData(abTestReplaceData));
            dataToValidate.AddRange(IngameData.Settings.levelsOrder.LoadAbTestModesData(abTestReplaceData));
            dataToValidate.AddRange(IngameData.Settings.hitmasters.liveOpsLevelsOrder.LoadModesData(abTestReplaceData));
            dataToValidate.AddRange(IngameData.Settings.hitmasters.liveOpsLevelsOrder.LoadAbTestModesData(abTestReplaceData));

            return ValidateModeData(dataToValidate.ToArray());
        }

        #endregion



        #region Private Methods

        private bool ValidateModeData(ModeData[] modesData)
        {
            if (modesData == null || modesData.Length == 0)
            {
                Debug.LogError($"Array modes is null or empty");
                return false;
            }
            
            bool result = true;
            
            foreach (ModeData modeData in modesData)
            {
                ValidateChapters(modeData.mode, modeData.chapters, err =>
                {
                    result = false;
                    
                    Debug.LogError($"Mode={modeData.mode} {err}");
                });
                
            }
            return result;
        }

        
        private static void ValidateChapters(GameMode mode, List<ChapterData> chapters, Action<string> onErrorCallback)
        {
            foreach (ChapterData chapter in chapters)
            {
                if (chapter.levels == null || chapter.levels.Count == 0)
                {
                    onErrorCallback?.Invoke($"Chapter index={chapter.index} levels is null or empty");
                    
                    continue;
                }

                ValidateLevel(mode, chapter.levels, (err) =>
                {
                    onErrorCallback?.Invoke($"Chapter Index={chapter.index} {err}");
                });
            }
        }

        
        private static void ValidateLevel(GameMode mode, List<LevelData> levels, Action<string> onErrorCallback)
        {
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].sublevels == null || levels[i].sublevels.Count == 0)
                {
                    onErrorCallback?.Invoke($"Level index={i} sublevels is null or empty");
                    continue;
                }
                for (int j = 0; j < levels[i].sublevels.Count; j++)
                {
                    if (string.IsNullOrEmpty(levels[i].sublevels[j]))
                    {
                        onErrorCallback?.Invoke($"Level index={i} sublevel index={j} is null or empty");
                        continue;
                    }

                    CheckSublevelResource(mode, levels[i].sublevels[j], (err) =>
                    {
                        onErrorCallback?.Invoke($"Level index={i} sublevel index={j} {err}");
                    });
                }
            }
        }


        private static void CheckSublevelResource(GameMode mode, string sublevel, Action<string> onErrorCallback)
        {
            LevelHeader header = LevelsContainer.GetHeader(mode, sublevel);

            // It's ok. "1" means that level is placeholder for proposes
            if (mode.IsHitmastersLiveOps() && sublevel.Equals("1"))
            {
                return;
            }

            if (header == null)
            {
                onErrorCallback?.Invoke($"header not found for sublevel={sublevel}");
                return;
            }
            
            if (header.isDisabled)
            {
                onErrorCallback?.Invoke($"header is disabledfor sublevel={sublevel}");
                return;
            }

            Levels.Level.Data data = LevelsContainer.GetLevelData(header);
            
            if (data == null)
            {
                onErrorCallback?.Invoke($"data not found for sublevel={sublevel}");
                return;
            }

            if (data.levelObjectsData == null || data.levelObjectsData.Count == 0)
            {
                onErrorCallback?.Invoke($"data is null or empty for sublevel={sublevel}");
            }
        }
        
        #endregion
    }
}