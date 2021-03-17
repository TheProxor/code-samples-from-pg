using Drawmasters.Helpers;
using Drawmasters.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace Drawmasters.Utils
{
    public static class ResourcesUtility
    {
        /// <summary>
        /// return all assets at path
        /// </summary>
        /// <typeparam name="T"> UnityEngine Object</typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<T> LoadAssetsAtPath<T>(string path) where T : UnityEngine.Object
        {
            List<T> loadedAssets = null;

            IAssetLoader<T> loader =
#if UNITY_EDITOR
                new EditorAssetsLoader<T>();
#else
            null;
#endif
            if (loader != null)
            {
                loadedAssets = loader.LoadAssets(path);
            }

            return loadedAssets;
        }

        public static void PerformActionOnFilesRecursively(string rootDir, Action<string> action)
        {
            foreach (string dir in Directory.GetDirectories(rootDir))
            {
                PerformActionOnFilesRecursively(dir, action);
            }

            foreach (string file in Directory.GetFiles(rootDir))
            {
                action(file);
            }
        }

        public static T FindInstance<T>(string prefabPath) where T : UnityEngine.Object
        {
            T result = default;

#if UNITY_EDITOR
            T loadedContent = AssetDatabase.LoadAssetAtPath<T>(prefabPath);

            if (loadedContent == null)
            {
                CustomDebug.Log("Check path for Content! Asset was not loaded in non playing mode");
            }
            else
            {
                result = loadedContent;
            }
#endif
            return result;
        }


        public static UnityEngine.Object[] LoadAllObjects<T>()
        {
            List<UnityEngine.Object> result = new List<UnityEngine.Object>();

            #if UNITY_EDITOR

            string[] allGuids = AssetDatabase.FindAssets("t:Object");

            IEnumerable<UnityEngine.Object> prefabsWithType = allGuids.Select(e => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(e), typeof(T)))
                                                                      .Where(e => e != null);

            result.AddRange(prefabsWithType);
            #endif

            return result.ToArray();
        }


        // Note that prefab's won't be loaded
        public static UnityEngine.Object[] LoadAssetsByType<T>(T t) where T : Type
        {
            List<UnityEngine.Object> result = new List<UnityEngine.Object>();

            #if UNITY_EDITOR
            var guids = AssetDatabase.FindAssets($"t:{t}");

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                var loadedAsset = AssetDatabase.LoadAssetAtPath(path, t);

                var o = AssetDatabase.LoadAssetAtPath(path, t);

                if (loadedAsset != null)
                {
                    result.Add(loadedAsset);
                }
            }
            #endif

            return result.ToArray();
        }
    }
}
