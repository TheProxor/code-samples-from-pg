using UnityEngine;

namespace Drawmasters.Editor
{
    public class VfxScriptDependenciesRule : IMatchingRule
    {
        #region Fields

        private readonly AssetsCache cache;

        #endregion



        #region Ctor

        public VfxScriptDependenciesRule()
        {
            cache = new AssetsCache(
                new string[]
                {
                    string.Empty
                        .AppendPathComponent("Scripts")
                }, new string[]
                {

                },
                new string[]
                {
                    "*.cs"
                });

            Debug.Log($"Were loaded {cache.AssetPathes.Count} scripts for check.");
        }

        #endregion



        #region IMatchingRule

        public bool IsMatch(string assetName)
        {
            bool result = false;

            foreach (var i in cache.AssetPathes)
            {
                result = i.IsScriptContainsEffectInvocation(assetName).Result;

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

