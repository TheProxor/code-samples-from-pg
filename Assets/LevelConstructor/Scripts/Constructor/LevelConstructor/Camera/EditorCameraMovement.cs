using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class EditorCameraMovement : CameraMovement
    {
        #region Nested types

        [Serializable]
        private class KeyToZoomDirection
        {
            public KeyCode key = default;
            public float zoomValue = default; 
        }

        #endregion
        
        
        #region Fields

        [SerializeField] private float xSpeed = default;
        [SerializeField] private float ySpeed = default;
        [SerializeField] private int yAngleMinLimit = default;
        [SerializeField] private int yAngleMaxLimit = default;        
        
        [Header("Zoom")]
        [SerializeField] private List<KeyToZoomDirection> zoomKeys = default;
        [SerializeField] private float zoomSpeed = default;
        [SerializeField] private Camera editorCamera = default;

        private float xDeg = 0.0f;
        private float yDeg = 0.0f;
        private Quaternion currentRotation;
        private Quaternion desiredRotation;

        #endregion



        #region Properties

        public Quaternion Rotation
        {
            get => transform.rotation; 
            set
            {
                currentRotation = value;
                desiredRotation = value;

                Vector3 rotation = value.eulerAngles;
                xDeg = rotation.x;
                yDeg = rotation.y;

                transform.rotation = value;
            }
        }

        #endregion



        #region Unity Lifecycle

        protected override void Update()
        {
            float deltaTime = Time.deltaTime;

            if (Input.GetKey(cameraInteractionKey))
            {
                Orbit(deltaTime);
            }

            UpdateMovementInput(deltaTime);
            UpdateZoomInput(deltaTime);
        }

        #endregion



        #region Methods

        public override void Init()
        {
            base.Init();

            xDeg = transform.eulerAngles.y;
            yDeg = transform.eulerAngles.x;

            currentRotation = transform.rotation;
            desiredRotation = transform.rotation;
        }


        private void UpdateZoomInput(float deltaTime)
        {
            foreach (var zoomDirection in zoomKeys)
            {
                if (Input.GetKey(zoomDirection.key))
                {
                    Zoom(zoomDirection.zoomValue, deltaTime);
                }
            }
        }
        
        
        private void Zoom(float zoomValue, float deltaTime)
        {
            float movement = zoomValue * zoomSpeed * deltaTime;
            editorCamera.orthographicSize += movement;
        }


        private void Orbit(float deltaTime)
        {
            xDeg += Input.GetAxis(ConstructorControlKeys.EditorCameraMovementDirectionX) * xSpeed * deltaTime;
            yDeg -= Input.GetAxis(ConstructorControlKeys.EditorCameraMovementDirectionY) * ySpeed * deltaTime;

            yDeg = ClampAngle(yDeg, yAngleMinLimit, yAngleMaxLimit);

            desiredRotation = Quaternion.Euler(yDeg, xDeg, 0.0f);
            currentRotation = transform.rotation;

            transform.rotation = Quaternion.Lerp(currentRotation, desiredRotation, deltaTime * dampening);
        }
        

        private float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360.0f)
            {
                angle += 360.0f;
            }
            if (angle > 360.0f)
            {
                angle -= 360.0f;
            }
            return Mathf.Clamp(angle, min, max);
        }

        #endregion
    }
}
