using UnityEngine;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;


namespace Drawmasters.Levels
{
    public class FixedJointLevelTargetComponent : LevelTargetComponent
    {
        #region Fields

        private FixedJoint2D fixedJoint2D;

        private LevelObject linkedObject;

        #endregion



        #region Methods

        public override void Initialize(LevelTarget levelTargetValue)
        {
            base.Initialize(levelTargetValue);

            levelTarget.OnLinksSet += LevelTarget_OnLinksSet;
            levelTarget.OnShouldApplyRagdoll += LevelTarget_OnShouldApplyRagdoll;
        }


        public override void Enable() { }


        public override void Disable()
        {
            levelTarget.OnLinksSet -= LevelTarget_OnLinksSet;
            levelTarget.OnShouldApplyRagdoll -= LevelTarget_OnShouldApplyRagdoll;

            levelTarget.OnShouldSetImmortal -= LevelTarget_OnShouldSetImmortal;

            BreakJoint();
        }


        private void BreakJoint()
        {
            if (fixedJoint2D != null)
            {
                fixedJoint2D.connectedBody = null;
                Object.Destroy(fixedJoint2D);
                fixedJoint2D = null;
            }
        }

        #endregion



        #region Events handlers

        private void LevelTarget_OnLinksSet(List<LevelObject> linkedObjects)
        {
            if (linkedObjects.First() != null)
            {
                if (fixedJoint2D == null)
                {
                    fixedJoint2D = levelTarget.gameObject.AddComponent<FixedJoint2D>();
                }
                linkedObject = linkedObjects.First();

                fixedJoint2D.connectedBody = linkedObject.Rigidbody2D;
                fixedJoint2D.autoConfigureConnectedAnchor = true;

                levelTarget.OnShouldSetImmortal += LevelTarget_OnShouldSetImmortal;
            }
        }


        private void LevelTarget_OnShouldApplyRagdoll(LevelTarget otherLevelTarget)
        {
            BreakJoint();
            levelTarget.OnShouldApplyRagdoll -= LevelTarget_OnShouldApplyRagdoll;
        }


        private void LevelTarget_OnShouldSetImmortal(bool isImmortal)
        {
            if (linkedObject != null)
            {
                if (isImmortal)
                {
                    linkedObject.StopMoving();
                    linkedObject.Rigidbody2D.simulated = false;
                }
                else
                {
                    linkedObject.Rigidbody2D.simulated = true;
                    linkedObject.StartMoving();
                }
            }
        }

        #endregion
    }
}
