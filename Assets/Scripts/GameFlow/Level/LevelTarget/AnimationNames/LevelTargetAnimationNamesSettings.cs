using UnityEngine;


namespace Drawmasters.Levels
{
    [CreateAssetMenu(fileName = "LevelTargetAnimationNamesSettings",
                     menuName = NamingUtility.MenuItems.IngameSettings + "LevelTargetAnimationNamesSettings")]
    public class LevelTargetAnimationNamesSettings : ScriptableObject
    {
        #region Fields

        public BossEnemyAnimationNames bossAnimationNames = default;
        public BossEnemyAnimationNames hitmastersBossAnimationNames = default;
        public EnemyAnimationNames enemyAnimationNames = default;
        public EnemyAnimationNames hostageAnimationNames = default;

        #endregion
    }
}
 