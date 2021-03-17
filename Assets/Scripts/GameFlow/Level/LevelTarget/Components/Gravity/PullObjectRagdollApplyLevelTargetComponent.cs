using UnityEngine;


namespace Drawmasters.Levels
{
    public class PullObjectRagdollApplyLevelTargetComponent : LevelTargetComponent
    {
        #region Fields

        private LevelObject pulledLevelObject;

        #endregion



        #region Methods

        public override void Enable()
        {
            ProjectilePullThrowComponent.OnObjectPull += ProjectilePullThrowComponent_OnObjectPull;
            ProjectilePullThrowComponent.OnObjectReleased += ProjectilePullThrowComponent_OnObjectReleased;

            foreach (var limb in levelTarget.Limbs)
            {
                limb.OnCollidableObjectHitted += Limb_OnCollidableObjectHitted;
            }

            levelTarget.OnShouldApplyRagdoll += UnsubscribeFromEvents;
        }


        public override void Disable()
        {
            UnsubscribeFromEvents(levelTarget);
        }


        private void UnsubscribeFromEvents(LevelTarget otherLevelTarget)
        {
            otherLevelTarget.OnShouldApplyRagdoll -= UnsubscribeFromEvents;

            foreach (var limb in otherLevelTarget.Limbs)
            {
                limb.OnCollidableObjectHitted -= Limb_OnCollidableObjectHitted;
            }

            ProjectilePullThrowComponent.OnObjectPull -= ProjectilePullThrowComponent_OnObjectPull;
            ProjectilePullThrowComponent.OnObjectReleased -= ProjectilePullThrowComponent_OnObjectReleased;
        }

        #endregion



        #region Events handlers

        private void ProjectilePullThrowComponent_OnObjectPull(LevelObject levelObject, Rigidbody2D arg2)
        {
            pulledLevelObject = levelObject;
        }


        private void ProjectilePullThrowComponent_OnObjectReleased(LevelObject releasedObject)
        {
            if (pulledLevelObject == releasedObject)
            {
                pulledLevelObject = null;
            }
        }


        private void Limb_OnCollidableObjectHitted(CollidableObject collidableObject, LevelTargetLimb limb)
        {
            if (pulledLevelObject != null &&
                (pulledLevelObject == collidableObject.PhysicalLevelObject ||
                pulledLevelObject == collidableObject.LevelTarget))
            {
                levelTarget.ApplyRagdoll();
            }
        }

        #endregion
    }
}
