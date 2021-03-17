using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.LevelConstructor
{
    public class CameraMovement : MonoBehaviour
    {
        #region Nested Types

        [Serializable]
        private class KeyToMovementDirection
        {
            public KeyCode key = default;
            public Vector3 direction = default;
        }

        #endregion



        #region Fields

        [SerializeField] private Vector3 targetOffset = default;
        [SerializeField] private float distance = default;
        [SerializeField] private float maxDistance = default;
        [SerializeField] private float minDistance = default;
        [SerializeField] private int zoomRate = default;
        [SerializeField] private float movementSpeed = default;
        [SerializeField] protected float dampening = default;

        [Header("Input")]
        [SerializeField] private List<KeyToMovementDirection> keysToMovementDirection = default;
        [Header("Camera")]
        [SerializeField] protected KeyCode cameraInteractionKey = default;

        private Transform target;

        private float currentDistance;
        private float desiredDistance;
        private Vector3 position;

        #endregion



        #region Properties

        public Vector3 Position
        {
            get => transform.position;
            set
            {
                transform.position = value;

                if (target != null)
                {
                    target.position = value + (transform.forward * currentDistance + targetOffset);
                }
            }
        }

        #endregion



        #region Unity Lifecycle

        protected virtual void Update()
        {
            float deltaTime = Time.deltaTime;

            if (Input.GetKey(cameraInteractionKey))
            {
                Pan(deltaTime);
            }

            UpdateMovementInput(deltaTime);
        }


        void LateUpdate()
        {
            if (target != null)
            {
                desiredDistance -= Input.GetAxis(ConstructorControlKeys.EditorCameraZoomKey) * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);
                desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
                // For smoothing of the zoom, lerp distance
                currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * dampening);

                // calculate position based on the new currentDistance 
                position = target.position - (transform.forward * currentDistance + targetOffset);
                transform.position = position;
            }
        }

        #endregion



        #region Methods

        public virtual void Init()
        {
            if (target != null)
            {
                Destroy(target.gameObject);
            }

            GameObject go = new GameObject("Camera Target");
            go.transform.position = transform.position + (transform.forward * distance);
            go.transform.rotation = transform.rotation;
            target = go.transform;

            distance = Vector3.Distance(transform.position, target.position);
            currentDistance = distance;
            desiredDistance = distance;

            position = transform.position;
        }


        protected void UpdateMovementInput(float deltaTime)
        {
            foreach (var keyToDirection in keysToMovementDirection)
            {
                if (Input.GetKey(keyToDirection.key))
                {
                    Move(keyToDirection.direction, deltaTime);
                }
            }
        }


        private void Pan(float deltaTime)
        {
            Vector2 panDirection = new Vector2(-Input.GetAxis(ConstructorControlKeys.EditorCameraMovementDirectionX),
                -Input.GetAxis(ConstructorControlKeys.EditorCameraMovementDirectionY));
            Move(panDirection, deltaTime);
        }


        private void Move(Vector3 globalDirection, float deltaTime)
        {
            Vector3 movement = globalDirection * movementSpeed * deltaTime;
            target.rotation = transform.rotation;
            target.Translate(movement, Space.Self);
        }

        #endregion
    }
}
