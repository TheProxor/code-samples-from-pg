using UnityEngine;

namespace Drawmasters.Editor
{
    public class PrefabDependenciesRule : IMatchingRule
    {
        #region Fields

        private readonly AssetsCache cache;        

        #endregion


        
        #region Ctor

        public PrefabDependenciesRule()
        {
            cache = new AssetsCache(new string[]
            {
                //TODO add path logic
                string.Empty.
                    AppendPathComponent("Vfx").
                    AppendPathComponent("Materials")
            }, new string []
            {
            },
            new string[]
            {
                "*.mat",
            });
            
            Debug.Log($"Resources count: {cache.Assets.Count}");
        }

        #endregion
        
        
        
        #region IMatchingRule

        public bool IsMatch(string guid)
        {
            bool result = false;
            
            foreach (var asset in cache.AssetPathes)
            {
                bool isContainGuid = asset.IsYamlContainsGuid(guid).Result;

                if (isContainGuid)
                {
                    //TODO if DEBUG
                    //Debug.Log($"Asset: {asset.name} includes guid: {guid} or asset {guid.GuidToPath()}");                    
                    result = true;
                    break;
                }
            }

            return result;
        }

        #endregion
    }
}