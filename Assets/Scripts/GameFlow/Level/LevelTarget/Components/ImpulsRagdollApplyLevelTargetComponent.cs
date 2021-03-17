using System;
using System.Collections.Generic;
using UnityEngine;

namespace Drawmasters.Levels
{
    public class ImpulsRagdollApplyLevelTargetComponent : LevelTargetComponent
    {
        #region Fields

        public static event Action<string> OnShouldLog;

        private List<LevelTargetLimb> levelTargetLimbs = default;

        private static bool shouldEnableLogs = false;

        #endregion



        #region Unity lifecycle

        public ImpulsRagdollApplyLevelTargetComponent(List<LevelTargetLimb> _levelTargetLimbs)
        {
            levelTargetLimbs = _levelTargetLimbs;
        }

        #endregion



        #region Methods

        public override void Enable()
        {
            foreach (var limb in levelTargetLimbs)
            {
                limb.OnCollidableObjectHitted += Limb_OnCollidableObjectHitted;
            }

            RagdollLevelTargetComponent.OnRagdollApplied += RagdollLevelTargetComponent_OnRagdollApplied;
        }


        public override void Disable()
        {
            UnsubscribeFromLimbsEvent();
        }


        private void UnsubscribeFromLimbsEvent()
        {
            foreach (var limb in levelTargetLimbs)
            {
                limb.OnCollidableObjectHitted -= Limb_OnCollidableObjectHitted;
            }

            RagdollLevelTargetComponent.OnRagdollApplied -= RagdollLevelTargetComponent_OnRagdollApplied;
        }

        #endregion



        #region Events handlers

        private float GetPhysicalObjectImpuls(CollidableObject collidable,
                                              LevelTargetLimb limb)
        {
            float result = default;

            PhysicalLevelObject levelObject = collidable.PhysicalLevelObject;

            if (levelObject != null)
            {
                result = PhysicsCalculation.GetImpulsMagnitude(levelTarget.StandPreviousFrameRigidbody2D,
                                                               levelObject.PreviousFrameRigidbody2D);
            }

            return result;
        }


        private float GetOtherEnemyImpuls(CollidableObject collidable,
                                          LevelTargetLimb limb)
        {
            float result = default;

            LevelTarget otherEnemy = collidable.LevelTarget;

            if (otherEnemy != null)
            {
                PreviousFrameRigidbody2D thisPrevRb = limb.ParentEnemy.StandPreviousFrameRigidbody2D;
                PreviousFrameRigidbody2D otherPrevRb = default;

                if (otherEnemy.Ragdoll2D.IsActive)
                {
                    Rigidbody2D limbRigidbody = otherEnemy.Ragdoll2D.GetRigidbody(limb.RootBoneName);

                    if (limbRigidbody != null)
                    {
                        otherPrevRb = otherEnemy.RigidbodyPairs.GetPreviousRigidbody(limbRigidbody);
                    }
                }
                else
                {
                    otherPrevRb = otherEnemy.StandPreviousFrameRigidbody2D;
                }

                if (thisPrevRb != null &&
                    otherPrevRb != null)
                {
                    result = PhysicsCalculation.GetImpulsMagnitude(thisPrevRb,
                                                                   otherPrevRb);
                }
            }

            return result;
        }

        private void Limb_OnCollidableObjectHitted(CollidableObject collidableObject, LevelTargetLimb limb)
        {
            float impulsPower = default;

            switch (collidableObject.Type)
            {
                case CollidableObjectType.PhysicalObject:
                    {
                        impulsPower = GetPhysicalObjectImpuls(collidableObject, limb);
                    }
                    break;
                case CollidableObjectType.EnemyTrigger:
                    {
                        impulsPower = GetOtherEnemyImpuls(collidableObject, limb);
                    }
                    break;
                default:
                    break;
            }

            if (impulsPower > IngameData.Settings.levelTarget.physicalsObjectsImpulsToApplyRagdoll)
            {
                levelTarget.ApplyRagdoll();
            }

#if UNITY_EDITOR
                Logging(impulsPower,
                        collidableObject.gameObject,
                        limb.gameObject);
#endif
        }


        private void RagdollLevelTargetComponent_OnRagdollApplied(LevelTarget appliedLevelTarget)
        {
            if (levelTarget.Equals(appliedLevelTarget))
            {
                UnsubscribeFromLimbsEvent();
            }
        }

        #endregion



        #region Editor
#if UNITY_EDITOR

        private void Logging(float impulsPower,
                             GameObject collidableObject,
                             GameObject limb)
        {
            if (shouldEnableLogs)
            {
                CustomDebug.Log($"Impuls power is {impulsPower} that create object {collidableObject.name} on level target {levelTarget.gameObject}");
            }

            string logText = $"Limb {limb.name} get impuls {impulsPower}. from {collidableObject.name}";
            OnShouldLog?.Invoke(logText);

            if (impulsPower >= IngameData.Settings.levelTarget.physicalsObjectsImpulsToApplyRagdoll)
            {
                levelTarget.ApplyRagdoll();
            }
        }

#endif
        #endregion
    }
}
