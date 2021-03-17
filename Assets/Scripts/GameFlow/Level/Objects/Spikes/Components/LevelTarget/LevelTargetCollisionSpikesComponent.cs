using UnityEngine;


namespace Drawmasters.Levels
{
    public abstract class LevelTargetCollisionSpikesComponent : PhysicalLevelObjectComponent
    {
        #region Fields

        [SerializeField] private CollisionNotifier hitLevelTargetCollisionNotifier = default;

        #endregion



        #region Methods

        public override void Enable()
        {
            hitLevelTargetCollisionNotifier.OnCustomTriggerEnter2D += CollisionNotifier_OnCustomTriggerEnter2D;
        }


        public override void Disable()
        {
            hitLevelTargetCollisionNotifier.OnCustomTriggerEnter2D -= CollisionNotifier_OnCustomTriggerEnter2D;
        }


        protected virtual void OnLevelTargetCollision(LevelTarget levelTarget) { }


        protected virtual void OnLevelLimbTargetCollision(LevelTarget levelTarget, LevelTargetLimbPart limbPart) { }


        #endregion



        #region Events handlers

        private void CollisionNotifier_OnCustomTriggerEnter2D(GameObject go, Collider2D collision)
        {
            CollidableObject collidableObject = collision.GetComponent<CollidableObject>();

            if (collidableObject == null)
            {
                return;
            }

            if (collidableObject.Type == CollidableObjectType.EnemyTrigger)
            {
                LevelTarget levelTarget = collidableObject.LevelTarget;

                if (levelTarget == null)
                {
                    CustomDebug.Log("Wrong object reference on enemy bone");
                    return;
                }

                OnLevelTargetCollision(levelTarget);


                LevelTargetLimbPart levelTargetLimbPart = collidableObject.GetComponent<LevelTargetLimbPart>();
                OnLevelLimbTargetCollision(levelTarget, levelTargetLimbPart);

            }
        }

        #endregion
    }
}
