using System.Collections.Generic;
using System.IO;
using Drawmasters.Helpers.Interfaces;
using Drawmasters.Utils;

#if UNITY_EDITOR
    using UnityEditor;
#endif


namespace Drawmasters.Helpers
{
    public class EditorAssetsLoader<T> : IAssetLoader<T> where T : UnityEngine.Object
    {
        #region IAssetLoader

        public List<T> LoadAssets(string path)
        {
            List<T> result = new List<T>();

            if (Directory.Exists(path))
            {
#if UNITY_EDITOR
                ResourcesUtility.PerformActionOnFilesRecursively(path, fileName =>
                {
                    T data = AssetDatabase.LoadAssetAtPath<T>(fileName);

                    if (data != null)
                    {
                        result.Add(data);
                    }
                });
#endif
            }

            return result;
        }

        #endregion
    }
}
