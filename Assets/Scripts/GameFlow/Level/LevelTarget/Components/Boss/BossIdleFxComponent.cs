using System;
using System.Collections.Generic;
using Modules.General;
using System.Linq;
using Drawmasters.Effects;


namespace Drawmasters.Levels
{
    public class BossIdleFxComponent : LevelTargetComponent
    {
        #region Fields

        private EnemyBossBase enemyBoss;

        private readonly IdleEffect[] idleEffects;

        #endregion



        #region Class lifecycle

        public BossIdleFxComponent(IdleEffect[] _idleEffects)
        {
            idleEffects = _idleEffects;
        }

        #endregion



        #region Methods

        public override void Enable()
        {
            enemyBoss = levelTarget as EnemyBossBase;

            if (enemyBoss == null)
            {
                CustomDebug.Log($"No impleted logic for simple level target. Can't detect stages and appear callback in {this}");
            }

            foreach (var idleEffect in idleEffects)
            {
                idleEffect.CreateAndPlayEffect();
            }
        }


        public override void Disable()
        {
            foreach (var idleEffect in idleEffects)
            {
                idleEffect.StopEffect();
            }

            Scheduler.Instance.UnscheduleAllMethodForTarget(this);
        }

        #endregion



        #region Events handlers


        #endregion
    }
}
