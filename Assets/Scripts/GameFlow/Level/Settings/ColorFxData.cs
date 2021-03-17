using System;
using Spine.Unity;
using UnityEngine;


namespace Drawmasters
{
    [Serializable]
    public class ColorFxData : BaseColorsData
    {
        [Tooltip("For bones serialization predominantly")] public SkeletonDataAsset dataAsset = default;

        [Enum(typeof(EffectKeys))] public string fxKey = default;
        [SpineBone(dataField = "dataAsset")] public string boneName = default;
    }
}
