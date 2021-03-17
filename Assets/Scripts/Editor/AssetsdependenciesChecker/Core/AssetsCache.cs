using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Drawmasters.Editor
{
    internal class AssetsCache
    {
        #region Properties

        internal List<Object> Assets { get; }

        internal List<string> AssetPathes { get; }

        internal List<string> AssetGuids { get; }

        #endregion


        #region Ctor


        public AssetsCache(string[] folderPathArray, string[] filterArray, string[] filesExtensionArray)
        {
            Assets = new List<Object>();
            
            foreach (var folderPath in folderPathArray)
            {
                var guids = AssetsLoader.LoadAssetsInFolder(folderPath, filesExtensionArray);

                foreach (var assetGuid in guids)
                {
                    string path = assetGuid.GuidToPath();

                    var buffer = AssetDatabase.LoadAssetAtPath<Object>(path);                    
                                        
                    Assets.Add(buffer);
                }
            }

            foreach (var filter in filterArray)
            {
                string[] guids = AssetDatabase.FindAssets(filter);

                foreach (var guid in guids)
                {
                    var asset = guid.FindAssetByGuid<Object>();
                    
                    Assets.Add(asset);
                }
            }

            AssetPathes = new List<string>(Assets.Count);

            foreach(var i in Assets)
            {
                AssetPathes.Add(i.ToGuid().GuidToPath());
            }

            AssetGuids = new List<string>(AssetPathes.Count);

            foreach(var i in AssetPathes)
            {
                AssetGuids.Add(i.PathToGuid());
            }
        }


        internal void UnloadAllAssets()
        {
            Assets.Clear();
            
            EditorUtility.UnloadUnusedAssetsImmediate();
        }

        #endregion
    }
}