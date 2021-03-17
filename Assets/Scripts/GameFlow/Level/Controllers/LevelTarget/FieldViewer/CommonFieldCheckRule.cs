using System;
using UnityEngine;

namespace Drawmasters.Levels
{
    public class CommonFieldCheckRule : IFieldCheckRule
    {
        #region Fields

        private readonly Func<Vector3, bool> isOutOfZone;

        #endregion



        #region IInitializable

        public void Initialize()
        {
            
        }

        #endregion



        #region IDeinitializable

        public void Deinitialize()
        {
            
        }

        #endregion



        #region IFieldCheckRule

        public bool IsMatching(LevelTarget checkingObject)
        {
            bool isMatching = true;

            isMatching &= checkingObject.Ragdoll2D.IsActive;

            if (isMatching)
            {
                isMatching &= isOutOfZone.Invoke(checkingObject.Ragdoll2D.EstimatedSkeletonPosition);
            }

            return isMatching;
        }

        #endregion



        #region Ctor

        public CommonFieldCheckRule(Func<Vector3, bool> _check)
        {
            isOutOfZone = _check;
        }

        #endregion
    }
}
