using UnityEngine;


namespace Drawmasters.Levels
{
    public class RotationRagdollApplyLevelTargetComponent : LevelTargetComponent
    {
        #region Fields

        private Rigidbody2D workRigidbody;

        private float maxRotationForRagdoll;

        #endregion



        #region Methods

        public override void Initialize(LevelTarget levelTargetValue)
        {
            base.Initialize(levelTargetValue);

            maxRotationForRagdoll = IngameData.Settings.levelTarget.maxRotationAngleToApplyRagdoll;
        }


        public override void Enable()
        {
            workRigidbody = levelTarget.StandRigidbody;

            MonoBehaviourLifecycle.OnUpdate += MonoBehaviourLifecycle_OnUpdate;
            StandLevelTargetComponent.OnStandRigidbodyRemoved += StandLevelTargetComponent_OnStandRigidbodyRemoved;
        }

        

        public override void Disable()
        {
            MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
            StandLevelTargetComponent.OnStandRigidbodyRemoved -= StandLevelTargetComponent_OnStandRigidbodyRemoved;
        }

        #endregion



        #region Events handlers

        private void MonoBehaviourLifecycle_OnUpdate(float deltaTime)
        {
            if (workRigidbody.rotation >  maxRotationForRagdoll ||
                workRigidbody.rotation < -maxRotationForRagdoll)
            {
                levelTarget.ApplyRagdoll();
                
                MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
            }
        }


        private void StandLevelTargetComponent_OnStandRigidbodyRemoved(LevelTarget otherlevelTarget)
        {
            if (levelTarget.Equals(otherlevelTarget))
            {
                MonoBehaviourLifecycle.OnUpdate -= MonoBehaviourLifecycle_OnUpdate;
            }
        }

        #endregion
    }
}
