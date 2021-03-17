using System;
using System.Collections.Generic;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class PhysicalObjectImpulsDestroyComponent : ImpulseDestroyComponent
    {
        #region Fields

        private static readonly HashSet<PhysicalLevelObjectType> OtherObjectsSet =
             new HashSet<PhysicalLevelObjectType> { PhysicalLevelObjectType.Wood,
                                                    PhysicalLevelObjectType.Metal,
                                                    PhysicalLevelObjectType.Dynamite };

        private static readonly HashSet<PhysicalLevelObjectType> PetsDestroyTypes =
             new HashSet<PhysicalLevelObjectType> { PhysicalLevelObjectType.Wood,
                                                    PhysicalLevelObjectType.Dynamite };
        #endregion



        #region Overrided methods

        public override void Enable()
        {
            base.Enable();

            if (collisionNotifier != null)
            {
                collisionNotifier.OnCustomTriggerEnter2D += CollisionNotifier_OnCustomTriggerEnter2D;
            }
        }


        public override void Disable()
        {
            if (collisionNotifier != null)
            {
                collisionNotifier.OnCustomTriggerEnter2D -= CollisionNotifier_OnCustomTriggerEnter2D;
            }

            base.Disable();
        }


        protected override void HandleCollision(GameObject go, GameObject otherGameObject)
        {
            CollidableObject collidable = otherGameObject.GetComponent<CollidableObject>();
            if (collidable == null)
            {
                return;
            }

            if (collidable.Type == CollidableObjectType.PhysicalObject ||
                collidable.Type == CollidableObjectType.Spikes)
            {
                PhysicalLevelObject physicalLevelObject = collidable.PhysicalLevelObject;
                if (physicalLevelObject == null)
                {
                    return;
                }

                bool isHandling = OtherObjectsSet.Contains(physicalLevelObject.PhysicalData.type);
                if (!isHandling)
                {
                    return;
                }

                HandlePhysicalObject(physicalLevelObject);
            }
            else if (collidable.Type == CollidableObjectType.EnemyTrigger)
            {
                LevelTarget enemy = collidable.LevelTarget;
                if (enemy == null)
                {
                    return;
                }

                PreviousFrameRigidbody2D handler;
                float mass;

                if (enemy.Ragdoll2D.IsActive)
                {
                    LevelTargetLimbPart limbPart = collidable.GetComponent<LevelTargetLimbPart>();
                    if (limbPart == null)
                    {
                        return;
                    }

                    string boneName = limbPart.BoneName;
                    Rigidbody2D rb = enemy.Ragdoll2D.GetRigidbody(boneName);

                    if (rb == null)
                    {
                        return;
                    }

                    handler = enemy.RigidbodyPairs.GetPreviousRigidbody(rb);

                    LevelTargetLimb parentLimb = FindMainLimb(limbPart.BoneName, enemy);

                    if (parentLimb == null)
                    {
                        CustomDebug.Log($"Part didn't found parent. Name = {limbPart.BoneName}. Enemy = {enemy.name}");
                    }

                    if (parentLimb.IsChoppedOff)
                    {
                        mass = MainLimbMass(parentLimb, enemy);
                    }
                    else
                    {
                        mass = IngameData.Settings.levelTarget.enemyMass;

                        foreach (var limb in enemy.Limbs)
                        {
                            if (limb.IsChoppedOff)
                            {
                                mass -= MainLimbMass(limb, enemy);
                            }
                        }
                    }

                }
                else
                {
                    handler = enemy.StandPreviousFrameRigidbody2D;
                    mass = enemy.StandRigidbody.mass;
                }

                if (handler != null)
                {
                    HandleEnemy(handler, mass);
                }
            }
            else if (collidable.Type == CollidableObjectType.Pet)
            {
                HandlePet();
            }
        }

        #endregion



        #region Methods

        private LevelTargetLimb FindMainLimb(string limbPartName, LevelTarget enemy)
        {
            foreach (var mainLimb in enemy.Limbs)
            {
                foreach (var limbPart in mainLimb.LimbParts)
                {
                    if (limbPart.BoneName.Equals(limbPartName))
                    {
                        return mainLimb;
                    }
                }
            }

            return null;
        }


        private float MainLimbMass(LevelTargetLimb limb, LevelTarget enemy)
        {
            float result = 0f;

            if (limb == null)
            {
                return result;
            }

            if (!enemy.Ragdoll2D.IsActive)
            {
                return result;
            }

            foreach (var limbPartName in limb.PartsBonesNames)
            {
                Rigidbody2D partRigidbody = enemy.Ragdoll2D.GetRigidbody(limbPartName);

                if (partRigidbody != null)
                {
                    result += partRigidbody.mass;
                }
            }

            return result;
        }


        private void HandleEnemy(PreviousFrameRigidbody2D previousRigidbody, float mass)
        {
            float impulsPower = PhysicsCalculation.GetImpulsFromEnemy(sourceLevelObject.PreviousFrameRigidbody2D,
                                                                      previousRigidbody,
                                                                      mass);
            DestroyObject(sourceLevelObject, impulsPower);

#if UNITY_EDITOR
                Logging(impulsPower, sourceLevelObject);
#endif
        }

        private void HandlePhysicalObject(PhysicalLevelObject physicalLevelObject)
        {
            float impulsPower = PhysicsCalculation.GetImpulsMagnitude(sourceLevelObject.PreviousFrameRigidbody2D,
                                                                      physicalLevelObject.PreviousFrameRigidbody2D);

            DestroyObject(physicalLevelObject, impulsPower);
            DestroyObject(sourceLevelObject, impulsPower);

#if UNITY_EDITOR
            Logging(impulsPower, sourceLevelObject);
            Logging(impulsPower, physicalLevelObject);
#endif

        }


        private void HandlePet()
        {
            if (PetsDestroyTypes.Contains(sourceLevelObject.PhysicalData.type))
            {
                float impulsPower = sourceLevelObject.Strength;
                DestroyObject(sourceLevelObject, impulsPower);
#if UNITY_EDITOR
                Logging(impulsPower, sourceLevelObject);
#endif
            }
        }

        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomTriggerEnter2D(GameObject go, Collider2D otherCollider)
        {
            HandleCollision(go, otherCollider.gameObject);
        }

        #endregion



        #region Editor
#if UNITY_EDITOR

        void Logging(float impulsPower, PhysicalLevelObject lvlObject)
        {
            bool isLogging = (impulsPower / lvlObject.Strength) >= minStrengthPercentDamageToLog;

            if (isLogging)
            {
                bool isDestroyed = impulsPower > lvlObject.Strength;
                string prefix = (isDestroyed) ? "Destroyed" : "Damaged";

                string logSourceObjectText = $"{prefix} {lvlObject.name} with impuls {impulsPower}. Velocity: {Math.Round(sourceLevelObject.PreviousFrameRigidbody2D.Velocity.magnitude)}";

                InvokeLogEvent(logSourceObjectText);
            }
        }

#endif
        #endregion
    }
}
