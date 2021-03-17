namespace Drawmasters.Levels
{
    public class StageHealthBarLevelTargetComponent : LevelTargetComponent
    {
        #region Fields

        private readonly BossHealthBar healthBar;

        #endregion



        #region Class lifecycle

        public StageHealthBarLevelTargetComponent(BossHealthBar _healthBar)
        {
            healthBar = _healthBar;
        }


        #endregion



        #region Methods

        public override void Enable()
        {
            if (levelTarget is EnemyBossBase enemyBoss)
            {
                healthBar.Initialize(enemyBoss);
            }
        }


        public override void Disable()
        {
            if (levelTarget is EnemyBossBase)
            {
                healthBar.Deinitialize();
            }
        }

        #endregion
    }
}
