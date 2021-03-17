using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using Modules.General;

namespace Drawmasters.Levels
{
    public class RopeCreateComponent : RopeComponent
    {
        #region Fields

        public static event Action<LevelObject, Rigidbody2D, Vector3> OnShouldConnectLevelObject;

        public static event Action<Rope, List<RopeSegment>> OnRopeGenerated;
        public static event Action<Rope> OnRopeDestroyed;
        public static event Action<Rope> OnRopeBecomeEmpty;

        private PhysicalLevelObject hookObject;
        private PhysicalLevelObject mainObject;

        private List<RopeSegment> ropeSegments;

        #endregion



        #region Methods

        public override void Initialize(Rope _rope)
        {
            base.Initialize(_rope);

            rope.OnLinksSet += Rope_OnLinksSet;
            rope.OnPrepareForCome += Rope_OnPrepareForCome;
        }


        public override void Enable()
        {
            ropeSegments = new List<RopeSegment>();

            if (hookObject != null)
            {
                hookObject.OnPreDestroy += HookPhysicalLevelObject_OnPreDestroy;

                rope.Hook.bodyType = RigidbodyType2D.Dynamic;
                rope.Hook.constraints = RigidbodyConstraints2D.None;
            }
            else
            {
                rope.Hook.bodyType = RigidbodyType2D.Kinematic;
                rope.Hook.constraints = RigidbodyConstraints2D.FreezeAll;
            }

            RopeStageComponent.OnObjectsCame += RopeStageComponent_OnObjectsCame;

            rope.Hook.simulated = true;
            GenerateRope();
        }


        public override void Disable()
        {
            RopeStageComponent.OnObjectsCame -= RopeStageComponent_OnObjectsCame;

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            rope.OnLinksSet -= Rope_OnLinksSet;
            rope.OnPrepareForCome -= Rope_OnPrepareForCome;

            DestroyRope();

            if (mainObject != null)
            {
                mainObject.OnPreDestroy -= MainPhysicalLevelObject_OnPreDestroy;
            }

            mainObject = null;
            hookObject = null;
        }


        public void GenerateRope()
        {
            Rigidbody2D previousRB = rope.Hook;

            Vector3 startPosition = (hookObject == null) ? Vector3.zero : hookObject.transform.position;

            Vector2 hookWorldPosition = startPosition - rope.LoadedData.hookAnchorOffset;
            Vector2 endWorldPosition = ((mainObject == null) ? Vector3.zero : mainObject.transform.position) - rope.LoadedData.endAnchorOffset;

            float realDistance = Vector2.Distance(hookWorldPosition, endWorldPosition);
            float ropeLength = Mathf.Max(realDistance, rope.LoadedData.length);
            float segmentsShift = rope.LoadedData.segmentShift;

            int segmentsNumber = Mathf.FloorToInt(ropeLength / segmentsShift);

            Vector2 connectedPosition;

            for (int i = 0; i < segmentsNumber; i++)
            {
                RopeSegment ropeSegment = Content.Management.CreateRopeSegment(rope.transform);

                ropeSegments.Add(ropeSegment);

                float factor = (i + 1) / (float)segmentsNumber;

                Vector3 endPosition = hookWorldPosition + (endWorldPosition - hookWorldPosition).normalized * ropeLength;
                connectedPosition = rope.LoadedData.isSphericalTrajectory ?
                                    Vector3.Slerp(hookWorldPosition, endPosition, factor) : Vector3.Lerp(hookWorldPosition, endPosition, factor);

                ropeSegment.transform.position = connectedPosition;
                connectedPosition = i == 0 ? Vector3.zero : previousRB.transform.InverseTransformPoint(connectedPosition);
                ropeSegment.Initialize(previousRB, connectedPosition);

                if (i < segmentsNumber - 1)
                {
                    previousRB = ropeSegment.MainRigidbody2D;
                }
                else
                {
                    if (mainObject != null)
                    {
                        Vector3 anchor = mainObject.transform.InverseTransformPoint(endWorldPosition);
                        OnShouldConnectLevelObject?.Invoke(mainObject, ropeSegment.MainRigidbody2D, anchor);
                    }

                    ropeSegments.Add(ropeSegment);
                }
            }

            if (hookObject != null)
            {
                Vector3 anchor = hookObject.transform.InverseTransformPoint(hookWorldPosition);
                OnShouldConnectLevelObject?.Invoke(hookObject, rope.Hook, anchor);
            }

            rope.Hook.position = hookWorldPosition;

            Scheduler.Instance.CallMethodWithDelay(this, () =>
            {
                foreach (var i in ropeSegments)
                {
                    i.transform.position = (i == ropeSegments.First()) ? rope.Hook.position : i.ConnectedPosition;
                };
            }, CommonUtility.OneFrameDelay);

            OnRopeGenerated?.Invoke(rope, ropeSegments);

            if (hookObject == null)
            {
                OnRopeBecomeEmpty?.Invoke(rope);
            }
        }


        public void DestroyRope()
        {
            for (int i = 0; i < ropeSegments.Count; i++)
            {
                Content.Management.DestroyObject(ropeSegments[i].gameObject);
            }

            ropeSegments.Clear();

            OnRopeDestroyed?.Invoke(rope);
        }

        #endregion



        #region Events handlers

        private void Rope_OnLinksSet(List<LevelObject> linkedObjects)
        {
            if (linkedObjects.Count > 0)
            {
                mainObject = linkedObjects[0] as PhysicalLevelObject;

                if (mainObject != null)
                {
                    mainObject.OnPreDestroy += MainPhysicalLevelObject_OnPreDestroy;
                }
            }

            if (linkedObjects.Count > 1)
            {
                hookObject = linkedObjects[1] as PhysicalLevelObject;
            }
        }


        private void MainPhysicalLevelObject_OnPreDestroy(PhysicalLevelObject physicalObject)
        {
            if (physicalObject == mainObject)
            {
                physicalObject.OnPreDestroy -= MainPhysicalLevelObject_OnPreDestroy;
                OnRopeBecomeEmpty?.Invoke(rope);
            }
        }


        private void HookPhysicalLevelObject_OnPreDestroy(PhysicalLevelObject physicalObject)
        {
            if (physicalObject == hookObject)
            {
                physicalObject.OnPreDestroy -= HookPhysicalLevelObject_OnPreDestroy;

                hookObject = null;
                rope.Hook.simulated = false;
                OnRopeBecomeEmpty?.Invoke(rope);
            }
        }


        private void Rope_OnPrepareForCome()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            DestroyRope();

            if (mainObject != null)
            {
                mainObject.OnPreDestroy -= MainPhysicalLevelObject_OnPreDestroy;
            }
        }


        private void RopeStageComponent_OnObjectsCame(Rope anotherRope)
        {
            if (anotherRope == rope)
            {
                DestroyRope();

                GenerateRope();

                if (mainObject != null)
                {
                    mainObject.OnPreDestroy += MainPhysicalLevelObject_OnPreDestroy;
                }
            }
        }

        #endregion
    }
}
