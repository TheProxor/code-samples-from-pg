// Use at your own risk
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif


namespace AssetsUtils
{
    public class AssetInfo
    {
        public string Guid = default;
        public string Path = default;

        public int usingCounter = default;
        public List<string> dependences = default;
        public bool isPrefab = default;
    }

#if UNITY_EDITOR

    [ExecuteInEditMode]
    public class AssetsChecker
    {
        #region Fields

        private const string noneAssetLinkGUID = "  - assetGUID: ";

        private static readonly string[] skipableFolders = { "ExternalPlugins", "Plugins", "Resources" };
        private static readonly Regex assetLinkGUIDRegex = new Regex("(assetGUID): ([\\w+]{32})");
        private static readonly Regex assetContainGUIDRegex = new Regex("(guid): ([\\w+]{32})");

        #endregion



        #region Private methods

        private static void ShowProgressBar(float value, string desc) =>
            EditorUtility.DisplayProgressBar("Find Unused Assets", desc, value);


        [MenuItem("AssetCleaner/Find Unused Assets")]
        private static void FindUnusedAssets()
        {
            ShowProgressBar(0.0f, "Checking assets that might be unused");

            AssetDatabase.Refresh();

            var assets = GetAssetsInfo();

            int checkedCount = 0;
            foreach (var asset in assets)
            {
                ShowProgressBar(Mathf.Lerp(0.2f, 0.8f, checkedCount / (float)assets.Count), $"Checking {asset.Path}");

                asset.dependences = new List<string>();
                List<string> dependencies = new List<string>(AssetDatabase.GetDependencies(asset.Path));
                Regex assetLinkRegex = new Regex(asset.Guid);

                if (asset.isPrefab)
                {
                    using (FileStream fs = File.OpenRead(asset.Path))
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        while (!sr.EndOfStream)
                        {
                            string prefabText = sr.ReadLine();

                            if (assetContainGUIDRegex.IsMatch(prefabText))
                            {
                                string assetLinkPath = AssetDatabase.GUIDToAssetPath(assetContainGUIDRegex.Match(prefabText).Groups[1].ToString());

                                if (!string.IsNullOrEmpty(assetLinkPath) &&
                                    !assetLinkPath.Equals(asset.Path))
                                {
                                    dependencies.Add(assetLinkPath);
                                }
                            }
                        }
                    }
                }

                foreach (var i in dependencies)
                {
                    if (!i.Equals(asset.Path))
                    {
                        asset.dependences.Add(i);
                    }
                }

                checkedCount++;
            }

            ShowProgressBar(0.8f, "Counting");

            foreach (var asset in assets)
            {
                foreach (var asset2 in assets)
                {
                    if (asset2.dependences.Contains(asset.Path) && !asset2.Guid.Equals(asset.Guid))
                    {
                        asset.usingCounter++;
                    }
                }
            }



            // проверить в этих ассетах нету ли на них ссылки из других ассетов

            EditorUtility.DisplayProgressBar("Find Unused Assets", $"Counting", 1.0f);

            assets = assets
                .Where(a => a.usingCounter > 0)
                .GroupBy(item => item.Path)
                .Select(item => item.First())
                .ToList();

            UnusedAssetsWindow.ShowResult(assets);
            EditorUtility.ClearProgressBar();
        }



        //[MenuItem("AssetCleaner/Find Missing Asset")] TODO: idk
        private static void FindMissingAssets()
        {
            List<AssetInfo> missedAssets = new List<AssetInfo>();

            AssetDatabase.Refresh();

            var assets = GetAssetsInfo();
            Regex guidRegex = new Regex("(guid|assetGUID): ([0-9a-fA-F]{32})");

            foreach (var asset in assets)
            {
                using (StreamReader sr = File.OpenText(asset.Path))
                {
                    while (true)
                    {
                        string prefabText = sr.ReadLine();
                        if (string.IsNullOrEmpty(prefabText)) break;

                        if (guidRegex.IsMatch(prefabText))
                        {
                            string assetLinkPath = AssetDatabase.GUIDToAssetPath(guidRegex.Match(prefabText).Groups[2].ToString());
                            if (string.IsNullOrEmpty(assetLinkPath))
                            {
                                missedAssets.Add(new AssetInfo { Path = asset.Path, Guid = guidRegex.Match(prefabText).Groups[2].ToString() });
                            }
                        }
                    }
                }
            }

            var distinctAssets = missedAssets.GroupBy(item => item.Path).Select(item => item.First()).ToList();

            //UnusedAssetsWindow.ShowResult(distinctAssets);
        }



