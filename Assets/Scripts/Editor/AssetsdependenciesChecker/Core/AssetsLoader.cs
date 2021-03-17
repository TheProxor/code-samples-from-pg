using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


namespace Drawmasters.Editor
{
    internal static class AssetsLoader
    {
        #region Api

        internal static List<string> LoadAssetsInFolder(string relativeFolderPath, string[] searchFilters)
        {
            DirectoryInfo rootDirectory = GetDirectory(relativeFolderPath);

            if (rootDirectory == null)
            {
                return null;
            }

            HashSet<FileInfo> files = LoadAllFiles(rootDirectory, searchFilters);

            HashSet<string> guids = LoadAllGuids(files);

            return guids.ToList();
        }

        #endregion



        #region Private methods

        private static DirectoryInfo GetDirectory(string relativeFolderPath)
        {
            string fullPath = Application.dataPath.AppendPathComponent(relativeFolderPath);

            fullPath = fullPath.Replace('\\', '/');

            DirectoryInfo directoryInfo = new DirectoryInfo(fullPath);

            if (!directoryInfo.Exists)
            {
                Debug.Log($"Folder doesn't exist. Full path: {fullPath}");

                return null;
            }

            return directoryInfo;
        }


        private static HashSet<FileInfo> LoadAllFiles(DirectoryInfo rootDirectory, string[] searchFilters)
        {
            HashSet<FileInfo> loadedFiles = new HashSet<FileInfo>();

            if (searchFilters == null || searchFilters.Length == 0)
            {
                searchFilters = new string[] { "*" };

                Debug.Log("Filter array is empty. Dummy filter added.");
            }

            foreach (var i in searchFilters)
            {
                var buffer = rootDirectory.GetFiles(i, SearchOption.AllDirectories);

                loadedFiles.AddRangeIfNotContains(buffer);
            }

            return loadedFiles;
        }

        private static HashSet<string> LoadAllGuids(HashSet<FileInfo> filesSet)
        {
            HashSet<string> guidsSet = new HashSet<string>();

            foreach(var i in filesSet)
            {
                string guid = ConvertFileToGuid(i);

                guidsSet.AddIfNotContains(guid);
            }

            return guidsSet;
        }


        private static string ConvertFileToGuid(FileInfo fileInfo)
        {
            string fullAssetPath = fileInfo.FullName;

            fullAssetPath = fullAssetPath.Replace('\\', '/');

            string relativePath =  "Assets" + fullAssetPath.Replace(Application.dataPath, "");

            return relativePath.PathToGuid();
        }

        #endregion
    }
}