using System;
using Spine.Unity;
using UnityEngine;

namespace Drawmasters.Proposal
{
    public partial class HitmastersVisualSettings
    {
        #region Nested types

        [Serializable]
        public class MapPointIconData
        {
            public HitmastersMapPoint.MapPointType type = default;

            public Sprite activeIcon = default;
            public Sprite disableIcon = default;
            public Sprite lockedIcon = default;
            public Sprite shadowIcon = default;

            [Enum(typeof(EffectKeys))] public string idleFxKey = default;
            [Enum(typeof(EffectKeys))] public string idleActiveFxKey = default;
            [Enum(typeof(EffectKeys))] public string unlockFxKey = default;
            public Vector3 unlockFxKeyScale = default;
        }
        
        
        [Serializable]
        public class GameModeSprite
        {
            public GameMode mode = default;
            public Sprite sprite = default;
            public Sprite smallSprite = default;
            public Sprite rewardSprite = default;
            public string uiMenuText = default;
            public ShooterSkinType skinType = default;
            public SkeletonDataAsset asset = default;
            public string startAnimation = default;
            public string loopAnimation = default;

            [Header("Effects")]
            [Enum(typeof(EffectKeys))] public string menuIdleFxKey = default;
            [Enum(typeof(EffectKeys))] public string popUpIdleFxKey = default;

            [Header("Popup Effects")]
            public EventFxData[] eventFxData = default;
        }
        

        [Serializable]
        public class EventFxData
        {
            // serialization only
            public SkeletonDataAsset asset = default;

            [Enum(typeof(EffectKeys))] public string fxKey = default;
            [SpineEvent(dataField = "asset")] public string animEvent = default;
            [SpineEvent(dataField = "asset")] public string animStopEvent = default;
            [SpineBone(dataField = "asset")] public string bone = default;
            public bool shouldAttachToRoot = true;
        }

        #endregion
    }
}