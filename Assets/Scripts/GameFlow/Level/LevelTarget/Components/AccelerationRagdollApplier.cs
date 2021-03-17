using UnityEngine;


namespace Drawmasters.Levels
{
    public class AccelerationRagdollApplier : LevelTargetComponent
    {
        #region Fields

        private const int AccelerationFramesRagdoll = 10;
        
        private const float FramesFactor = 0.5f;
        
        private float previousVelocity;
        private int accelerationFrames;
        private Rigidbody2D checkRigidbody;
        private float acceleration;

        #endregion



        #region Overrided methds

        public override void Initialize(LevelTarget _levelTarget)
        {
            base.Initialize(_levelTarget);

            checkRigidbody = levelTarget.StandRigidbody;
            
            acceleration = Physics2D.gravity.magnitude *
                           checkRigidbody.gravityScale *
                           Time.fixedDeltaTime *
                           FramesFactor;
        }


        public override void Enable()
        {
            MonoBehaviourLifecycle.OnFixedUpdate += MonoBehaviourLifecycle_OnUpdate;
            StandLevelTargetComponent.OnStandRigidbodyRemoved += StandLevelTargetComponent_OnStandRigidbodyRemoved;
            levelTarget.OnShouldApplyRagdoll += OnShouldApplyRagdoll;

            accelerationFrames = 0;
            previousVelocity = 0f;
        }


        public override void Disable()
        {
            MonoBehaviourLifecycle.OnFixedUpdate -= MonoBehaviourLifecycle_OnUpdate;
            levelTarget.OnShouldApplyRagdoll -= OnShouldApplyRagdoll;
            StopCheckVelocity();
        }


        private void StopCheckVelocity() =>
                MonoBehaviourLifecycle.OnFixedUpdate -= MonoBehaviourLifecycle_OnUpdate;

        #endregion



        #region Events handlers

        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            // sometimes checkRigidbody is null according to crashlytics
            if (checkRigidbody == null)
            {
                StopCheckVelocity();
                return;
            }

            float currentVelocity = checkRigidbody.velocity.magnitude;
            float velocityDelta = currentVelocity - previousVelocity;

            bool isFalling = (velocityDelta - acceleration) > 0f;

            if (isFalling)
            {
                accelerationFrames++;

                if (accelerationFrames >= AccelerationFramesRagdoll)
                {
                    levelTarget.ApplyRagdoll();
                }
            }
            else
            {
                accelerationFrames = 0;
            }

            previousVelocity = currentVelocity;

        }


        private void OnShouldApplyRagdoll(LevelTarget otherLevelTarget) =>
            StopCheckVelocity();


        private void StandLevelTargetComponent_OnStandRigidbodyRemoved(LevelTarget otherEnemy)
        {
            if (levelTarget.Equals(otherEnemy))
            {
                StopCheckVelocity();
            }
        }

        #endregion
    }

}
