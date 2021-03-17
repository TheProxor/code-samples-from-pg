using System;
using Spine.Unity;
using UnityEngine;


namespace Drawmasters.Proposal.Settings
{
    [Serializable]
    public class ChestAnimationSettings
    {
        #region Fields
        
        #pragma warning disable 0414

        [Tooltip("Use for serialization only")]
        [SerializeField] private SkeletonDataAsset asset = default;

        #pragma warning restore 0414

        [SpineAnimation(dataField = "asset")] public string appearAnimationName = default;
        [SpineAnimation(dataField = "asset")] public string idleAnimationName = default;
        [SpineAnimation(dataField = "asset")] public string jumpAnimationName = default;
        [SpineAnimation(dataField = "asset")] public string openAnimationName = default;
        [SpineAnimation(dataField = "asset")] public string outAnimationName = default;
        public float durationOpen = default;
        public float durationClaimReward = default;

        #endregion
    }
}