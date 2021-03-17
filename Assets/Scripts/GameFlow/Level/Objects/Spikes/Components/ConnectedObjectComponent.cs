using System;
using UnityEngine;
using Object = UnityEngine.Object;


namespace Drawmasters.Levels
{
    public class ConnectedObjectComponent : PhysicalLevelObjectComponent
    {
        #region Fields

        public static Action<PhysicalLevelObject> OnShouldCorrose;
        public static Action<PhysicalLevelObject> OnShouldLaserDestroy;

        [SerializeField] private Collider2D[] colliders2D = default;

        private FixedJoint2D fixedJoint2D;
        private PhysicalLevelObject connectedPhysicalObject;

        #endregion



        #region Methods

        public override void Initialize(CollisionNotifier notifier, Rigidbody2D rigidbody, PhysicalLevelObject sourceObject)
        {
            base.Initialize(notifier, rigidbody, sourceObject);

            if (sourceLevelObject is Spikes spikes)
            {
                spikes.OnShouldConnectObject += Spikes_OnShouldConnectObject;
            }
        }


        public override void Enable()
        {
            sourceLevelObject.OnPreDestroy += SourceLevelObject_OnPreDestroy;
            LaserDestroyComponent.OnStartDestroy += LaserDestroyComponent_OnStartDestroy;
        }


        public override void Disable()
        {
            LaserDestroyComponent.OnStartDestroy -= LaserDestroyComponent_OnStartDestroy;

            if (sourceLevelObject is Spikes spikes)
            {
                spikes.OnShouldConnectObject -= Spikes_OnShouldConnectObject;
            }

            if (fixedJoint2D != null)
            {
                Object.Destroy(fixedJoint2D);
                fixedJoint2D = null;

                SetConnectedCollisionIgnore(false);
            }

            ResetConnectedObject();
        }



        private void SetConnectedCollisionIgnore(bool ignore)
        {
            if (connectedPhysicalObject != null)
            {
                foreach (var spikeCollider in colliders2D)
                {
                    Physics2D.IgnoreCollision(spikeCollider, connectedPhysicalObject.MainCollider2D, ignore);
                }
            }
        }


        private void ResetConnectedObject()
        {
            if (connectedPhysicalObject != null)
            {
                connectedPhysicalObject.OnPreDestroy -= ConnectedPhysicalObject_OnPreDestroy;
                connectedPhysicalObject = null;
            }
        }

        #endregion



        #region Events handlers

        private void Spikes_OnShouldConnectObject(PhysicalLevelObject objectToLink)
        {
            if (fixedJoint2D != null)
            {
                CustomDebug.Log("Multiply connected. Spikes will be connected to the first linked object");
            }

            fixedJoint2D = sourceLevelObject.gameObject.AddComponent<FixedJoint2D>();
            fixedJoint2D.connectedBody = objectToLink.Rigidbody2D;

            connectedPhysicalObject = objectToLink;
            connectedPhysicalObject.OnPreDestroy += ConnectedPhysicalObject_OnPreDestroy;

            SetConnectedCollisionIgnore(true);
        }


        private void SourceLevelObject_OnPreDestroy(PhysicalLevelObject obj)
        {
            if (connectedPhysicalObject != null)
            {
                connectedPhysicalObject.OnPreDestroy -= ConnectedPhysicalObject_OnPreDestroy;
                connectedPhysicalObject.DestroyObject();

                connectedPhysicalObject = null;
            }
        }


        private void LaserDestroyComponent_OnStartDestroy(PhysicalLevelObject physicalLevelObject)
        {
            bool isConnectedObjectsStartedDestroy = (physicalLevelObject == sourceLevelObject) ||
                                                    (physicalLevelObject == connectedPhysicalObject);

            PhysicalLevelObject objectToDestroy = default;

            if (physicalLevelObject == sourceLevelObject)
            {
                objectToDestroy = connectedPhysicalObject;
            }

            if (physicalLevelObject == connectedPhysicalObject)
            {
                objectToDestroy = sourceLevelObject;
            }

            if (isConnectedObjectsStartedDestroy)
            {
                LaserDestroyComponent.OnStartDestroy -= LaserDestroyComponent_OnStartDestroy;
                OnShouldLaserDestroy?.Invoke(objectToDestroy);
            }
        }


        private void ConnectedPhysicalObject_OnPreDestroy(PhysicalLevelObject levelObjects)
        {
            if (connectedPhysicalObject.Equals(levelObjects))
            {
                ResetConnectedObject();
            }

            sourceLevelObject.DestroyObject();
        }

        #endregion
    }
}
