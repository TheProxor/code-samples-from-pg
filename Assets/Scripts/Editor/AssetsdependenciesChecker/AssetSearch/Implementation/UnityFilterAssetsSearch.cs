using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Drawmasters.Editor
{
    public class UnityFilterAssetsSearch : IAssetSearch
    {
        #region Fields

        private readonly string unitySearchFilter;
        private readonly string targetFolder;

        #endregion



        #region Ctor

        public UnityFilterAssetsSearch(string _unitySearchFilter,
                                       string _targetFolder = "")
        {
            unitySearchFilter = _unitySearchFilter;
            targetFolder = _targetFolder;

            if (!string.IsNullOrEmpty(targetFolder))
            {
                string fullPath = Application.dataPath.AppendPathComponent(targetFolder);

                if (!Directory.Exists(fullPath))
                {
                    throw new DirectoryNotFoundException($"Directory doesn't exist at path {fullPath} .");
                }
            }
        }

        #endregion



        #region IAssetSearch

        public List<Object> SearchAssets()
        {
            string[] guids = FindAssets();

            List<Object> result = new List<Object>(guids.Length);

            foreach (var guid in guids)
            {
                var asset = guid.FindAssetByGuid<Object>();

                result.Add(asset);
            }

            return result;
        }

        #endregion


        #region Private methods

        private string [] FindAssets()
        {
            bool isConcreteFolderDefined = !string.IsNullOrEmpty(targetFolder);

            return isConcreteFolderDefined ? 
                AssetDatabase.FindAssets(unitySearchFilter, new string[] { targetFolder }) : 
                AssetDatabase.FindAssets(unitySearchFilter);
        }

        #endregion
    }
}

