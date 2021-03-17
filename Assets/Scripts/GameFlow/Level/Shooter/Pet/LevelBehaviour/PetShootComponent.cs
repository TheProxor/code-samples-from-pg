using System;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.Levels;
using Modules.General;
using UnityEngine;


namespace Drawmasters.Pets
{
    public class PetShootComponent : PetComponent
    {
        #region Fields

        public static event Action<Pet, LevelObject> OnShouldPrepareShot;
        public static event Action<Pet, LevelObject> OnShouldDiscardShot;

        public static event Action<Pet, Vector3> OnShooted;

        private readonly PetLevelSettings petLevelSettings;
        private readonly IShotModule shotModule;
        private readonly int layerMask;

        private readonly List<LevelObject> markedLevelObjects;
        private readonly List<LevelObject> aimingLevelObjects;

        private bool isShotOnCooldown;
        private Weapon weapon;

        #endregion



        #region Class lifecycle

        public PetShootComponent()
        {
            petLevelSettings = IngameData.Settings.pets.levelSettings;
            layerMask = LayerMask.GetMask(PhysicsLayers.Enemy);
            shotModule = new AlwaysAvailableShotModule();

            markedLevelObjects = new List<LevelObject>();
            aimingLevelObjects = new List<LevelObject>();
        }

        #endregion



        #region Methods

        public override void Initialize(Pet _pet)
        {
            base.Initialize(_pet);

            PetInvokeComponent.OnShouldInvokePetForLevel += PetInvokeComponent_OnShouldInvokePetForLevel;
        }


        public override void Deinitialize()
        {
            PetInvokeComponent.OnShouldInvokePetForLevel -= PetInvokeComponent_OnShouldInvokePetForLevel;
            PetMoveComponent.OnMoveStarted -= PetMoveComponent_OnMoveStarted;

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);

            shotModule.Deinitialize();
            StopMonitorShots();

            markedLevelObjects.Clear();
            aimingLevelObjects.Clear();

            base.Deinitialize();
        }


        private void StartMonitorShots()
        {
            shotModule.OnShotReady += OnShotModuleReady;
            PetAimComponent.OnAimed += PetAimComponent_OnAimed;
        }


        private void StopMonitorShots()
        {
            shotModule.OnShotReady -= OnShotModuleReady;
            PetAimComponent.OnAimed -= PetAimComponent_OnAimed;
        }


        private bool TryAim(LevelObject levelObject)
        {
            bool canAim = aimingLevelObjects.Count == 0;
            if (canAim)
            {
                aimingLevelObjects.Add(levelObject);

                OnShouldPrepareShot?.Invoke(mainPet, levelObject);
            }

            return canAim;
        }


        private bool TryShot(Vector3 targetPosition)
        {
            if (isShotOnCooldown || !weapon.CanShoot)
            {
                return false;
            }

            var trajectory = new Vector2[] { mainPet.CurrentSkinLink.transform.position,
                                             targetPosition };
            weapon.Shot(trajectory);


            isShotOnCooldown = true;
            Scheduler.Instance.CallMethodWithDelay(this, () => isShotOnCooldown = false, petLevelSettings.attackDelay);

            OnShooted?.Invoke(mainPet, targetPosition);

            return true;
        }

        #endregion



        #region Events handlers

        private void PetInvokeComponent_OnShouldInvokePetForLevel(Pet pet)
        {
            if (pet != mainPet)
            {
                return;
            }

            PetMoveComponent.OnMoveStarted += PetMoveComponent_OnMoveStarted;
        }


        private void PetMoveComponent_OnMoveStarted(Pet anotherPet)
        {
            if (mainPet != anotherPet)
            {
                return;
            }

            PetMoveComponent.OnMoveStarted -= PetMoveComponent_OnMoveStarted;

            Scheduler.Instance.CallMethodWithDelay(this, () =>
            {
                weapon = Content.Management.CreateWeapon(WeaponType.Pet, mainPet.CurrentSkinLink.transform, mainPet.CurrentSkinLink.transform);
                weapon.SetupShooterColorType(mainPet.ColorType);

                shotModule.Initialize();

                StartMonitorShots();
                isShotOnCooldown = false;
            }, petLevelSettings.moveStartedAttackDelay);
        }


        private void PetAimComponent_OnAimed(Pet anotherPet, LevelObject levelObject)
        {
            bool wasShooted = TryShot(levelObject.transform.position);

            if (wasShooted)
            {
                markedLevelObjects.Add(levelObject);

                Scheduler.Instance.CallMethodWithDelay(this, () => markedLevelObjects.Remove(levelObject), petLevelSettings.delayToNextShotInLevelTarget);
            }

            aimingLevelObjects.Remove(levelObject);
        }


        private void OnShotModuleReady()
        {
            if (!weapon.CanShoot)
            {
                return;
            }

            Vector3 center = mainPet.CurrentSkinLink.CurrentPosition;
            float radius = petLevelSettings.attackRadius;
#if UNITY_EDITOR
            CommonUtility.DrawCircle(center, radius, 10, Color.red, true, 0.0f);
#endif
            Collider2D[] colliders2D = Physics2D.OverlapCircleAll(center, radius, layerMask);
            List<LevelTarget> overlapedLevelTargets = colliders2D.Select(e => e.GetComponent<CollidableObject>()).Where(e => e != null &&
                                                                        e.Type == CollidableObjectType.EnemyStand)
                                                             .Select(e => e.LevelTarget)
                                                             .ToList();

            foreach (var levelTarget in overlapedLevelTargets)
            {
                bool shouldAimInLevelTarget = !markedLevelObjects.Contains(levelTarget) &&
                                              levelTarget.Type.IsEnemy() &&
                                              !levelTarget.IsHitted;
                if (shouldAimInLevelTarget)
                {
                    TryAim(levelTarget);
                }
                else
                {
                    aimingLevelObjects.Remove(levelTarget);
                }
            }

            LevelObject[] discardedAimLevelObjects = aimingLevelObjects.Where(e => !overlapedLevelTargets.Exists(o => e == o)).ToArray();
            foreach (var levelObject in discardedAimLevelObjects)
            {
                aimingLevelObjects.Remove(levelObject);

                OnShouldDiscardShot?.Invoke(mainPet, levelObject);
            }
        }

        #endregion
    }
}
