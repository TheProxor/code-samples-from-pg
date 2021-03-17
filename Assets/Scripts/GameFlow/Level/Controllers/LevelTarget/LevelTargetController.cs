using System;
using System.Collections.Generic;
using System.Linq;


namespace Drawmasters.Levels
{
    public class LevelTargetController : LevelObjectsFieldController
    {
        #region Fields

        public event Action<LevelTargetType> OnTargetHitted;

        private readonly List<LevelTarget> allTargets = new List<LevelTarget>();
        private readonly List<Shooter> shooters = new List<Shooter>();
        private readonly List<LevelTarget> enemies = new List<LevelTarget>();
        private readonly List<LevelHostage> hostages = new List<LevelHostage>();
        
        
        private IFieldCheckRule fieldCheckRule;

        private ILeftObjectHandler leftObjectHandler;

        #endregion



        #region Public methods

        public override void Initialize()
        {
            base.Initialize();
            
            fieldCheckRule = new CommonFieldCheckRule(IsOutOfGameZone);

            leftObjectHandler = new CommonLeftObjectHandler();
            leftObjectHandler.Initialize();            
        }


        public override void Deinitialize()
        {
            for (int i = allTargets.Count - 1; i > -1; i--)
            {
                Remove(allTargets[i]);
            }
            
            shooters.Clear();
            hostages.Clear();
            enemies.Clear();

            base.Deinitialize();
        }


        public void Add(LevelTarget target)
        {
            string debugMessage = string.Empty;

            if (target.Type == LevelTargetType.Shooter)
            {
                if (target is Shooter shooterTarget)
                {
                    shooters.Add(shooterTarget);
                }
                else
                {
                    debugMessage = $"Error. Type is {target.Type}, but not implement {nameof(Shooter)} class.";
                }
            }
            else if (target.Type == LevelTargetType.Boss)
            {
                if (target is EnemyBossBase bossTarget)
                {
                    enemies.Add(bossTarget);
                }
                else
                {
                    debugMessage = $"Error. Type is {target.Type}, but not implement {nameof(EnemyBossBase)} class.";
                }
            }
            else if (target.Type == LevelTargetType.Hostage)
            {
                if (target is LevelHostage hostageTarget)
                {
                    hostages.Add(hostageTarget);
                }
                else
                {
                    debugMessage = $"Error. Type is {target.Type}, but not implement {nameof(LevelHostage)} class.";
                }
            }
            else if (target.Type == LevelTargetType.Enemy)
            {
                if (target is LevelEnemy enemyTarget)
                {
                    enemies.Add(enemyTarget);
                }
                else
                {
                    debugMessage = $"Error. Type is {target.Type}, but not implement {nameof(LevelEnemy)} class.";
                }
            }
            else
            {
                debugMessage = "Wrong level target added.";
            }

            if (string.IsNullOrEmpty(debugMessage))
            {
                target.OnDefeated += LevelTarget_OnDefeated;
                target.OnHitted += Target_OnHit;
                target.OnGameFinished += Target_OnGameFinished;

                allTargets.Add(target);
            }
            else
            {
                CustomDebug.Log(debugMessage);
            }
        }

        
        public bool IsAllEnemiesHitted()
            => !enemies.Exists(e => !e.IsHitted);


        public bool IsAllEnemiesHitted(ShooterColorType colorType) =>
            GetAliveEnemiesCount(colorType) == 0;


        public int GetEnemiesCount(ShooterColorType colorType) =>
            enemies.FindAll(e => e.ColorType == colorType).Count;


        public List<Shooter> GetShooters() =>
            shooters;


        public EnemyBossBase[] GetBosses() =>
            enemies.Where(i => i.Type == LevelTargetType.Boss).Select(i => i as EnemyBossBase).ToArray();
        


        public void MarkFriendlyTargetsImmortal()
        {
            foreach (var target in allTargets)
            {
                if (target.Type.IsFriendly())
                {
                    target.SetImmortal(true);
                }
            }
        }
        
        #endregion
        
        
        
        #region Private methods
        
        private int GetAliveEnemiesCount(ShooterColorType colorType) =>
            enemies.FindAll(e => e.ColorType == colorType && !e.IsHitted).Count;
        
        private void Remove(LevelTarget target)
        {
            target.OnDefeated -= LevelTarget_OnDefeated;
            target.OnHitted -= Target_OnHit;
            target.OnGameFinished -= Target_OnGameFinished;

            allTargets.Remove(target);

            if (target.Type.IsEnemy())
            {
                enemies.Remove(target);
            }
            else if (target.Type == LevelTargetType.Hostage)
            {
                hostages.Remove(target as LevelHostage);
            }
            else if (target.Type == LevelTargetType.Shooter)
            {
                shooters.Remove(target as Shooter);
            }
        }

        private void IncrementKilledEnemies(LevelTarget hitTarget)
        {
            LevelProgressObserver.TriggerKillEnemy(hitTarget);
        }

        #endregion



        #region Events handlers

        private void LevelTarget_OnDefeated(LevelTarget shotedTarget)
        {
            if (shotedTarget.Type == LevelTargetType.Enemy)
            {
                IncrementKilledEnemies(shotedTarget);
            }

            OnTargetHitted?.Invoke(shotedTarget.Type);
        }


        protected override void OnCheckGameZone()
        {
            for (int i = allTargets.Count - 1; i >= 0; i--)
            {
                bool isOutOfZone = fieldCheckRule.IsMatching(allTargets[i]);

                if (isOutOfZone)
                {
                    leftObjectHandler.HandleLeftTarget(allTargets[i]);
                }
            }
        }


        private void Target_OnHit(LevelTarget killed) => 
            leftObjectHandler.HandleKilledTarget(killed);

        
        private void Target_OnGameFinished(LevelObject finishedTarget)
        {
            Remove(finishedTarget as LevelTarget);
        }

        #endregion
    }
}
