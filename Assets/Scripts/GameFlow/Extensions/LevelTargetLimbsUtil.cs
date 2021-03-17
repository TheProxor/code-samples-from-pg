using Drawmasters.Levels;
using UnityEngine;

namespace GameFlow.Extensions
{
    public static class LevelTargetLimbsUtil
    {
        #region Public methods

        public static Vector3 FindLimbPosition(this LevelTarget levelTarget, string limbName, Vector3 defaultValue = default)
        {
            Vector3 result = defaultValue;

            Transform transform = FindLimbTransform(levelTarget, limbName);
            if (transform != null)
            {
                result = transform.position;
            }

            return result;
        }
        
        public static Transform FindLimbTransform(this LevelTarget levelTarget, string limbName)
        {
            Transform result = default;
            
            LevelTargetLimb limb = levelTarget.Limbs.Find(e => e.RootBoneName.Contains(limbName));
            if (limb != null)
            {
                result = limb.transform;
            }

            return result;
        }
        
        #endregion
    }
}