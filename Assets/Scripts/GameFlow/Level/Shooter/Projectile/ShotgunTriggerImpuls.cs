namespace Drawmasters.Levels
{
    public class ShotgunTriggerImpuls : WeaponTriggerImpuls
    {
        #region Lifecycle

        public ShotgunTriggerImpuls(CollisionNotifier _collisionNotifier) 
            : base(_collisionNotifier)
        {
        }

        #endregion



        #region Abstract implementation

        protected override bool CanTakeCollision(PhysicalLevelObject physicalLevelObject) 
            => true;

        #endregion
    }
}

