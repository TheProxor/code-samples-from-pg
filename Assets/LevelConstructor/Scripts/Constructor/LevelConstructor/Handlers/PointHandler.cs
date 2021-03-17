using System;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{ 
    public class PointHandler : BaseHandler
    {
        #region Fields

        public static event Action<PointHandler> OnPositionChange;
        
        private Vector3 currentMovementDistance;

        #endregion
        
        

        #region Methods

        public override void Drag(Vector3 dragVector, Camera camera, bool shouldSnap)
        {
            Vector3 axisDirection = camera.WorldToScreenPoint(transform.position) - camera.WorldToScreenPoint(ObjectsCenter);
            float distanceFactor = axisDirection.magnitude / Vector3.Distance(transform.position, ObjectsCenter);
            currentMovementDistance = currentMovementDistance + dragVector / distanceFactor;

            if (shouldSnap)
            {
                Vector3 resultPositionWithoutSnapping = transform.position.Add(currentMovementDistance);
                Vector3 snapOffset = new Vector3(resultPositionWithoutSnapping.x % snapSize, resultPositionWithoutSnapping.y % snapSize, 
                    resultPositionWithoutSnapping.z % snapSize);
                
                transform.position = resultPositionWithoutSnapping - snapOffset;
                currentMovementDistance = snapOffset;
            }
            else
            {
                transform.position = transform.position.Add(currentMovementDistance);
                currentMovementDistance = Vector3.zero;
            }

            OnPositionChange?.Invoke(this);
        }

        #endregion
    }
}
