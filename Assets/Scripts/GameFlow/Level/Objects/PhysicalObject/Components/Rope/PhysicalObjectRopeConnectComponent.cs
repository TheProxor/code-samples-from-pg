using Modules.General;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class PhysicalObjectRopeConnectComponent : PhysicalLevelObjectComponent
    {
        #region Fields

        private HingeJoint2D joint;


        #endregion



        #region Methods

        public override void Initialize(CollisionNotifier notifier, 
            Rigidbody2D rigidbody, 
            PhysicalLevelObject sourceObject)
        {
            base.Initialize(notifier, rigidbody, sourceObject);
            
            if (joint != null)
            {
                joint.enabled = false;
            }
            
            RopeCreateComponent.OnShouldConnectLevelObject += RopeCreateComponent_OnShouldConnectLevelObject;
        }

        public override void Enable(){ }


        public override void Disable()
        {
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
            
            RopeCreateComponent.OnShouldConnectLevelObject -= RopeCreateComponent_OnShouldConnectLevelObject;

            if (!joint.IsNull())
            {
                joint.enabled = false;
            }
        }

        #endregion



        #region Events handlers

        private void RopeCreateComponent_OnShouldConnectLevelObject(LevelObject levelObject, Rigidbody2D ropeEndRigidbody2D, Vector3 anchor)
        {
            if (levelObject == sourceLevelObject &&
                ropeEndRigidbody2D != null)
            {
                if (joint == null)
                {
                    joint = sourceLevelObject.gameObject.AddComponent<HingeJoint2D>();
                }

                joint.autoConfigureConnectedAnchor = false;
                joint.connectedBody = ropeEndRigidbody2D;
                joint.anchor = anchor;
                joint.connectedAnchor = Vector2.zero;
                joint.enabled = false;

                Scheduler.Instance.CallMethodWithDelay(this, () =>
                {
                    joint.enabled = true;
                }, CommonUtility.OneFrameDelay);

            }
        }

        #endregion
    }
}
