using System;
using Drawmasters.ServiceUtil;
using Modules.Sound;


namespace Drawmasters.Levels
{
    public class EnemySounds : LevelTargetComponent
    {
        #region Fields

        private static readonly string[] LimbsCollisionKeys = { AudioKeys.Ingame.HUMANLITTLEOBJECTS01,
                                                                AudioKeys.Ingame.HUMANLITTLEOBJECTS02,
                                                                AudioKeys.Ingame.HUMANLITTLEOBJECTS03,
                                                                AudioKeys.Ingame.HUMANLITTLEOBJECTS04,
                                                                AudioKeys.Ingame.HUMANLITTLEOBJECTS05
                                                              };
        private Guid aimingSoundGuid;

        #endregion



        #region Overrided methods

        public override void Enable()
        {
            LimbsVisualDamageLevelTargetComponent.OnMonolithCollision += LimbsVisualDamageLevelTargetComponent_OnMonolithCollision;
            ShooterEnemyAiming.OnAimAtEnemy += ShooterEnemyAiming_OnAimAtEnemy;
            levelTarget.OnHitted += LevelTarget_OnHitted;
        }


        public override void Disable()
        {
            LimbsVisualDamageLevelTargetComponent.OnMonolithCollision -= LimbsVisualDamageLevelTargetComponent_OnMonolithCollision;
            ShooterEnemyAiming.OnAimAtEnemy -= ShooterEnemyAiming_OnAimAtEnemy;
            levelTarget.OnHitted -= LevelTarget_OnHitted;
        }

        #endregion



        #region Events handlers

        private void LimbsVisualDamageLevelTargetComponent_OnMonolithCollision(LevelTargetLimb damagedLimb, float damage)
        {
            bool isHandledLimb = damagedLimb.ParentEnemy.Equals(levelTarget);
            if (isHandledLimb)
            {
                bool isEnoughDamage = damage >= IngameData.Settings.levelTarget.limbDamageForCollisionSound;
                if (isEnoughDamage)
                {
                    string soundKey = LimbsCollisionKeys.RandomObject();

                    SoundManager.Instance.PlayOneShot(soundKey, 1f);
                }
            }
        }


        private void ShooterEnemyAiming_OnAimAtEnemy(LevelTarget enemy)
        {
            bool isCharacterAimingAtEnemy = enemy != null && enemy.Equals(levelTarget) && enemy.Type != LevelTargetType.Boss;
            
            if (isCharacterAimingAtEnemy &&
                !SoundManager.Instance.IsActive(aimingSoundGuid))
            {
                bool isBonusLevel = GameServices.Instance.LevelEnvironment.Context.IsBonusLevel;               

                float volume = isBonusLevel ? 0.5f : 1.0f;
                aimingSoundGuid = SoundManager.Instance.PlayOneShot(SoundGroupKeys.RandomScaryEnemiesKey, volume);
            }
        }


        private void LevelTarget_OnHitted(LevelTarget enemy)
        {
            levelTarget.OnHitted -= LevelTarget_OnHitted;
            ShooterEnemyAiming.OnAimAtEnemy -= ShooterEnemyAiming_OnAimAtEnemy;
        }

        #endregion
    }
}

