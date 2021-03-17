using System;
using DG.Tweening;
using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Modules.General;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class BonusLevelController : ILevelController
    {
        #region Fields
        
        public const int StagesCount = 3;

        public event Action<int> OnStageBegun;
        public event Action<int> OnDecelerationBegin;
        public event Action<int> OnStageEnded;
        public event Action<int> OnStartSpawn;
        public event Action<int> OnStopObjects;
        public event Action<int> OnUnstopObjects;

        public event Action OnBonusLevelFinished;

        private readonly ILevelEnvironment levelEnvironment;

        private LeaveObjectsHelpers leaveObjectsHelpers;
        private BonusLevelSettings levelSettings;

        #endregion



        #region Properties

        public LevelObjectMonolith Monolith { get; private set; }

        public int CurrentStageIndex { get; private set; }

        public float CustomTimeScale { get; private set; } = 1f;

        public bool IsEnabled { get; private set; }

        #endregion
        
        
        
        #region Ctor

        public BonusLevelController(ILevelEnvironment _levelEnvironment)
        {
            levelEnvironment = _levelEnvironment;
        }
        
        #endregion
        
        
        
        #region ILevelController

        public void Initialize()
        {
            levelSettings = IngameData.Settings.bonusLevelSettings;

            CustomTimeScale = 1f;
            CurrentStageIndex = -1;

            IsEnabled = GameServices.Instance.LevelEnvironment.Context.IsBonusLevel;
            
            if (IsEnabled)
            {
                Level.OnLevelStateChanged += Level_OnLevelStateChanged; 
            
                leaveObjectsHelpers = new LeaveObjectsHelpers(Monolith, this);
                leaveObjectsHelpers.Initialize();
                leaveObjectsHelpers.OnAllObjectsLeftZone += OnAllObjectsLeftZone;            
                GameServices.Instance.LevelControllerService.Projectile.OnProjectileLeftGameZone += Projectile_OnProjectileLeftGameZone;
                ProjectileStayApplyComponent.OnStopProjectile += OnShotEnded;
                ProjectileSmashApplyComponent.OnSmashProjectile += OnShotEnded;            
                ShootersInputLevelController.OnStartDraw += ShootersInputLevelController_OnStartDraw;
            }
        }

        
        public void Deinitialize()
        {
            if (IsEnabled)
            {
                Level.OnLevelStateChanged -= Level_OnLevelStateChanged;
                if (leaveObjectsHelpers != null)
                {
                    leaveObjectsHelpers.OnAllObjectsLeftZone -= OnAllObjectsLeftZone;
                    leaveObjectsHelpers.Deinitialize();
                    leaveObjectsHelpers = null;
                }            
                GameServices.Instance.LevelControllerService.Projectile.OnProjectileLeftGameZone -= Projectile_OnProjectileLeftGameZone;            
                ProjectileStayApplyComponent.OnStopProjectile -= OnShotEnded;
                ProjectileSmashApplyComponent.OnSmashProjectile -= OnShotEnded;            
                ShootersInputLevelController.OnStartDraw -= ShootersInputLevelController_OnStartDraw;
            }

            ClearAll();
        }

        #endregion



        #region Public methods

        public void AddMonolith(LevelObjectMonolith levelObjectMonolith)
        {
            Monolith = levelObjectMonolith;
        }

        #endregion
        
        
        
        #region Private methods

        private void StartSpawn()
        {
            if (CurrentStageIndex + 1 < StagesCount)
            {
                CurrentStageIndex++;

                OnStartSpawn?.Invoke(CurrentStageIndex);

                Scheduler.Instance.CallMethodWithDelay(this, 
                    BeginStage, levelSettings.spawnDelay);
            }
            else
            {
                OnBonusLevelFinished?.Invoke();

            }
            
        }


        private void BeginStage()
        {
            CustomTimeScale = 1f;

            //hotfix
            GameServices.Instance.LevelControllerService.Projectile.ReturnToInitialState();
            
            OnStageBegun?.Invoke(CurrentStageIndex);
            
            Scheduler.Instance.CallMethodWithDelay(this, 
                                                   BeginDeceleration, 
                                                   levelSettings.decelerationDelay);
        }


        private void BeginDeceleration()
        {
            OnDecelerationBegin?.Invoke(CurrentStageIndex);

            var tween = DOTween.To(() => 1f, 
                    time => CustomTimeScale = time, 
                    levelSettings.minTimeScale, 
                    levelSettings.timeScaleChangeDuration)
                .SetId(this);

            if (levelSettings.timeScaleCurve != null &&
                levelSettings.timeScaleCurve.keys.Length > 1)
            {
                tween.SetEase(levelSettings.timeScaleCurve);
            }
        }


        private void ClearAll()
        {
            DOTween.Kill(this, true);
            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }
        
        #endregion
        
        
        
        #region Events handlers
        
        private void Level_OnLevelStateChanged(LevelState levelState)
        {
            if (levelState == LevelState.Initialized)
            {
                LevelContext context = levelEnvironment.Context;

                if (context.IsBonusLevel)
                {
                    StartSpawn();
                }
            }
        }


        private void OnAllObjectsLeftZone()
        {
            OnStageEnded?.Invoke(CurrentStageIndex);
            
            ClearAll();
            StartSpawn();
        }
        
        private void Projectile_OnProjectileLeftGameZone()
        {
            CustomTimeScale = 1f;
            
            OnUnstopObjects?.Invoke(CurrentStageIndex);
        }


        private void OnShotEnded(Projectile projectile)
        {
            CustomTimeScale = 1f;
            
            OnUnstopObjects?.Invoke(CurrentStageIndex);
        }
        
        
        private void ShootersInputLevelController_OnStartDraw(Shooter shooter, Vector2 direction)
        {
            ClearAll();
            
            CustomTimeScale = 0f;
            
            OnStopObjects?.Invoke(CurrentStageIndex);
        }

        #endregion
    }
}

