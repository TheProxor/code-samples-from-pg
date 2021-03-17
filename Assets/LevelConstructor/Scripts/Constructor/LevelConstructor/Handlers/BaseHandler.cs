using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class BaseHandler : MonoBehaviour
    {
        #region Fields

        [SerializeField] private SpriteRenderer handlerRenderer = default;

        protected List<EditorLevelObject> selectedObjects = new List<EditorLevelObject>();

        protected float snapSize = default;
        
        private Color selectedColor = default;
        private Color unselectedColor = default;

        #endregion
        
        
        
        #region Properties

        public virtual List<EditorLevelObject> SelectedObjects
        {
            get => selectedObjects;
            set
            {
                selectedObjects = value;

                if (selectedObjects.Count > 0)
                {
                    UpdateRelativePosition();
                }
                else
                {
                    StopAllCoroutines();
                }
            }
        }
        
        
        protected Vector3 ObjectsCenter
        {
            get
            {
                Vector3 center = Vector3.zero;

                if (selectedObjects.Count > 0)
                {
                    foreach (var selectedObject in selectedObjects)
                    {
                        center += selectedObject.Center;
                    }

                    center /= selectedObjects.Count;
                }

                return center;
            } 
        }
        
        #endregion



        #region Methods
        
        public void ChangeSelection(bool isSelected)
        {
            handlerRenderer.color = (isSelected) ? (selectedColor) : (unselectedColor);
        }


        public void SetSnapSize(float newSnapSize)
        {
            snapSize = newSnapSize;
        }
        
        
        public virtual void Initialize(Vector3 direction, Color unselectedColor, Color selectedColor, float snapSize, 
            List<EditorLevelObject> selectedObjects)
        {
            this.selectedColor = selectedColor;
            this.unselectedColor = unselectedColor;
            this.snapSize = snapSize;

            SelectedObjects = selectedObjects;

            handlerRenderer.color = unselectedColor;
        }


        public virtual void UpdateRelativePosition() { }


        public virtual void Drag(Vector3 dragVector, Camera camera, bool shouldSnap) { }

        #endregion
    }
}
