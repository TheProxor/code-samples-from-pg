using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Drawmasters.LevelConstructor
{
    public static class EditorObjectsContainer
    {
        #region Fields

        const string EditorObjectsFolder = "Assets/LevelConstructor/Prefabs/Constructor/Objects";

        private static List<EditorLevelObject> levelObjectsPrefabs = null;

        #endregion



        #region Fields

        public static List<EditorLevelObject> Prefabs
        {
            get
            {
                if (levelObjectsPrefabs == null)
                {
                    UpdatePrefabs();
                }

                return levelObjectsPrefabs;
            }
        }
        
        
        public static GameMode CurrentGameMode { get; set; }


        public static WeaponType CurrentWeaponType { get; set; }

        #endregion



        #region Methods

        public static EditorLevelObject Create(int index)
        {
            EditorLevelObject result = default;
            EditorLevelObject source = Prefabs.Find(lo => lo.Index == index);

            if (source == null)
            {
                Debug.Log($"Source prefab for [{index}] index missing.");
            }
            else
            {
                result = Object.Instantiate(source);
            }

            return result;
        }


        public static void UpdatePrefabs()
        {
            levelObjectsPrefabs = new List<EditorLevelObject>();

            string[] guids = AssetDatabase.FindAssets("t:GameObject", new string[] { EditorObjectsFolder });

            foreach (var guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                EditorLevelObject levelObject = AssetDatabase.LoadAssetAtPath<EditorLevelObject>(assetPath);

                if (levelObject == null)
                {
                    CustomDebug.Log($"EditorObject is NULL. Path = {assetPath}");
                }
                else if (levelObject.IsLevelObjectAvailableForGameMode(CurrentGameMode))
                {
                    levelObjectsPrefabs.Add(levelObject);
                }
            }
        }

        #endregion
    }
}
