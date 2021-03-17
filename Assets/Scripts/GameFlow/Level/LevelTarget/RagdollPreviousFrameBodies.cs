using UnityEngine;
using System.Collections.Generic;


namespace Drawmasters.Levels
{
    public class RagdollPreviousFrameBodies : IInitializable, IDeinitializable
    {
        #region Fields

        private readonly LevelTarget levelTarget;

        private Dictionary<Rigidbody2D, PreviousFrameRigidbody2D> previousRigidbodies;

        #endregion



        #region IInitialize

        public void Initialize()
        {
            previousRigidbodies = new Dictionary<Rigidbody2D, PreviousFrameRigidbody2D>();

            RagdollLevelTargetComponent.OnRagdollApplied += RagdollLevelTargetComponent_OnRagdollApplied;
        }

        #endregion



        #region IDeinitializable

        public void Deinitialize()
        {
            foreach (var pair in previousRigidbodies)
            {
                pair.Value.Deinitialize();
            }
            previousRigidbodies.Clear();
            previousRigidbodies = null;

            RagdollLevelTargetComponent.OnRagdollApplied -= RagdollLevelTargetComponent_OnRagdollApplied;
        }

        #endregion



        #region Lifecycle

        public RagdollPreviousFrameBodies (LevelTarget _levelTarget)
        {
            levelTarget = _levelTarget;
        }

        #endregion



        #region Methods

        public PreviousFrameRigidbody2D GetPreviousRigidbody(Rigidbody2D rb)
        {
            previousRigidbodies.TryGetValue(rb, 
                                            out PreviousFrameRigidbody2D result);

            return result;
        }


        private void FillRagdollData()
        {
            bool isRagdollActive = levelTarget.Ragdoll2D.IsActive;            
            if (isRagdollActive)
            {
                foreach (var ragdollRigidbody in levelTarget.Ragdoll2D.RigidbodyArray)
                {
                    if (ragdollRigidbody == null)
                    {
                        continue;
                    }
                    
                    PreviousFrameRigidbody2D previousRb = new PreviousFrameRigidbody2D(ragdollRigidbody);
                    previousRb.Initialize();

                    previousRigidbodies.Add(ragdollRigidbody, previousRb);
                }
            }
        }

        #endregion



        #region Events handlers

        private void RagdollLevelTargetComponent_OnRagdollApplied(LevelTarget _levelTarget)
        {
            if (levelTarget == _levelTarget)
            {   
                FillRagdollData();
            }
        }

        #endregion
    }
}
