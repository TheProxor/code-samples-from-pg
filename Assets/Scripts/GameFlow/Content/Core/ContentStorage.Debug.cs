using System.Collections.Generic;
using System.Linq;
using Drawmasters.Levels;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif


namespace Drawmasters
{
    public partial class ContentStorage
    {
        #region Fields

        [Header("Data for fast dictionaries filling")]
        [SerializeField] private List<LevelObject> levelObjectPrefabs = default;

        #endregion



        #region Properties

        public List<AssetLink> LevelObjectLinks => levelObjects.Select(a => a.Link).ToList();

        #endregion



        #region Editor methods
#if UNITY_EDITOR

        [Sirenix.OdinInspector.Button]
        private void AddMissedLevelObject()
        {
            foreach (var prefab in levelObjectPrefabs)
            {
                string prefabPath = AssetDatabase.GetAssetPath(prefab);
                string prefabGUID = AssetDatabase.AssetPathToGUID(prefabPath);

                bool isAlreadySet = LevelObjectLinks.Select(a => a.assetGUID).ToList().Contains(prefabGUID);

                if (!isAlreadySet)
                {
                    int index = levelObjects.Count == 0 ? 0 : levelObjects.Max((i) => i.Index) + 1;
                    var assetLink = new AssetLink(prefab.gameObject);

                    levelObjects.Add(new LevelObjectItem
                    {
                        Index = index,
                        Link = assetLink
                    });

                    CustomDebug.Log("prefab with GUID: <color=green>" + prefabGUID +
                                    "</color> added with index <color=green>" + index + "</color>");
                }
            }
        }


        /// <summary>
        /// Return value: index for Get().
        /// </summary>
        public int AddLevelObject(LevelObject value)
        {
            int index = levelObjects.Count == 0 ? 0 : levelObjects.Max((i) => i.Index) + 1;
            var assetLink = new AssetLink(value);

            levelObjects.Add(new LevelObjectItem
            {
                Index = index,
                Link = assetLink,
                shouldUsePool = true
            });

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            EditorUtility.SetDirty(Content.Storage);
            return index;
        }
#endif        
        #endregion
    }
}
