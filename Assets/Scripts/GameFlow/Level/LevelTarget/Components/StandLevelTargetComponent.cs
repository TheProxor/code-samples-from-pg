using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;


namespace Drawmasters.Levels
{
    public class StandLevelTargetComponent : LevelTargetComponent
    {
        #region Fields

        public static event Action<LevelTarget> OnStandRigidbodyRemoved;

        private readonly List<Collider2D> standColliders;

        #endregion



        #region Overrided methods

        public override void Initialize(LevelTarget _levelTarget)
        {
            base.Initialize(_levelTarget);

            AddRigidbodyComponent();
            ApplyRigidbodySettings();
        }

        public override void Enable()
        {
            RagdollLevelTargetComponent.OnRagdollApplied += RagdollLevelTargetComponent_OnRagdollApplied;

            if (levelTarget.Type == LevelTargetType.Shooter || levelTarget.Type == LevelTargetType.Boss)
            {
                levelTarget.StandRigidbody.bodyType = RigidbodyType2D.Static;
            }
            else
            {
                levelTarget.StandRigidbody.bodyType = RigidbodyType2D.Dynamic;

                levelTarget.StandRigidbody.freezeRotation = false;

                levelTarget.StandRigidbody.velocity = default;
                levelTarget.StandRigidbody.rotation = default;
                levelTarget.StandRigidbody.angularVelocity = default;
            }

            standColliders.ForEach(c => c.enabled = true);
        }


        public override void Disable()
        {
            RagdollLevelTargetComponent.OnRagdollApplied -= RagdollLevelTargetComponent_OnRagdollApplied;
            standColliders.ForEach(c => c.enabled = false);
        }


        #endregion


        #region Lifecycle

        public StandLevelTargetComponent(List<Collider2D> _standColliders)
        {
            standColliders = _standColliders;
        }

        #endregion



        #region Methods

        private void AddRigidbodyComponent()
        {
            if (levelTarget.StandRigidbody == null)
            {
                levelTarget.StandRigidbody = levelTarget.gameObject.AddComponent<Rigidbody2D>();
            }
        }


        private void ApplyRigidbodySettings()
        {
            levelTarget.StandRigidbody.mass = IngameData.Settings.levelTarget.enemyMass;
            levelTarget.StandRigidbody.gravityScale = levelTarget.IsBoss ?
                IngameData.Settings.levelTarget.bossGravityScale : IngameData.Settings.levelTarget.enemyGravityScale;
        }

        #endregion



        #region Events handlers

        private void RagdollLevelTargetComponent_OnRagdollApplied(LevelTarget appliedLevelTarget)
        {
            if (levelTarget.Equals(appliedLevelTarget))
            {
                standColliders.ForEach(c => c.enabled = false);

                Object.Destroy(levelTarget.StandRigidbody);
                levelTarget.StandRigidbody = null;

                OnStandRigidbodyRemoved?.Invoke(levelTarget);
            }
        }

        #endregion
    }
}
