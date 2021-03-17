using UnityEngine;
using System.Collections.Generic;


namespace Drawmasters.Levels
{
    public class JointsHandler : IInitializable, IDeinitializable
    {
        #region Helper types

        private class JointInfo
        {
            public Joint2D Joint { get; private set; }

            public GameObject Marker { get; private set; }


            public JointInfo(Joint2D _joint,
                             GameObject _marker)
            {
                Joint = _joint;
                Marker = _marker;
            }
        }

        #endregion



        #region Fields

        private List<Vector3> jointsPoints;
        private readonly Rigidbody2D mainRigidbody;

        private readonly List<JointInfo> jointsInfo;

        private RigidbodyConstraints2D defaultConstraints2D;
        private RigidbodyType2D defaultRigidbodyType2D;

        #endregion



        #region IInitializable

        public void Initialize()
        {
            RememberDefaultConstraints();
            
            bool isEmptyData = jointsPoints == null ||
                               jointsPoints.Count == 0;
            if (isEmptyData)
            {
                return;
            }

            if (jointsPoints.Count == 1)
            {
                mainRigidbody.constraints = RigidbodyConstraints2D.None;
                CreateHingeJoint(jointsPoints[0]);
            }
            else
            {
                CreateFakeJoints(jointsPoints);

                mainRigidbody.velocity = default;
                mainRigidbody.angularVelocity = default;

                mainRigidbody.bodyType = RigidbodyType2D.Kinematic;
                mainRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }

        #endregion



        #region IDeinitializable

        public void Deinitialize()
        {
            foreach (var info in jointsInfo)
            {
                if (info != null)
                {
                    if (info.Joint != null)
                    {
                        info.Joint.enabled = false;

                        Object.Destroy(info.Joint);
                    }

                    if (info.Marker != null)
                    {
                        CommonUtility.SetObjectActive(info.Marker, false);

                        Object.Destroy(info.Marker);
                    }
                }
            }

            jointsInfo.Clear();
            
            RestoreConstraints();
        }

        #endregion


        #region Lifecycle

        public JointsHandler(List<Vector3> _jointsPoints,
                             Rigidbody2D _mainRigidbody)
        {
            mainRigidbody = _mainRigidbody;

            jointsInfo = new List<JointInfo>();
            SetupJointPoints(_jointsPoints);
        }

        #endregion



        #region Methods
        
        public void SetupJointPoints(List<Vector3> _jointsPoints)
        {
            jointsPoints = _jointsPoints;

            jointsInfo.Clear();
        }


        private void CreateFakeJoints(List<Vector3> relativePositions)
        {
            foreach (var relative in relativePositions)
            {
                Vector2 localPoint = TransformPoint(relative);

                GameObject marker = CreateMarker(localPoint);

                jointsInfo.Add(new JointInfo(null, marker));
            }
        }


        private void CreateHingeJoint(Vector3 relativePosition)
        {
            Vector3 localPosition = TransformPoint(relativePosition);

            HingeJoint2D hinge = mainRigidbody.gameObject.AddComponent<HingeJoint2D>();
            hinge.connectedBody = null;
            hinge.anchor = localPosition;
            hinge.connectedAnchor = localPosition;
            hinge.enableCollision = true;

            GameObject marker = CreateMarker(localPosition);

            jointsInfo.Add(new JointInfo(hinge, marker));
        }


        private Vector3 TransformPoint(Vector3 realtivePoint)
        {
            Transform parent = mainRigidbody.transform;

            Vector3 worldPoint = realtivePoint + parent.position;
            Vector3 localPoint = parent.InverseTransformPoint(worldPoint);

            return localPoint;
        }


        private GameObject CreateMarker(Vector3 localPosition)
        {
            GameObject originalPrefab = Content.Storage.PrefabByType(CustomPrefabType.JointMarker);

            GameObject marker = Content.Management.CreateObject(originalPrefab);
            marker.transform.parent = mainRigidbody.transform;
            marker.transform.localPosition = localPosition;
            marker.transform.localScale = Vector3.one;

            return marker;
        }

        #endregion



        #region Private methods

        private void RememberDefaultConstraints()
        {
            defaultConstraints2D = mainRigidbody.constraints;
            defaultRigidbodyType2D = mainRigidbody.bodyType;
        }


        private void RestoreConstraints()
        {
            mainRigidbody.constraints = defaultConstraints2D;
            mainRigidbody.bodyType = defaultRigidbodyType2D;
        }

        #endregion
    }
}
