using System.Collections;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class DragHandler : BaseHandler
    {
        #region Fields

        Vector3 currentMovementDistance = Vector3.zero;

        #endregion
        
        

        #region Methods

        public override void Drag(Vector3 dragVector, Camera camera, bool shouldSnap)
        {
            UpdateRelativePosition();
            
            currentMovementDistance = currentMovementDistance + dragVector;

            if (shouldSnap)
            {
                Vector3 resultPositionWithoutSnapping = transform.position.Add(currentMovementDistance);
                Vector3 snapOffset = new Vector3(resultPositionWithoutSnapping.x % snapSize, resultPositionWithoutSnapping.y % snapSize, 
                    resultPositionWithoutSnapping.z % snapSize);

                Vector3 positionOffset = resultPositionWithoutSnapping - snapOffset - transform.position;
                transform.position = transform.position.Add(positionOffset);
                currentMovementDistance = snapOffset;

                StartCoroutine(MoveObject(positionOffset));
            }
            else
            {
                transform.position = transform.position.Add(currentMovementDistance);
                StartCoroutine(MoveObject(currentMovementDistance));
                
                currentMovementDistance = Vector3.zero;
            }
        }
        
        
        public override void UpdateRelativePosition()
        {
            transform.position = ObjectsCenter;
        }
        
        
        private IEnumerator MoveObject(Vector3 deltaPosition)
        {
            yield return new WaitForFixedUpdate();

            foreach (var selectedObject in SelectedObjects)
            {
                Vector3 newPosition = selectedObject.transform.position + deltaPosition;
                selectedObject.Move(newPosition);
            }
        }

        #endregion
    }
}
