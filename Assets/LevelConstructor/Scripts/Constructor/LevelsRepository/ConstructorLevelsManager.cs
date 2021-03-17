using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Drawmasters.Levels;
using UnityEditor;
using UnityEngine;


namespace Drawmasters.LevelsRepository.Editor
{
    public static class ConstructorLevelsManager
    {
        static readonly Regex AssetNameRegex = new Regex("(.+)__(.+)__(.+)");
        const int IndexGroupHash = 3;

        const string FormatAssetName = "{0}__{1}__{2}";
        const string AssetExtension = ".asset";
        static readonly string EditorPath = Path.Combine("Assets/Data/Resources", LevelsContainer.ResourcePath);

        static List<LevelHeader> headers;
        static List<LevelBody> bodies;


        static ConstructorLevelsManager() => LoadData();


        public static void CreateLevel(LevelHeader header, Level.Data data = null)
        {
            bool exists = true;
            string assetName = string.Empty;

            while (exists)
            {
                assetName = string.Format(FormatAssetName, header.mode, header.title, Hash128.Compute(DateTime.Now.Ticks.ToString()).ToString());
                exists = AssetDatabase.GetMainAssetTypeAtPath(Path.Combine(EditorPath, LevelsContainer.HeadersFolder, assetName)) != null;
            }

            InsertEmptySpace(header.mode);
            AssetDatabase.CreateAsset(header, Path.Combine(EditorPath, LevelsContainer.HeadersFolder, assetName + AssetExtension));

            SetLevelData(header, data);

            AssetDatabase.Refresh();

            LoadData();
        }


        public static void SetHeader(LevelHeader target, LevelHeader data)
        {
            if (headers.Contains(target) && target.mode != data.mode)
            {
                InsertEmptySpace(data.mode);
            }

            if (target.mode != data.mode || target.title != data.title)
            {
                string hash = AssetNameRegex.Match(target.name).Groups[IndexGroupHash].Value;
                string newAssetName = string.Format(FormatAssetName, data.mode, data.title, hash);

                string oldBodyPath = Path.Combine(EditorPath, LevelsContainer.BodiesFolder, target.name + AssetExtension);
                AssetDatabase.RenameAsset(oldBodyPath, newAssetName);
                string oldHeaderPath = Path.Combine(EditorPath, LevelsContainer.HeadersFolder, target.name + AssetExtension);
                AssetDatabase.RenameAsset(oldHeaderPath, newAssetName);
                AssetDatabase.Refresh();
            }

            target.title = data.title;
            target.mode = data.mode;
            target.projectilesCount = data.projectilesCount;
            target.isDisabled = data.isDisabled;
            target.weaponType = data.weaponType;
            target.stagesCount = data.stagesCount;
            target.levelType = data.levelType;
            target.stageProjectilesCount = data.stageProjectilesCount;

            RefreshHeader(target);
        }


        public static LevelHeader[] GetLevelHeaders(GameMode mode) => headers.Where(h => h.mode == mode).OrderBy(h => h.title).ToArray();

        public static LevelHeader[] GetLevelHeaders(GameMode mode, LevelType levelType) => headers.Where(h => h.mode == mode && h.levelType == levelType).OrderBy(h => h.title).ToArray();

        public static LevelHeader[] GetLevelHeaders() => headers.ToArray();

        public static Level.Data GetLevelData(LevelHeader header) => GetBody(header).data;

        public static void RemoveLevel(LevelHeader header)
        {
            AssetDatabase.DeleteAsset(GetBodyPath(header));
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(header));
            AssetDatabase.Refresh();

            LoadData();
        }


        public static void SetLevelData(LevelHeader header, Level.Data data)
        {
            data = data ?? new Level.Data();

            LevelBody body = GetBody(header);

            if (body == null)
            {
                body = CreateBody(header);
            }

            body.data = data;
            body.UpdateBackground();

            EditorUtility.SetDirty(body);
            AssetDatabase.SaveAssets();
        }


        private static LevelBody CreateBody(LevelHeader header)
        {
            LevelBody body = ScriptableObject.CreateInstance<LevelBody>();
            AssetDatabase.CreateAsset(body, GetBodyPath(header));
            AssetDatabase.Refresh();
            LoadData();

            return body;
        }


        public static (LevelHeader, int)[] FindLevelsWithObject(int objectIndex)
        {
            List<(LevelHeader, int)> result = new List<(LevelHeader, int)>();

            foreach (var header in headers)
            {
                var levelData = GetLevelData(header);

                int foundObjects = levelData.levelObjectsData.Count((data) => data.index == objectIndex);

                if (foundObjects > 0)
                {
                    result.Add((header, foundObjects));
                }
            }

            return result.OrderBy((info) => info.Item1.mode).ThenBy((info) => info.Item1.title).ToArray();
        }


        static string GetBodyPath(LevelHeader header) => Path.Combine(EditorPath, LevelsContainer.BodiesFolder, header.name + AssetExtension);


        static void LoadData()
        {
            headers = new List<LevelHeader>(Resources.LoadAll<LevelHeader>(Path.Combine(LevelsContainer.ResourcePath, LevelsContainer.HeadersFolder)));
            bodies = new List<LevelBody>(Resources.LoadAll<LevelBody>(Path.Combine(LevelsContainer.ResourcePath, LevelsContainer.BodiesFolder)));
        }


        static void RefreshHeader(LevelHeader header, bool saveAssets = true)
        {
            EditorUtility.SetDirty(header);

            if (saveAssets)
            {
                AssetDatabase.SaveAssets();
            }
        }


        static LevelBody GetBody(LevelHeader header) => bodies.Find((b) => b.name == header.name);


        static void InsertEmptySpace(GameMode mode)
        {
            LevelHeader[] newModeHeaders = GetLevelHeaders(mode);

            for (int i = 0; i < newModeHeaders.Length; i++)
            {
                RefreshHeader(newModeHeaders[i], false);
            }

            AssetDatabase.SaveAssets();
        }
    }
}
