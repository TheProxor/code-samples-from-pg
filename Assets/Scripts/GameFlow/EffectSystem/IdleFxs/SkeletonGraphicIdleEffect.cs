using System;
using UnityEngine;
using Drawmasters.Utils;
using Spine.Unity;
using Object = UnityEngine.Object;


namespace Drawmasters.Effects
{
    public class SkeletonGraphicEffect : RootIdleEffect
    {
        #region Fields

        private readonly Transform rootParent;

        private SkeletonGraphic skeletonData;
        private string boneName;

        #endregion



        #region Class lifecycle

        public SkeletonGraphicEffect(string _idleEffectName, Transform _rootParent) : base(_idleEffectName)
        {
            rootParent = _rootParent;
        }

        #endregion



        #region Methods

        public void SetupBoneName(SkeletonGraphic _skeletonData, string _boneName)
        {
            skeletonData = _skeletonData;
            boneName = _boneName;
        }


        public override void StopEffect()
        {
            base.StopEffect();

            if (fxRoot != null)
            {
                Object.Destroy(fxRoot.gameObject);
                fxRoot = null;
            }
        }


        protected override Transform GetFxRoot()
        {
            GameObject resultGO = SpineUtility.InstantiateBoneFollower(skeletonData, boneName, rootParent);
            return resultGO.transform;
        }

        #endregion
    }
}
