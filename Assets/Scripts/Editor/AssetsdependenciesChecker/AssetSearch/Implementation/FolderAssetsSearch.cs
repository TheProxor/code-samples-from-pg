using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace Drawmasters.Editor
{
    public class FolderAssetsSearch : IAssetSearch
    {
        #region Fields

        private const string DummySearchPattern = "*";

        private readonly string folderPath;
        private readonly string searchPattern;

        #endregion



        #region Ctor

        public FolderAssetsSearch(string _folderPath, string _searchPattern = DummySearchPattern)
        {
            folderPath = _folderPath;
            searchPattern = _searchPattern;

            if (string.IsNullOrEmpty(searchPattern))
            {
                searchPattern = DummySearchPattern;
            }

            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"Directory doesn't exist at path {folderPath} .");
            }
        }

        #endregion



        #region IAssetSearch

        public List<Object> SearchAssets()
        {
            IList<string> guids = AssetsLoader.LoadAssetsInFolder(folderPath, new string[] { searchPattern });

            List<Object> result = new List<Object>(guids.Count);

            foreach (var assetGuid in guids)
            {
                string path = assetGuid.GuidToPath();

                Object loadedObject = AssetDatabase.LoadAssetAtPath<Object>(path);

                result.Add(loadedObject);
            }

            return result;
        }

        #endregion
    }
}
