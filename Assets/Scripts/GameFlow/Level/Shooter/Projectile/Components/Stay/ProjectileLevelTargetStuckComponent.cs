using Drawmasters.ServiceUtil;
using Drawmasters.Utils;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class ProjectileLevelTargetStuckComponent : ProjectileCollisionComponent
    {
        #region Fields

        private readonly LevelTargetController levelTargetController;
        private FixedJoint2D fixedJoint2D;
        private LevelTargetLimb stuckedLimbs;

        #endregion



        #region Class lifecycle

        public ProjectileLevelTargetStuckComponent(CollisionNotifier _projectileCollisionNotifier) : base(_projectileCollisionNotifier)
        {
            levelTargetController = GameServices.Instance.LevelControllerService.Target;
        }

        #endregion



        #region Methods

        public override void Deinitialize()
        {
            if (stuckedLimbs != null)
            {
                stuckedLimbs.OnExploded -= Limbs_OnExploded;
            }
            stuckedLimbs = null;

            RemoveFixedJoint();

            base.Deinitialize();
        }


        protected override void OnTriggerEnter(CollidableObject collidableObject)
        {
            if (collidableObject.LevelTarget != null &&
                levelTargetController.IsAllEnemiesHitted(mainProjectile.ColorType) &&
                mainProjectile.ColorType == collidableObject.LevelTarget.ColorType &&
                CollisionUtility.IsContainsLevelTargetRigidbody(collidableObject))
            {
                Rigidbody2D limbRigidbody2D = CollisionUtility.FindLevelTargetRigidbody(collidableObject);

                if (limbRigidbody2D == null)
                {
                    return;
                }

                if (fixedJoint2D == null)
                {
                    fixedJoint2D = limbRigidbody2D.gameObject.AddComponent<FixedJoint2D>();
                }

                fixedJoint2D.autoConfigureConnectedAnchor = false;
                fixedJoint2D.connectedAnchor = default;
                fixedJoint2D.connectedBody = mainProjectile.MainRigidbody2D;
                fixedJoint2D.enabled = true;

                mainProjectile.StopTrajectoryPath();

                stuckedLimbs = collidableObject.LevelTarget.GetLimb(limbRigidbody2D);

                if (stuckedLimbs != null)
                {
                    stuckedLimbs.OnExploded += Limbs_OnExploded;
                }

                StopCheckCollisions();
            }
        }


        private void Limbs_OnExploded()
        {
            stuckedLimbs.OnExploded -= Limbs_OnExploded;

            RemoveFixedJoint();
        }


        private void RemoveFixedJoint()
        {
            if (fixedJoint2D != null)
            {
                Object.Destroy(fixedJoint2D);
                fixedJoint2D = null;
            }
        }

        protected override void OnCollisionEnter(CollidableObject collidableObject) { }

        #endregion
    }
}
