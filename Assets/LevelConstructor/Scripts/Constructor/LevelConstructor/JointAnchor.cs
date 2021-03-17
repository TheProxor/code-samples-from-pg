using System;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Drawmasters.LevelConstructor
{
    public class JointAnchor : MonoBehaviour, IDragHandler
    {
        #region Fields

        public event Action<Float3InputRemovableUi, Vector3> OnPositionChanged;

        private Float3InputRemovableUi parentControl;

        private Camera editorCamera;

        private EditorLevelObject followObject;

        private Vector3 offset;

        #endregion


        #region Unity lifecycle

        private void Update()
        {
            transform.position = followObject.transform.position + offset;            
        }

        #endregion



        #region Methods

        public void Init(Float3InputRemovableUi _parentControl,
                         EditorLevelObject _followObject)
        {
            parentControl = _parentControl;
            editorCamera = Camera.main;
            followObject = _followObject;

            offset = transform.position - followObject.transform.position;
        }


        public void ChangeLocalPosition(Vector3 position)
        {
            transform.position = position;
        }

        #endregion



        #region IDragHandler

        public void OnDrag(PointerEventData eventData)
        {
            if (parentControl != null)
            {
                transform.position = editorCamera.ScreenToWorldPoint(Input.mousePosition).SetZ(0.0f);   
                offset = transform.position - followObject.transform.position;             

                OnPositionChanged?.Invoke(parentControl, transform.position);
            }
        }

        #endregion
    }
}
