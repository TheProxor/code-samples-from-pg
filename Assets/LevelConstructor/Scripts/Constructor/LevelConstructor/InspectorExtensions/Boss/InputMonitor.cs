using System;
using Drawmasters.Utils;
using UnityEngine;
using Object = UnityEngine.Object;


namespace Drawmasters.LevelConstructor
{
    public class InputMonitor
    {
        #region Fields

        private readonly float pointsDistance;

        private Camera camera;
        private LineRenderer pathRenderer;

        private Vector3 lastScreenInputPoint;
        private Vector3 allowedDirection;

        #endregion



        #region Properties

        public Vector3[] CurrentWorldPoints { get; private set; }

        #endregion



        #region Class lifecycle

        public InputMonitor(float _pointsDistance = 0.0f)
        {
            pointsDistance = _pointsDistance;
            CurrentWorldPoints = Array.Empty<Vector3>();
        }

        #endregion



        #region Methods

        public void SimplifyTrajectory(float tolerance)
        {
            if (pathRenderer != null)
            {
                pathRenderer.Simplify(tolerance);
                CurrentWorldPoints = pathRenderer.GetCurrentPoints().ToArray();
            }
        }

        public void MarkDrawFollowLine(Camera _camera)
        {
            pathRenderer = pathRenderer ?? Content.Management.CreateDefaultLineRenderer();
            camera = _camera;
        }


        public void StartMonitor() =>
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;


        public void SetVerticalDirection(bool enabled) =>
            allowedDirection = enabled ? Vector3.up : default;


        public void SetHorizontalDirection(bool enabled) =>
            allowedDirection = enabled ? Vector3.right : default;


        public void StopMonitor()
        {
            if (pathRenderer != null)
            {
                Object.Destroy(pathRenderer.gameObject);
                pathRenderer = null;
            }

            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
        }


        public void Clear() =>
            CurrentWorldPoints = Array.Empty<Vector3>();


        private Vector3 GetDirection(Vector3 previousMousePosition, Vector3 mousePosition)
        {
            if (Vector3.Distance(previousMousePosition, mousePosition) < 10.0f)
            {
                return default;
            }

            if (allowedDirection == Vector3.up)
            {
                return mousePosition.x > previousMousePosition.x ? Vector3.right : Vector3.left;
            }
            else if (allowedDirection == Vector3.right)
            {
                return mousePosition.y > previousMousePosition.y ? Vector3.up : Vector3.down;
            }

            return default;
        }

        #endregion



        #region Events handlers

        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (lastScreenInputPoint == Vector3.zero)
                {
                    lastScreenInputPoint = Input.mousePosition;
                }
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 mouseScreenPosition = Input.mousePosition;

                if (allowedDirection != default)
                {
                    Vector3 foundDirection = GetDirection(lastScreenInputPoint, mouseScreenPosition);

                    Vector3 axisDirection = mouseScreenPosition - lastScreenInputPoint;
                    float dragAngle = Vector3.Angle(axisDirection, foundDirection);
                    float dragDistance = axisDirection.magnitude * Mathf.Cos(dragAngle * Mathf.Deg2Rad);

                    mouseScreenPosition = lastScreenInputPoint + (dragDistance * foundDirection);
                }

                if (Vector3.Distance(lastScreenInputPoint, mouseScreenPosition) >= pointsDistance)
                {
                    Vector3 worldPoint = camera.ScreenToWorldPoint(mouseScreenPosition).SetZ(default);
                    CurrentWorldPoints = CurrentWorldPoints.Add(worldPoint);

                    lastScreenInputPoint = mouseScreenPosition;

                    if (pathRenderer != null)
                    {
                        pathRenderer.positionCount = CurrentWorldPoints.Length;
                        pathRenderer.SetPositions(CurrentWorldPoints);
                    }
                }
            }
        }

        #endregion
    }
}
