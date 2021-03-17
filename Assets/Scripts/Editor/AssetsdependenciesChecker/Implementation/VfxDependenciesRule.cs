using System.Threading;
using UnityEngine;

namespace Drawmasters.Editor
{
    internal class VfxDependenciesRule : IMatchingRule
    {
        #region Fields

        private readonly AssetsCache cache;

        #endregion



        #region Ctor

        public VfxDependenciesRule()
        {
            cache = new AssetsCache(new string[]
            {
                string.Empty.
                    AppendPathComponent("Resources").
                    AppendPathComponent("ContentStorage"),
                string.Empty.
                    AppendPathComponent("Prefabs")
            }, new string[]
            {
                //"t:prefab"
            },
            new string[]
            {
                "*.prefab",
                "*.asset"
            });

            Debug.Log($"Resources for vfx check count: {cache.Assets.Count}");
        }

        #endregion



        #region IMatchingRule

        public bool IsMatch(string assetPath)
        {
            bool result = false;
            
            foreach(var i in cache.AssetPathes)
            {
                result = i.IsYamlContainsAssetName(assetPath).Result;

                if (result)
                {
                    break;
                }
            }

            return result;
        }

        #endregion
    }
}
