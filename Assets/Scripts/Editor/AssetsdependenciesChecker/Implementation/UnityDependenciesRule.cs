using UnityEditor;
using UnityEngine;

namespace Drawmasters.Editor
{
    internal class UnityDependenciesRule : IMatchingRule
    {
        #region IMatchingRule

        public bool IsMatch(string guid)
            => FindSimpleUnityDependenciesCount(guid) == 0;

        #endregion
        
        
        #region Private methods

        private int FindSimpleUnityDependenciesCount(string guid)
        {
            string path = guid.GuidToPath();
            
            string[] guids = AssetDatabase.GetDependencies(path, false);
            
            //TODO if DEBUG
            foreach (var i in guids)
            {
                Debug.Log($"Dependencies: {i}");
            }
            
            return guids.Length;
        }


        #endregion

    }
}

