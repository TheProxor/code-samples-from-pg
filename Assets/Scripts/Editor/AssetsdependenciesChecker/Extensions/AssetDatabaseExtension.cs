using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

// [g][u][i][d][\W][\s][\w]{32}
// \bguid[\W][\s][\w]{32}
// \bguid[\\W][\\s]\b8ab1a180190cd4bbba7f5e40740dbeb9

namespace Drawmasters.Editor
{
    internal static class AssetDatabaseExtension
    {
        #region Api

        internal static string ToGuid(this Object asset) =>
            AssetDatabase.GetAssetPath(asset).PathToGuid();

        internal static string GuidToPath(this string guid) =>
            AssetDatabase.GUIDToAssetPath(guid);

        internal static string PathToGuid(this string path) =>
            AssetDatabase.AssetPathToGUID(path);

        internal static Object FindAssetByGuid<T>(this string guid) where T : Object
        {
            string path = guid.GuidToPath();

            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        internal static string GetAssetPath(this Object asset) =>
            AssetDatabase.GetAssetPath(asset);

        internal static bool IsYamlContainsGuid(this Object asset, string guidForCheck) =>
            asset.GetAssetPath().IsYamlContainsGuid(guidForCheck).Result;

        internal static async Task<bool> IsYamlMatchRegex(string assetPath, Regex regex)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }

            bool result = false;

            using (FileStream fs = new FileStream(assetPath, FileMode.Open))
            using (StreamReader sr = new StreamReader(fs))
            {
                while (!sr.EndOfStream)
                {
                    string line = await sr.ReadLineAsync();

                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    result = regex.IsMatch(line);
                    if (result)
                    {
                        break;
                    }
                }
            }

            return result;
        }


        internal static async Task<bool> IsYamlContainsGuid(this string assetPath, string guidForCheck)
            => await IsYamlMatchRegex(assetPath, new Regex($"\\bguid[\\W][\\s]\\b{guidForCheck}", RegexOptions.IgnoreCase));

        internal static bool IsYamlContainsAssetName(this Object assetObject, string guidForCheck)
            => assetObject.GetAssetPath().IsYamlContainsAssetName(guidForCheck).Result;

        internal static async Task<bool> IsYamlContainsAssetName(this string assetPath, string pathNameForCheck)
        {
            string assetName = pathNameForCheck;

            string validAssetName = assetName.Substring(assetName.LastIndexOf('/') + 1);

            validAssetName = validAssetName.Replace(".prefab", "");

            return await IsYamlMatchRegex(assetPath, new Regex($"\\b{validAssetName}", RegexOptions.IgnoreCase));
        }


        internal static async Task<bool> IsScriptContainsEffectInvocation(this string scriptPath, string pathForCheck)
        {
            string assetName = pathForCheck;

            string validAssetName = assetName.Substring(assetName.LastIndexOf('/') + 1);

            validAssetName = validAssetName.Replace(".prefab", "");

            validAssetName = $"EffectKeys[.]{validAssetName}";

            return await IsYamlMatchRegex(scriptPath, new Regex($"\\b{validAssetName}\\W", RegexOptions.IgnoreCase));
        }

        #endregion
    }
}

