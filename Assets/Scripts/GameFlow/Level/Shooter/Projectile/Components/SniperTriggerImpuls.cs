namespace Drawmasters.Levels
{
    public class SniperTriggerImpuls : WeaponTriggerImpuls
    {
        #region Lifecycle

        public SniperTriggerImpuls(CollisionNotifier _collisionNotifier) 
            : base(_collisionNotifier)
        {
        }

        #endregion



        #region Abstract implementation

        protected override bool CanTakeCollision(PhysicalLevelObject physicalLevelObject) 
            => physicalLevelObject.PhysicalData.type != PhysicalLevelObjectType.Wood;

        #endregion
    }
}