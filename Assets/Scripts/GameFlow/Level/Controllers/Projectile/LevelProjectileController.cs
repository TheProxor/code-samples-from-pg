using System;
using System.Linq;
using System.Collections.Generic;
using Drawmasters.Levels.Inerfaces;
using Drawmasters.Pool;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class LevelProjectileController : LevelObjectsFieldController, IInitialStateReturn
    {
        #region Fields

        public event Action OnProjectileLeftGameZone;

        private readonly List<Projectile> activeProjectiles = new List<Projectile>();
        private readonly List<Projectile> stoppedProjectiles = new List<Projectile>();

        #endregion



        #region Properties

        public bool IsAllProjectilesStopped =>
            stoppedProjectiles.Count == activeProjectiles.Count;


        public ShooterColorType[] ActiveProjectilesColorTypes =>
            activeProjectiles.Select(e => e.ColorType).ToArray();

        #endregion



        #region Methods

        public override void Initialize()
        {
            base.Initialize();

            ProjectileShotComponent.OnShotProduced += AddProjectile;
            StageLevelTargetComponent.OnShouldChangeStage += StageLevelTargetComponent_OnShouldChangeState;

            ProjectileStayApplyComponent.OnStopProjectile += OnStopProjectile;
            ProjectileSmashApplyComponent.OnSmashProjectile += OnStopProjectile;
        }


        public override void Deinitialize()
        {
            ProjectileShotComponent.OnShotProduced -= AddProjectile;
            StageLevelTargetComponent.OnShouldChangeStage -= StageLevelTargetComponent_OnShouldChangeState;

            ProjectileStayApplyComponent.OnStopProjectile -= OnStopProjectile;
            ProjectileSmashApplyComponent.OnSmashProjectile -= OnStopProjectile;

            DestroyActiveProjectiles();

            base.Deinitialize();
        }


        public void ReturnToInitialState() =>
            DestroyActiveProjectiles();


        public bool IsAnyActiveProjectileExists() =>
            activeProjectiles.Count > 0;


        public bool IsAnyActiveProjectileExists(ProjectileType type) =>
            activeProjectiles.Where(e => e.Type == type).Count() > 0;
        

        public bool IsAnyActiveProjectileExists(ShooterColorType colorType) =>
            activeProjectiles.Where(e => e.ColorType == colorType).Count() > 0;


        public bool IsAnyActiveProjectileExists(ProjectileType type, ShooterColorType colorType) =>
            activeProjectiles.Where(e => e.Type == type && e.ColorType == colorType).Count() > 0;


        private void DestroyActiveProjectiles()
        {
            foreach (var projectile in activeProjectiles)
            {
                projectile.Deinitialize();

                projectile.OnShouldDestroy -= Projectile_OnShouldDestroy;

                ComponentPool pool = PoolManager.Instance.GetComponentPool(projectile);

                pool.Push(projectile);
            }

            activeProjectiles.Clear();
            stoppedProjectiles.Clear();
        }


        private void AddProjectile(Projectile projectile)
        {
            projectile.OnShouldDestroy += Projectile_OnShouldDestroy;
            activeProjectiles.Add(projectile);
        }


        private void RemoveProjectile(int index)
        {
            if (index >= activeProjectiles.Count || index < 0)
            {
                return;
            }

            activeProjectiles[index].Deinitialize();

            activeProjectiles[index].OnShouldDestroy -= Projectile_OnShouldDestroy;

            ComponentPool pool = PoolManager.Instance.GetComponentPool(activeProjectiles[index]);

            pool.Push(activeProjectiles[index]);

            stoppedProjectiles.Remove(activeProjectiles[index]);
            activeProjectiles.RemoveAt(index);
        }


        protected override void OnCheckGameZone()
        {
            for (int i = activeProjectiles.Count - 1; i >= 0; i--)
            {
                if (i >= 0 && i < activeProjectiles.Count)
                {
                    Projectile projectile = activeProjectiles[i];

                    if (!projectile.IsNull() &&
                        IsOutOfGameZone(projectile.transform.position))
                    {
                        projectile.Destroy();
                    }
                }
            }
        }

        #endregion



        #region Events handlers

        private void Projectile_OnShouldDestroy(Projectile projectile)
        {
            int foundIndex = activeProjectiles.FindIndex(t => t.Equals(projectile));

            RemoveProjectile(foundIndex);
            OnProjectileLeftGameZone?.Invoke();
        }


        private void StageLevelTargetComponent_OnShouldChangeState(int stage, LevelTarget levelTarget) =>
            DestroyActiveProjectiles();


        private void OnStopProjectile(Projectile projectile)
        {
            stoppedProjectiles.AddExclusive(projectile);
        }

        #endregion
    }
}
