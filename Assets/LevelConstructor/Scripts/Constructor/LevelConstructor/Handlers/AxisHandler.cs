using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class AxisHandler : BaseHandler
    {
        #region Fields

        const float HandleDistanceFromObjectCenter = 20.0f;

        private Vector3 direction;

        private float currentMoveDistance;

        #endregion



        #region Properties

        public override List<EditorLevelObject> SelectedObjects
        {
            get => selectedObjects;
            set
            {

                base.SelectedObjects = value;
                
                currentMoveDistance = 0.0f;

                if (SelectedObjects.Count > 0)
                {
                    UpdateRelativePosition();
                }
            }
        }

        #endregion



        #region Unity Lifecycle

        private void FixedUpdate()
        {
            foreach (var selectedObject in SelectedObjects)
            {
                selectedObject.Rigidbody2D.velocity = Vector3.zero;
            }
        }

        #endregion



        #region Methods

        public override void Initialize(Vector3 direction, Color unselectedColor, Color selectedColor, float snapSize,
            List<EditorLevelObject> editorLevelObjects)
        {
            base.Initialize(direction, unselectedColor, selectedColor, snapSize, selectedObjects);
            
            this.direction = direction;
        }


        public override void Drag(Vector3 dragVector, Camera camera, bool shouldSnap)
        {
            UpdateRelativePosition();
            
            Vector3 axisDirection = camera.WorldToScreenPoint(transform.position) - camera.WorldToScreenPoint(ObjectsCenter);
            float dragAngle = Vector3.Angle(axisDirection, dragVector);
            float dragDistance = dragVector.magnitude * Mathf.Cos(dragAngle * Mathf.Deg2Rad);
            float distanceFactor = axisDirection.magnitude / Vector3.Distance(transform.position, ObjectsCenter);

            if (shouldSnap)
            {
                currentMoveDistance += dragDistance / distanceFactor;
                if (((int)(currentMoveDistance / snapSize)) != 0)
                {
                    StartCoroutine(MoveObject((currentMoveDistance - currentMoveDistance % snapSize) * direction, true));
                    currentMoveDistance %= snapSize;
                }
            }
            else
            {
                currentMoveDistance = 0.0f;
                StartCoroutine(MoveObject(dragDistance * direction / distanceFactor, false));
            }
        }
        

        public override void UpdateRelativePosition()
        {
            transform.position = ObjectsCenter + direction * HandleDistanceFromObjectCenter;
        }


        private IEnumerator MoveObject(Vector3 deltaPosition, bool shouldSnap)
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
