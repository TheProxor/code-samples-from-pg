using Drawmasters.Statistics.Data;
using Drawmasters.ServiceUtil;

namespace Drawmasters.Levels
{
    public static class ColorTypesSolutions
    {
        public static bool IsSafeBossColorType(ShooterColorType bossColorType, ShooterColorType colorTypeToCheck) =>
            bossColorType != colorTypeToCheck;


        public static bool ShouldHighlightLevelTarget(LevelTarget levelTarget, ShooterColorType shooterColorType) =>
           levelTarget.Type == LevelTargetType.Boss ?
            IsSafeBossColorType(levelTarget.ColorType, shooterColorType) : levelTarget.ColorType == shooterColorType;


        public static bool CanCaptureEnemy(ShooterColorType capturedColorType, ShooterColorType startDrawColorType, LevelTargetType targetType) =>
           targetType == LevelTargetType.Boss || capturedColorType == startDrawColorType;


        public static bool CanHitEnemy(Projectile projectile, LevelTarget levelTarget)
        {
            bool result = default;

            if (projectile.Type == ProjectileType.PetRocket)
            {
                result = true;
            }
            else if (projectile.Type != ProjectileType.BossRocket)
            {
                result = levelTarget is EnemyBoss boss ?
                    boss.CurrentStageColorType == projectile.ColorType : projectile.ColorType == levelTarget.ColorType;
            }

            return result;
        }


        public static bool CanHitShooter() => PlayerData.IsUaKillingShootersEnabled;



        public static bool ShouldSmashProjectiles(Projectile first, Projectile second)
        {
            bool isBossRocketCollision = first.Type == ProjectileType.BossRocket || second.Type == ProjectileType.BossRocket;
            bool isDifferentColors = first.ColorType != second.ColorType;

            return isDifferentColors && !isBossRocketCollision;
        }


        public static bool ShouldExplodeProjectiles(Projectile first, Projectile second)
        {
            bool isBossRocketCollision = first.Type == ProjectileType.BossRocket || second.Type == ProjectileType.BossRocket;
            bool isBossRockets = first.Type == ProjectileType.BossRocket && second.Type == ProjectileType.BossRocket;
            bool isSimilarColors = first.ColorType == second.ColorType;

            bool result = true;
            result &= isSimilarColors;
            result &= isBossRocketCollision;
            result &= !isBossRockets;

            return result;
        }

        public static bool ShouldSmashProjectile(Projectile mainProjectile, LevelTarget levelTarget)
        {
            bool result = false;

            if (!levelTarget.IsHitted &&
                levelTarget.Type != LevelTargetType.Shooter &&
                levelTarget.Type != LevelTargetType.Boss)
            {
                result = mainProjectile.ColorType != levelTarget.ColorType;
            }

            if (levelTarget is EnemyBoss boss)
            {
                result = boss.CurrentStageColorType != mainProjectile.ColorType;
            }
            
            return result;
        }


        public static bool ShouldDestroyProjectileOnCollision(LevelTarget levelTarget, ShooterColorType projectileColorType)
        {
            if (levelTarget == null)
            {
                return false;
            }

            bool result = default;

            if (levelTarget is EnemyBoss boss)
            {
                result = boss.CurrentStageColorType == projectileColorType;
            }

            return result;
        }
    }
}
