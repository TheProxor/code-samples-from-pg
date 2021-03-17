using UnityEngine;


namespace Drawmasters.LevelsRepository
{
    public class LevelBody : ScriptableObject
    {
        #region Fields

        public Levels.Level.Data data = default;

        #endregion



        #region Editor


        [Sirenix.OdinInspector.Button]
        public void LeaveOnlyMonolith(int monolithLevelObjectIndex = 21)
        {
            data.levelObjectsData.RemoveAll(element => element.index != monolithLevelObjectIndex);
        }


        [Sirenix.OdinInspector.Button]
        public void UpdateBackground(int visualIndex = 0, int levelObjectIndex = 42, bool shouldOverride = false)
        {
            if (shouldOverride)
            {
                data.levelObjectsData.RemoveAll(element => element.index == levelObjectIndex);
            }

            bool isBackExists = data.levelObjectsData.Exists(element => element.index == levelObjectIndex);

            if (!isBackExists)
            {
                Levels.LevelObjectData backgroundData = new Levels.LevelObjectData(levelObjectIndex,
                                                                                   Vector3.zero,
                                                                                   Vector3.zero,
                                                                                   true,
                                                                                   new LevelObjectMoveSettings(),
                                                                                   null,
                                                                                   "{\"spriteIndex\":" + visualIndex + ",\"sortingOrder\":-1}");

                data.levelObjectsData.Add(backgroundData);
                
                CustomDebug.Log($"Create background with index {levelObjectIndex} by default.");
            }
        }


        [Sirenix.OdinInspector.Button]
        private void SwapLevelObjectIndexes(int from, int to)
        {
            foreach(var i in data.levelObjectsData)
            {
                if (i.index == from)
                {
                    i.index = to;
                }
            }
        }

        #endregion
    }
}
