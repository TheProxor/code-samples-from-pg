using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Levels.Order
{
    [CreateAssetMenu(fileName = "LevelsOrder",
                     menuName = NamingUtility.MenuItems.IngameSettings + "LevelsOrder")]
    public class LevelsOrder : ScriptableObject
    {
        #region Nested types

        [SerializeField]
        public class AbTestReplaceData
        {
            public int sublevelIndexToReplace = default;
            public string sublevelTargetName = default;
        }

        #endregion



        #region Fields

        [SerializeField] private ModeData[] modesData = default;

        [Header("A/B Test")]
        [SerializeField] private ModeData[] abTestModesData = default;

        [SerializeField] private bool allowLoadAbTestData = default;

        #endregion



        #region Methods

        public ModeData[] LoadModesData(AbTestReplaceData[] abTestReplaceData) =>
            LoadData(modesData, abTestReplaceData);


        public ModeData[] LoadAbTestModesData(AbTestReplaceData[] abTestReplaceData)
        {
            ModeData[] baseData = allowLoadAbTestData ? abTestModesData : modesData;
            return LoadData(baseData, abTestReplaceData);
        }


        private ModeData[] LoadData(ModeData[] baseData, AbTestReplaceData[] abTestReplaceData)
        {
            ModeData[] result = new List<ModeData>(baseData).ToArray();

            if (abTestReplaceData == null || abTestReplaceData.Length == 0)
            {
                return result;
            }

            int sublevelIndex = 0;

            // TODO: Actually, it's better to test perfomance with common loop and with "Find()" for each abTestReplaceData element. Vladislav.k
            foreach (var modeData in result)
            {
                foreach (var chapterData in modeData.chapters)
                {
                    foreach (var levelData in chapterData.levels)
                    {
                        for (int i = 0; i < levelData.sublevels.Count; i++)
                        {
                            AbTestReplaceData replaceData = Array.Find(abTestReplaceData, e => e.sublevelIndexToReplace == sublevelIndex);

                            bool shouldReplaceSublevel = replaceData != null;
                            if (shouldReplaceSublevel)
                            {
                                levelData.sublevels[i] = replaceData.sublevelTargetName;
                            }

                            sublevelIndex++;
                        }
                    }
                }
            }

            return result;
        }

        #endregion



        #region Editor

        [Sirenix.OdinInspector.Button]
        private void CreateJson() => Debug.Log(JsonConvert.SerializeObject(modesData));


        [Sirenix.OdinInspector.Button]
        private void TestLoadMode()
        {
            var loadedModesData = LoadModesData(GameServices.Instance.AbTestService.CommonData.abTestSublevelsReplaceData);

            int sublevelIndex = default;
            foreach (var m in loadedModesData)
            {
                foreach (var chapterData in m.chapters)
                {
                    foreach (var levelData in chapterData.levels)
                    {
                        for (int i = 0; i < levelData.sublevels.Count; i++)
                        {
                            Debug.Log($" Index: {sublevelIndex} | SublevelName: {levelData.sublevels[i]}");
                            sublevelIndex++;
                        }
                    }
                }
            }

        }

        private void OnValidate()
        {
            foreach (var i in modesData)
            {
                i.CustomValidate();
            }

            foreach (var i in abTestModesData)
            {
                i.CustomValidate();
            }
        }

        #endregion
    }
}
