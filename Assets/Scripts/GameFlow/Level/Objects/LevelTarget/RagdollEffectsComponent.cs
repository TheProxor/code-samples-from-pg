using System.Collections.Generic;
using UnityEngine;

namespace Drawmasters.Levels
{
    public class RagdollEffectsComponent : LevelTargetComponent
    {
        #region Fields

        List<BoneTorque> bonesTorque;

        #endregion



        #region Overrided methods

        public override void Enable()
        {
            RagdollLevelTargetComponent.OnRagdollApplied += RagdollLevelTargetComponent_OnRagdollApplied;
        }

        

        public override void Disable()
        {
            RagdollLevelTargetComponent.OnRagdollApplied -= RagdollLevelTargetComponent_OnRagdollApplied;
        }

        #endregion



        #region Methods

        private void AddTorque(Rigidbody2D bone, float force)
        {
            bool isFlipped = Mathf.Approximately(levelTarget.transform.eulerAngles.y, 180f);

            if (isFlipped)
            {
                force *= -1f;
            }

            bone.AddTorque(force, ForceMode2D.Impulse);
        }


        private void ApplyEffects()
        {
            foreach(var boneTorqueInfo in bonesTorque)
            {
                Rigidbody2D body = levelTarget.Ragdoll2D.GetRigidbody(boneTorqueInfo.boneName);

                if (body != null)
                {
                    AddTorque(body, boneTorqueInfo.force);
                }
            }
        }

        #endregion



        #region Lifecycle

        public RagdollEffectsComponent()
        {
            bonesTorque = IngameData.Settings.levelTargetSettings.bonesTorque;
        }

        #endregion



        #region Events handlers

        private void RagdollLevelTargetComponent_OnRagdollApplied(LevelTarget enemy)
        {
            if (levelTarget.Equals(enemy))
            {
                if (!enemy.Ragdoll2D.IsActive)
                {
                    return;
                }

                ApplyEffects();
            }
        }

        #endregion
    }
}
