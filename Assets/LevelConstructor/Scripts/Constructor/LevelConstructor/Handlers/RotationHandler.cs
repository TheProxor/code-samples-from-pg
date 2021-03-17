using System.Collections;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class RotationHandler : BaseHandler
    {
        #region Fields

        private const float DistancePerAngleDelimiter = 10.0f;

        private float dragOffset = default;

        #endregion
        
        
        
        #region Methods
        
        public override void Drag(Vector3 dragVector, Camera camera, bool shouldSnap)
        {
            float dragAngle = Vector2.Distance(dragVector, ObjectsCenter) / DistancePerAngleDelimiter 
                              * Mathf.Sign(dragVector.x);

            if (shouldSnap)
            {
                dragAngle += dragOffset;
                
                dragOffset = dragAngle % snapSize;
                
                dragAngle -= dragOffset;
            }

            StartCoroutine(RotateObject(dragAngle));
        }
        
        
        public override void UpdateRelativePosition()
        {
            transform.position = ObjectsCenter;
        }


        private IEnumerator RotateObject(float angle)
        {
            yield return new WaitForFixedUpdate();

            foreach (var selectedObject in selectedObjects)
            {
                selectedObject.Rotate(new Vector3(0.0f, 0.0f, angle));
            }
        }

        #endregion    
    }
}

