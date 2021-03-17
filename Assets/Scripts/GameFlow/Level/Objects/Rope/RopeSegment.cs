using System;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class RopeSegment : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Rigidbody2D mainRigidbody2D = default;
        [SerializeField] private AnchoredJoint2D joint = default;


        private Rigidbody2D previousRB;
        public Vector2 ConnectedPosition { get; private set; }

        private Vector3 savedPos;
        private Quaternion quaternion;

        #endregion



        #region Properties

        public Rigidbody2D MainRigidbody2D => mainRigidbody2D;

        #endregion



        #region Methods

        public void Initialize(Rigidbody2D _previousRB, Vector2 _connectedPosition)
        {
            previousRB = _previousRB;
            ConnectedPosition = _connectedPosition;

            joint.connectedBody = previousRB;
            joint.connectedAnchor = ConnectedPosition;

            savedPos = transform.position;
            quaternion = transform.rotation;

            joint.enabled = false;
        }


        public void MarkAnchorAutoConfigurable()
        {
            transform.position = savedPos;
            transform.rotation = quaternion;

            joint.connectedBody = previousRB;
            joint.connectedAnchor = ConnectedPosition;

            joint.autoConfigureConnectedAnchor = true;
        }

        public void Enable() => joint.enabled = true;
        
        #endregion
    }
}
