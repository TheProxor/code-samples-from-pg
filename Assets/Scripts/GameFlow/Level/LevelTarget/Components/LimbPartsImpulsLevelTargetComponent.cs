using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class LimbPartsImpulsLevelTargetComponent : LevelTargetComponent
    {
        #region Fields

        public static event Action<string> OnShouldLog;

        private List<LevelTargetLimbPart> limbsParts = default;

        #endregion



        #region Lifecycle

        public LimbPartsImpulsLevelTargetComponent(List<LevelTargetLimbPart> _limbsParts)
        {
            limbsParts = _limbsParts;
        }

        #endregion



        #region Methods

        public override void Enable()
        {
            foreach (var part in limbsParts)
            {
                part.OnCollidableObjectHitted += Part_OnCollidableObjectHitted;
            }
        }


        public override void Disable()
        {
            foreach (var part in limbsParts)
            {
                part.OnCollidableObjectHitted -= Part_OnCollidableObjectHitted;
            }
        }

        #endregion



        #region Events handlers

        private void Part_OnCollidableObjectHitted(CollidableObject collidableObject, LevelTargetLimbPart part)
        {
            Projectile projectile = collidableObject.Projectile;

            if (projectile == null)
            {
                return;
            }

            Rigidbody2D foundRigidbody2D = levelTarget.Ragdoll2D.GetRigidbody(part.BoneName);

            if (foundRigidbody2D != null)
            {
                LevelTargetSettings.LimbPartData foundData = levelTarget.Settings.FindLimbPartData(part.BoneName);

                if (foundData != null)
                {
                    LevelTargetSettings.ProjectileData projectileData = levelTarget.Settings.FindProjectileData(projectile.Type);
                    float projectileCollisionImpuls = (projectileData == null) ? default : projectileData.projectileCollisionImpuls;

                    float impulsMagnitude = projectileCollisionImpuls * foundData.impulsMultiplier;
                    Vector2 impulsToAdd = PhysicsCalculation.GetLimbPartImpuls(projectile, impulsMagnitude);

                    foundRigidbody2D.AddForce(impulsToAdd, ForceMode2D.Impulse);

                    string logText = $"Bone {part.BoneName} get impuls {impulsToAdd.magnitude} from projectile";

                    OnShouldLog?.Invoke(logText);
                }
            }
        }

        #endregion
    }
}
