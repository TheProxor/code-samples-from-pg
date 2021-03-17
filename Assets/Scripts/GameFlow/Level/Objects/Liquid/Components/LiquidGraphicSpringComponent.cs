using System.Collections.Generic;
using Drawmasters.Utils;
using Modules.General;
using UnityEngine;
using UnityEngine.U2D;


namespace Drawmasters.Levels
{
    public class LiquidShapeGraphicsComponent : LiquidComponent
    {
        #region Nested types

        private class NodeData
        {
            public float positionX = default;
            public float positionY = default;
            public float velocity = default;
            public float acceleration = default;
        }

        #endregion



        #region Fields

        private const float wallHeightPart = 1.0f;

        private float halfHeight;
        private List<NodeData> nodeData;
        private LiquidSettings settings;

        #endregion



        #region Methods

        public override void Enable()
        {
            settings = IngameData.Settings.liquidSettings;

            CreateRectSpline();
            InitializeSpringValues();

            RunIdleAnimation();

            
            liquid.CollisionNotifier.OnCustomTriggerEnter2D += CollisionNotifier_OnCustomTriggerEnter2D;
            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
        }


        public override void Disable()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            liquid.CollisionNotifier.OnCustomTriggerEnter2D -= CollisionNotifier_OnCustomTriggerEnter2D;
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
        }


        private void CreateRectSpline()
        {
            if (liquid.LoadedData != null && liquid.Spline != null)
            {
                float halfWidth = liquid.LoadedData.size.x * 0.5f;
                halfHeight = liquid.LoadedData.size.y * 0.5f;

                Vector3 commonLeftTangent = new Vector3(-settings.commonBezieMagnitude, 0.0f, 0.0f);
                Vector3 commonRightTangent = new Vector3(settings.commonBezieMagnitude, 0.0f, 0.0f);

                Spline spline = liquid.Spline;
                
                spline.Clear();

                // left border points
                spline.InsertPointAt(0, new Vector3(-halfWidth, -halfHeight, 0.0f));
                spline.SetTangentMode(0, ShapeTangentMode.Linear);
                spline.SetHeight(0, wallHeightPart);

                spline.InsertPointAt(1, new Vector3(-halfWidth, halfHeight, 0.0f));
                spline.SetTangentMode(1, ShapeTangentMode.Linear);
                spline.SetHeight(1, wallHeightPart);

                // middle
                int middlePointsCount = (int)(liquid.LoadedData.size.x / settings.distanceBetweenPoints) - 1;
                for (int i = 2; i <= middlePointsCount; i++)
                {
                    spline.InsertPointAt(i, new Vector3(i * settings.distanceBetweenPoints - halfWidth, halfHeight, 0.0f));
                    spline.SetTangentMode(i, ShapeTangentMode.Continuous);
                    spline.SetLeftTangent(i, commonLeftTangent);
                    spline.SetRightTangent(i, commonRightTangent);
                    spline.SetHeight(i, wallHeightPart);
                    spline.SetCorner(i, true);
                }

                // right border points
                int lastPointIndex = middlePointsCount + 1;

                spline.InsertPointAt(lastPointIndex, new Vector3(halfWidth, halfHeight, 0f));
                spline.SetTangentMode(lastPointIndex, ShapeTangentMode.Linear);
                spline.SetHeight(lastPointIndex, wallHeightPart);

                spline.InsertPointAt(lastPointIndex + 1, new Vector3(halfWidth, -halfHeight, 0f));
                spline.SetTangentMode(lastPointIndex + 1, ShapeTangentMode.Linear);
                spline.SetHeight(lastPointIndex + 1, wallHeightPart);
            }
        }


        private void InitializeSpringValues()
        {
            Spline spline = liquid.Spline;
            int nodesCount = spline.GetPointCount();

            nodeData = new List<NodeData>(nodesCount);

            for (int i = 0; i < nodesCount; i++)
            {
                Vector3 nodeInitialPosition = spline.GetPosition(i);

                NodeData data = new NodeData
                {
                    positionY = nodeInitialPosition.y,
                    positionX = nodeInitialPosition.x,
                    acceleration = default,
                    velocity = default
                };

                nodeData.Add(data);
            }
        }


        private void Splash(int index, float velocity)
        {
            nodeData[index].velocity += velocity;
        }


        private int NearestSplinePoint(Vector3 triggerWorldPoint)
        {
            int result = default;

            Vector3 triggerLocalPoint = liquid.transform.InverseTransformPoint(triggerWorldPoint);

            float minDistance = float.MaxValue;
            
            Spline spline = liquid.Spline;
            
            for (int i = 0; i < nodeData.Count; i++)
            {
                float distance = Vector2.Distance(triggerLocalPoint, spline.GetPosition(i));

                if (spline.GetPosition(i).x < triggerLocalPoint.x &&
                    distance < minDistance)
                {
                    result = i;
                    minDistance = distance;
                }
            }

            return result;
        }

        #endregion



        #region Events handlers

        public void RunIdleAnimation()
        {
            int nodeIndex = Random.Range(0, nodeData.Count);
            Splash(nodeIndex, settings.idleImpulsMagnitude);

            Scheduler.Instance.CallMethodWithDelay(this, RunIdleAnimation, settings.idleImpulsPeriod);
        }



        private void CollisionNotifier_OnCustomTriggerEnter2D(GameObject go, Collider2D collision)
        {
            Rigidbody2D foundRigidbody = CollisionUtility.FindLevelObjectRigidbody(collision);

            if (foundRigidbody != null)
            {
                int nearestSplinePoint = NearestSplinePoint(collision.transform.position);
                nearestSplinePoint = Mathf.Clamp(nearestSplinePoint, 2, nodeData.Count - 2);
                float impuls = PhysicsCalculation.GetImpulsMagnitude(foundRigidbody.velocity, foundRigidbody.mass) * settings.falledObjectImpulsMultiplier;

                Splash(nearestSplinePoint, -impuls);
            }
        }


        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            for (int i = 2; i < nodeData.Count - 2; i++)
            {
                // -f = k * deltaX + d*v;
                float accelerationDelta = settings.spring * (nodeData[i].positionY - halfHeight) + nodeData[i].velocity * settings.damping;

                nodeData[i].acceleration = -accelerationDelta;
                nodeData[i].positionY += nodeData[i].velocity;
                nodeData[i].velocity += nodeData[i].acceleration;

                liquid.Spline.SetPosition(i, new Vector3(nodeData[i].positionX, nodeData[i].positionY, liquid.Spline.GetPosition(i).z));
            }

            float leftDeltas;
            float rightDeltas;

            for (int j = 0; j < settings.passesCount; j++)
            {
                for (int i = 2; i < nodeData.Count - 2; i++)
                {
                    leftDeltas = settings.spread * (nodeData[i].positionY - nodeData[i - 1].positionY);
                    nodeData[i - 1].velocity += leftDeltas;

                    rightDeltas = settings.spread * (nodeData[i].positionY - nodeData[i + 1].positionY);
                    nodeData[i + 1].velocity += rightDeltas;
                }
            }
        }

        #endregion
    }
}