        private static bool IsAsset(string fullPath) =>
            fullPath.Contains(".prefab") || fullPath.Contains(".asset");


        [MenuItem("AssetCleaner/Find Missing Asset Links")]
        private static void FindMissingAssetsLinks()
        {
            ShowProgressBar(0.0f, "Checking missiong assets links");

            List<(string guid, string path)> missedLinks = new List<(string guid, string path)>();

            AssetDatabase.Refresh();

            var assetsLinksGuidsAndPathes = GetAssetLinksGuidsAndPathes();

            int chekedCount = 0;
            foreach (var (guid, path) in assetsLinksGuidsAndPathes)
            {
                ShowProgressBar(Mathf.Lerp(0.0f, 0.9f, chekedCount / (float) assetsLinksGuidsAndPathes.Count), $"Checking {path}");
                Debug.Log($"hui {Mathf.Lerp(0.0f, 0.9f, chekedCount / assetsLinksGuidsAndPathes.Count)}");

                using (FileStream fs = File.OpenRead(path))
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        string readLine = sr.ReadLine();

                        if (readLine.Equals(noneAssetLinkGUID, StringComparison.Ordinal))
                        {
                            missedLinks.Add(("GUID is None", path));
                        }
                        else if (assetLinkGUIDRegex.IsMatch(readLine))
                        {
                            string assetLinkGuid = assetLinkGUIDRegex.Match(readLine).Groups[2].ToString();
                            string assetLinkPath = AssetDatabase.GUIDToAssetPath(assetLinkGuid);
                            object loadedAssetFromLink = AssetDatabase.LoadAssetAtPath(assetLinkPath, typeof(object));

                            if (string.IsNullOrEmpty(assetLinkPath) || loadedAssetFromLink == null)
                            {
                                CustomDebug.Log($"cant load {assetLinkPath}");
                                missedLinks.Add((assetLinkGuid, path));
                            }
                        }
                    }
                }

                chekedCount++;
            }

            EditorUtility.ClearProgressBar();
            MissedAssetsLinksWindow.ShowResult(missedLinks);
        }


        private static List<(string guid, string path)> GetAssetLinksGuidsAndPathes()
        {
            List<(string guid, string path)> guidsAndPathes = GetNotSkippableGuidsAndPathes();
            return guidsAndPathes.Where(e => IsAsset(e.path)).ToList();
        }


        private static List<AssetInfo> GetAssetsInfo()
        {
            List<(string guid, string path)> guidsAndPathes = GetNotSkippableGuidsAndPathes();
            List<(string guid, string path)> assetsGuidsAndPathes = guidsAndPathes.Where(e => e.path.Contains(".") && !e.path.Contains(".cs")).ToList();

            List<AssetInfo> result = new List<AssetInfo>();

            foreach (var (guid, path) in assetsGuidsAndPathes)
            {
                AssetInfo info = new AssetInfo { Guid = guid, Path = path };

                info.isPrefab = IsAsset(path);
                result.Add(info);
            }

            return result;
        }


        private static List<(string guid, string path)> GetNotSkippableGuidsAndPathes()
        {
            string[] allAssetsGuids = AssetDatabase.FindAssets(string.Empty, new string[] { "Assets" });

            string[] notSkipableAssetsGuids = allAssetsGuids.Where(e =>
            {
                string path = AssetDatabase.GUIDToAssetPath(e);
                return !Array.Exists(skipableFolders, s => path.Contains(s));

            }).ToArray();

            List<(string guid, string path)> assets = new List<(string guid, string path)>();

            foreach (var guid in notSkipableAssetsGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                assets.Add((guid, path));
            }

            return assets;
        }

        #endregion
    }

#endif

}
