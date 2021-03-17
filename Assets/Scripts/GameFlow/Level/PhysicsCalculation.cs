using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Levels
{
    public static class PhysicsCalculation
    {
        #region Methods

        /// <summary>
        /// Return impuls before collision and physics calculation for two rigidbodies 2D. Should use on collision to calculate damage done
        /// </summary>
        public static float GetImpulsMagnitude(PreviousFrameRigidbody2D firstRigidbody2D, PreviousFrameRigidbody2D secondRigidbody2D)
        {
            Vector2 resultVelocityVector = firstRigidbody2D.Velocity - secondRigidbody2D.Velocity;
            float averageMass = (firstRigidbody2D.Mass + secondRigidbody2D.Mass) * 0.5f;

            return GetImpulsMagnitude(resultVelocityVector, averageMass);
        }


        public static float GetImpulsFromEnemy(PreviousFrameRigidbody2D thisPreviousFrameRigidbody,
                                               PreviousFrameRigidbody2D enemyPreviousFrameRigidbody,
                                               float enemyMass)
        {
            Vector2 velocity = thisPreviousFrameRigidbody.Velocity - enemyPreviousFrameRigidbody.Velocity;

            return GetImpulsMagnitude(velocity, enemyMass) * IngameData.Settings.levelTarget.physicalObjectDamageMultiplier;
        }


        public static float GetImpulsMagnitudeWithMonolith(PreviousFrameRigidbody2D firstRigidbody2D)
        {
            float averageMass = (firstRigidbody2D.Mass + IngameData.Settings.monolith.mass) * 0.5f;

            return GetImpulsMagnitude(firstRigidbody2D.Velocity, averageMass);
        }


        public static Vector2 GetCollisionImpulsForObject(Projectile projectile, float impulsMagnitude)
        {
            Vector2 result = default;

            if (projectile != null)
            {
                result = projectile.PreviousFrameRigidbody2D.Velocity.normalized * impulsMagnitude;
            }

            return result;
        }


        public static Vector2 GetLimbPartImpuls(Projectile projectile, float impulsMagnitude)
        {
            return projectile.PreviousFrameRigidbody2D.Velocity.normalized * impulsMagnitude;
        }


        public static float GetImpulsMagnitude(Vector2 velocity, float mass) => velocity.magnitude * mass;


        public static float CalculateLimbReceivedDamage(PhysicalLevelObject physicalLevelObject, LevelTargetLimb limb, LevelTarget levelTarget)
        {
            float pureDamage = CalculateLimbReceivedPureDamage(physicalLevelObject, limb, levelTarget);
            float damageMultiplier = IngameData.Settings.physicalObject.GetDamageImpulsMultiplier(physicalLevelObject.PhysicalData);
            float damageToReceive = pureDamage * damageMultiplier;

            return damageToReceive;
        }


        public static float CalculateLimbReceivedDamageForChopOffAll(PhysicalLevelObject physicalLevelObject, LevelTargetLimb limb, LevelTarget levelTarget)
        {
            float pureDamage = CalculateLimbReceivedPureDamage(physicalLevelObject, limb, levelTarget);
            float damageMultiplier = IngameData.Settings.physicalObject.GetAllLimbsChopOffDamageImpulsMultiplier(physicalLevelObject.PhysicalData);
            float damageToReceive = pureDamage * damageMultiplier;

            return damageToReceive;
        }

        /// <summary>
        /// return damage to level target by physical object without multiplier
        /// </summary>
        /// <returns></returns>
        public static float CalculateLimbReceivedPureDamage(PhysicalLevelObject physicalLevelObject, LevelTargetLimb limb, LevelTarget levelTarget)
        {
            float totalMass = default;
            int massCounter = default;

            totalMass += physicalLevelObject.Rigidbody2D.mass;
            massCounter++;

            Vector2 averageLimbVelocityVector = default;

            List<string> allLimPartsBones = limb.PartsBonesNames;
            foreach (var bonePartName in allLimPartsBones)
            {
                Rigidbody2D foundRigidbody2D = levelTarget.Ragdoll2D.GetRigidbody(bonePartName);

                if (foundRigidbody2D != null)
                {
                    totalMass += foundRigidbody2D.mass;
                    averageLimbVelocityVector += foundRigidbody2D.velocity;
                    massCounter++;
                }
            }

            Vector2 resultVelocityVector = physicalLevelObject.PreviousFrameRigidbody2D.Velocity - averageLimbVelocityVector;
            float damageToReceive = (totalMass / massCounter) * resultVelocityVector.magnitude;

            return damageToReceive;
        }


        public static float CalculateLimbReceivedDamageFromMonolit(LevelTargetLimb limb, LevelTarget levelTarget)
        {
            float totalMass = default;
            int massCounter = default;

            totalMass += IngameData.Settings.monolith.mass;
            massCounter++;

            Vector2 totalLimbVelocityVector = default;

            List<string> allLimPartsBones = limb.PartsBonesNames;
            foreach (var bonePartName in allLimPartsBones)
            {
                Rigidbody2D foundRigidbody2D = levelTarget.Ragdoll2D.GetRigidbody(bonePartName);

                if (foundRigidbody2D != null)
                {
                    totalMass += foundRigidbody2D.mass;
                    totalLimbVelocityVector += foundRigidbody2D.velocity;
                    massCounter++;
                }
            }

            float damageMultiplier = IngameData.Settings.monolith.damageConversionImpulsMultiplier;
            float damageToReceive = (totalMass / massCounter) * totalLimbVelocityVector.magnitude * damageMultiplier;

            return damageToReceive;
        }


        public static float CalculateLimbDamageFromRigidbody(LevelTargetLimb limb,
                                                             LevelTarget levelTarget,
                                                             Rigidbody2D anotherRigidbody)
        {
            if (anotherRigidbody == null)
            {
                return default;
            }

            float totalMass = default;
            int massCounter = default;

            float damageMultiplier = IngameData.Settings.levelTarget.limbPartDamageReceiveImpulsMultiplier;

            totalMass += anotherRigidbody.mass;
            massCounter++;

            Vector2 totalLimbVelocityVector = default;

            List<string> allLimPartsBones = limb.PartsBonesNames;
            foreach (var bonePartName in allLimPartsBones)
            {
                Rigidbody2D foundRigidbody2D = levelTarget.Ragdoll2D.GetRigidbody(bonePartName);

                if (foundRigidbody2D != null)
                {
                    totalMass += foundRigidbody2D.mass;
                    totalLimbVelocityVector += foundRigidbody2D.velocity;
                    massCounter++;
                }
            }

            Vector2 resultVelocityVector = anotherRigidbody.velocity - totalLimbVelocityVector;

            float damageToReceive = (totalMass / massCounter) * resultVelocityVector.magnitude * damageMultiplier;

            return damageToReceive;
        }


        public static float AverageLimbVelocityMagnitude(LevelTargetLimb limb, LevelTarget levelTarget)
        {
            float result = default;
            int count = default;

            List<string> allLimPartsBones = limb.PartsBonesNames;
            foreach (var bonePartName in allLimPartsBones)
            {
                Rigidbody2D foundRigidbody2D = levelTarget.Ragdoll2D.GetRigidbody(bonePartName);

                if (foundRigidbody2D != null)
                {
                    result += foundRigidbody2D.velocity.magnitude;
                    count++;
                }
            }

            if (!Mathf.Approximately(count, 0.0f))
            {
                result /= count;
            }

            return result;
        }


        // stoled from rocket buddy
        public static float CalculateAutoAimProjectileSpeed(Vector2 aimVector, ArcAimSettings aimSettings, float gravityScale)
        {
            float distanceFactor = Mathf.InverseLerp(aimSettings.distanceForMinFlyTime, aimSettings.distanceForMaxFlyTime, Mathf.Abs(aimVector.y));
            float factor = AnimationUtility.LinearCurve.Evaluate(distanceFactor);
            float currentFlyTime = Mathf.Lerp(aimSettings.minProjectileFlyTime, aimSettings.maxProjectileFlyTime, factor);

            float angleInRad = CalculateAngle(aimVector, aimSettings, gravityScale);

            return aimVector.x / currentFlyTime / Mathf.Cos(angleInRad);
        }


        public static float CalculateAngle(Vector3 direction, ArcAimSettings aimSettings, float gravityScale)
        {
            float distanceFactor = Mathf.InverseLerp(aimSettings.distanceForMinFlyTime, aimSettings.distanceForMaxFlyTime, Mathf.Abs(direction.y));
            float factor = AnimationUtility.LinearCurve.Evaluate(distanceFactor);
            float flyTime = Mathf.Lerp(aimSettings.minProjectileFlyTime, aimSettings.maxProjectileFlyTime, factor);

            bool isInverted = direction.x < 0.0f;
            direction.x = Mathf.Abs(direction.x);

            float angle = Mathf.Atan((2.0f * direction.y - Physics2D.gravity.y * gravityScale * flyTime * flyTime) / (2.0f * direction.x));
            angle = isInverted ? Mathf.PI - angle : angle;
            return angle;
        }


        public static float CalculateAcceleration(float beginVelocity, float endVelocity, float time)
        {
            time = Mathf.Max(time, float.Epsilon);

            return (endVelocity - beginVelocity) / time;
        }


        #endregion
    }
}
