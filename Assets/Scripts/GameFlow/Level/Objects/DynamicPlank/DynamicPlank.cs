using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Levels.Objects
{
    public class DynamicPlank : LevelObject
    {
        #region Nested types

        public enum CycleType
        {
            None = 0,
            YoYo = 1,
            Circle = 2
        }

        [Serializable]
        public class PathPoint
        {
            public Vector3 position;
            public Vector3 rotation;
        }


        [Serializable]
        public class SerializableData
        {
            public CycleType cycleType;
            public float speed;
            public List<PathPoint> path;
        }

        #endregion



        #region Fields

        const float MinDistanceToPoint = 0.01f;
        const float Mass = 200.0f;
        const float SecondsCountToCheckInactivity = 1f;
        const float InactivitySpeedCoeff = 0.05f;

        [Space]
        [Header("DynamicPlank")]
        [SerializeField] Rigidbody2D body = null;
        [Space]
        [SerializeField] Renderer bodyGears = default;
        [SerializeField] LineRenderer wayRendererPrefab = null;
        [Space]
        [SerializeField] GameObject waysGearPrefabM = null;
        [SerializeField] GameObject waysGearPrefabS = null;
        [Space]
        [SerializeField] Vector3 wayOffset = default;
        [SerializeField] Vector3 gearOffset = default;

        readonly List<Renderer> wayRenderers = new List<Renderer>();
        readonly List<GameObject> createdGears = new List<GameObject>();
        readonly List<Renderer> createdGearsRenderers = new List<Renderer>();
        readonly List<FixedJoint> joints = new List<FixedJoint>();
        readonly List<LevelObject> objectsWithAddedTag = new List<LevelObject>();

        CycleType cycleType;


        int nextPathPointIndex;
        Vector3 targetPosition;

        Quaternion targetRotation;
        Quaternion previousRotation;

        float distanceBetweenClosestPoints;

        List<PathPoint> path;

        bool isMovingForward;

        Vector3 lastFrameStartPosition;

        float inactivityDistanceSqr;
        float inactivityTimer;
        float speed;

        static WaitForFixedUpdate wait = new WaitForFixedUpdate();

        Coroutine movementCoroutine;

        #endregion



        #region Unity lifecycle

        protected override void Awake()
        {
            base.Awake();

            Rigidbody2D.mass = Mass;
        }


        void FixedUpdate()
        {
            Rigidbody2D.velocity = Vector3.zero;
        }

        #endregion



        #region Public methods

        public override void SetData(LevelObjectData data)
        {
            base.SetData(data);

            SerializableData loadedData = JsonUtility.FromJson<SerializableData>(data.additionalInfo);

            cycleType = loadedData.cycleType;
            speed = loadedData.speed;

            path = new List<PathPoint>(loadedData.path);
            path.Insert(0, new PathPoint { position = body.transform.position, rotation = body.transform.eulerAngles });

            bodyGears.gameObject.SetActive(false);

            for (int i = 1; i < path.Count; i++)
            {
                if (path[i - 1].rotation != path[i].rotation)
                {
                    bodyGears.gameObject.SetActive(true);
                    break;
                }
            }

            for (int i = 0; i < path.Count - 1; i++)
            {
                LineRenderer wayRenderer = Instantiate(wayRendererPrefab, transform);
                wayRenderer.transform.rotation = wayRendererPrefab.transform.rotation;
                wayRenderer.positionCount = 2;
                wayRenderer.SetPosition(0, path[i].position + wayOffset);
                wayRenderer.SetPosition(1, path[i + 1].position + wayOffset);
                wayRenderers.Add(wayRenderer);
            }

            if (cycleType == CycleType.Circle)
            {
                LineRenderer wayRenderer = Instantiate(wayRendererPrefab, transform);
                wayRenderer.transform.rotation = wayRendererPrefab.transform.rotation;
                wayRenderer.positionCount = 2;
                wayRenderer.SetPosition(0, path[path.Count - 1].position + wayOffset);
                wayRenderer.SetPosition(1, path[0].position + wayOffset);
                wayRenderers.Add(wayRenderer);
            }

            for (int i = 0; i < path.Count; i++)
            {
                GameObject gearPrefab = (cycleType != CycleType.Circle && (i == 0 || i == path.Count - 1)) ? waysGearPrefabM : waysGearPrefabS;
                GameObject go = Instantiate(gearPrefab, path[i].position + gearOffset, gearPrefab.transform.rotation, transform);
                createdGearsRenderers.AddRange(go.GetComponentsInChildren<Renderer>());
                createdGears.Add(go);
            }
        }


        public override void FinishGame()
        {
            base.FinishGame();

            Rigidbody2D.isKinematic = false;
            body.transform.localPosition = Vector3.zero;
            body.transform.localRotation = Quaternion.identity;

            wayRenderers.ForEach((renderer) => Destroy(renderer.gameObject));
            wayRenderers.Clear();

            createdGears.ForEach((gear) => Destroy(gear.gameObject));
            createdGears.Clear();
            createdGearsRenderers.Clear();

            objectsWithAddedTag.Clear();

            joints.ForEach((joint) => Destroy(joint));
            joints.Clear();

            if (movementCoroutine != null)
            {
                StopCoroutine(movementCoroutine);
                movementCoroutine = null;
            }
        }

        #endregion



        #region Protected methods

        protected override void GetPhysicalBody()
        {
            Rigidbody2D = body;
        }

        #endregion



        #region Private methods

        void Init()
        {
            Rigidbody2D.isKinematic = false;

            isMovingForward = true;
            inactivityDistanceSqr = Mathf.Pow(speed * Time.fixedDeltaTime * InactivitySpeedCoeff, 2);

            nextPathPointIndex = 0;
            MoveToNextPoint();

            inactivityTimer = 0.0f;

            movementCoroutine = StartCoroutine(Movement());
        }


        IEnumerator Movement()
        {
            while (true)
            {
                if ((body.transform.position - lastFrameStartPosition).sqrMagnitude < inactivityDistanceSqr)
                {
                    inactivityTimer += Time.fixedDeltaTime;

                    if (inactivityTimer > SecondsCountToCheckInactivity)
                    {
                        if ((cycleType == CycleType.None) || (cycleType == CycleType.Circle))
                        {
                            yield break;
                        }

                        isMovingForward = !isMovingForward;
                        MoveToNextPoint();
                    }
                }
                else
                {
                    inactivityTimer = 0.0f;
                }

                lastFrameStartPosition = body.transform.position;

                Vector3 newPosition = Vector3.MoveTowards(body.transform.position, targetPosition, speed * Time.fixedDeltaTime);
                Rigidbody2D.MovePosition(newPosition);

                float distanceToPoint = (body.transform.position - targetPosition).magnitude;

                Quaternion newRotation = Quaternion.Lerp(previousRotation, targetRotation, 1.0f - distanceToPoint / distanceBetweenClosestPoints);

                Rigidbody2D.MoveRotation(newRotation.eulerAngles.z);

                if (distanceToPoint < MinDistanceToPoint)
                {
                    MoveToNextPoint();
                }

                yield return wait;
            }
        }


        void MoveToNextPoint()
        {
            int previousPathPointIndex = nextPathPointIndex;
            if (isMovingForward)
            {
                nextPathPointIndex++;

                if (nextPathPointIndex == path.Count)
                {
                    switch (cycleType)
                    {
                        case CycleType.None:
                            return;

                        case CycleType.YoYo:
                            isMovingForward = false;
                            nextPathPointIndex = previousPathPointIndex - 1;
                            break;

                        case CycleType.Circle:
                            nextPathPointIndex = 0;
                            break;
                    }
                }
            }
            else
            {
                nextPathPointIndex--;

                if (nextPathPointIndex < 0)
                {
                    switch (cycleType)
                    {
                        case CycleType.None:
                            return;

                        case CycleType.YoYo:
                            isMovingForward = true;
                            nextPathPointIndex = 1;
                            break;

                        case CycleType.Circle:
                            nextPathPointIndex = path.Count - 1;
                            break;
                    }
                }
            }


            targetPosition = path[nextPathPointIndex].position;
            targetRotation = Quaternion.Euler(path[nextPathPointIndex].rotation);

            Vector3 previousPosition = path[previousPathPointIndex].position;
            previousRotation = Quaternion.Euler(path[previousPathPointIndex].rotation);

            distanceBetweenClosestPoints = (targetPosition - previousPosition).magnitude;
        }

        #endregion
    }
}
