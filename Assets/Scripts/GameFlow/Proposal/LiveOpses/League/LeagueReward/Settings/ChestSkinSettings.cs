using System;
using Spine.Unity;
using UnityEngine;


namespace Drawmasters.Proposal.Settings
{
    [Serializable]
    public class ChestSkinSettings
    {
        #region Fields
        
        #pragma warning disable 0414

        [Tooltip("Use for serialization only")]
        [SerializeField] private SkeletonDataAsset asset = default;

        #pragma warning restore 0414

        public ChestType chestType = default;
        [SpineSkin(dataField = "asset")] public string skinName = default;

        #endregion
    }
}
