using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Drawmasters.Editor
{
    internal class ReplaceGuid : IMatchingRule
    {
        public string OldGuid { get; }

        public string NewGuid { get; }

        public ReplaceGuid(string oldGuid, string newGuid)
        {
            OldGuid = oldGuid;
            NewGuid = newGuid;
        }

        protected async Task<bool> PerformReplaceGuid(string assetPath)
        {
            string newLine = string.Empty;

            using (FileStream fs = new FileStream(assetPath, FileMode.Open))
            using (StreamReader sr = new StreamReader(fs))
            {
                string oldLine = await sr.ReadToEndAsync();

                newLine = oldLine.Replace(OldGuid, NewGuid);

                if (oldLine == newLine)
                {
                    newLine = string.Empty;
                }
            }

            if (!string.IsNullOrEmpty(newLine))
            {
                using (FileStream fs = new FileStream(assetPath, FileMode.Create))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    await sw.WriteAsync(newLine);
                }
            }

            return false;
        }

        public bool IsMatch(string assetPath)
        {
            return PerformReplaceGuid(assetPath).Result;
        }
    }



    internal class DependenciesChecker
    {
        #region Api

        [MenuItem("Drawmasters/Run dependencies checker")]
        public static void Run()
        {
            AssetsCache assetsCache = new AssetsCache(new string[]
            {
                string.Empty
                    .AppendPathComponent("Prefabs"),
                string.Empty
                    .AppendPathComponent("Resources"),
            }, new string[]
            {

            },
            new string[]
            {
                "*.prefab"
            });

            Debug.Log($"Loaded assets for check : {assetsCache.Assets.Count}");

            CheckForAll(
                new SimpleDependenciesSearch(
                    new ReplaceGuid("08ed1fc53546c46758314f420626e1ae", "140fc5dfcf174e6e91c25074057fa9a8")
                    ),
                //new VfxDependenciesRule(), 
                //new VfxScriptDependenciesRule()),

                //TODO rules for textures
                //new UnityDependenciesRule(),
                //new PrefabDependenciesRule()),

                assetsCache.AssetPathes);
        }

        #endregion



        #region Private methods

        private async static void CheckForAll(IMatchingRule rule, List<string> assets)
        {
            int foundCount = 0;

            foreach(var i in assets)
            {
                bool isMatch = await Task.Run(() => rule.IsMatch(i));

                foundCount++;

                if (foundCount % 10 == 0)
                {
                    float percents = foundCount / (float)assets.Count * 100;

                    Debug.Log(percents.ToString("F2"));
                }
            }

            Debug.Log($"Found : {foundCount}");
        }

        #endregion
    }
}