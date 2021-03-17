using System.Collections.Generic;



namespace Drawmasters.Levels
{
    public class LaserObjectsDestroyComponent : LaserComponent
    {
        #region Fields

        private List<ILaserDestroyable> alreadyHittedObjects;

        #endregion



        #region Methods

        public override void Enable()
        {
            alreadyHittedObjects = alreadyHittedObjects ?? new List<ILaserDestroyable>();

            laser.OnObjectHitted += Laser_OnObjectHitted;
        }


        public override void Disable()
        {
            laser.OnObjectHitted -= Laser_OnObjectHitted;

            alreadyHittedObjects.Clear();
        }

        #endregion



        #region Events handlers

        private void Laser_OnObjectHitted(ILaserDestroyable hittedObject)
        {
            if (hittedObject is EnemyBossBase)
            {
                hittedObject.StartLaserDestroy();
                return;
            }

            if (!alreadyHittedObjects.Contains(hittedObject))
            {
                hittedObject.StartLaserDestroy();
                alreadyHittedObjects.Add(hittedObject);
            }
        }

        #endregion
    }
}
