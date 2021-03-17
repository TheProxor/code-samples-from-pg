using System.Collections.Generic;

namespace Drawmasters.Levels
{
    public class LimbsVisualDamageHitmastersLevelsLevelTarget : LimbsVisualDamageLevelTargetComponent
    {
        #region Ctor

        public LimbsVisualDamageHitmastersLevelsLevelTarget(List<LevelTargetLimb> _enemyLimbs) 
            : base(_enemyLimbs) { }

        #endregion



        #region Overrided methods

        protected override bool IsColorMatch(Projectile projectile, LevelTarget levelTarget)
            => true;

        #endregion
    }
}

