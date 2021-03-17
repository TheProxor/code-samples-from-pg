using Sirenix.OdinInspector;
using System;
using UnityEngine;


namespace Drawmasters
{
    [Serializable]
    public class AssetLinkWrapper
    {
        #region Fields

        [ResourceLink] public AssetLink link = default;
        public bool shouldUsePool = default;

        [ShowIf("shouldUsePool")]
        public int preInstantiateCount = default;

        #endregion



        #region Lifecycle

        public AssetLinkWrapper(GameObject asset)
        {
            link = new AssetLink(asset);
        }


        public AssetLinkWrapper(GameObject asset, bool _usePooling = false, int _preInstantiateCount = 0)
        {
            link = new AssetLink(asset);
            shouldUsePool = _usePooling;
            preInstantiateCount = _preInstantiateCount;
        }

        #endregion
    }
}
