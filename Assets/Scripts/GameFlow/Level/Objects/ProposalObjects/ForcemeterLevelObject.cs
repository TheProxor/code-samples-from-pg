using System.Collections.Generic;
using Drawmasters.Effects;
using Spine.Unity;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class ForcemeterLevelObject : ComponentLevelObject
    {
        #region Fields

        [SerializeField] private SkeletonAnimation skeletonAnimation = default;

        [SerializeField] private IdleEffect idleEffect = default;
        [SerializeField] private IdleEffect idleLightsEffect = default;
        [SerializeField] private IdleEffect idleWiresEffect = default;
        [SerializeField] private ForcemeterRewardElement[] rewardElements = default;
        [SerializeField] private SpriteRenderer fadeSpriteRenderer = default;
        [SerializeField] private IdleEffect idleSmokeEffect = default;

        [Header("Proposal reward scene")]
        [SerializeField] private IdleEffect[] rewardDataSceneIdleEffects = default;

        private List<ForcemeterComponent> components;

        #endregion



        #region Properties

        public SkeletonAnimation SkeletonAnimation => skeletonAnimation;

        public SpriteRenderer FadeSprite => fadeSpriteRenderer;

        #endregion



        #region Methods

        protected override void InitializeComponents()
        {
            if (components == null)
            {
                components = new List<ForcemeterComponent>
                {
                    new ForcemeterAnimationComponent(),
                    new ForcemeterEffectsComponent(idleEffect, idleLightsEffect, idleWiresEffect, idleSmokeEffect),
                    new ForcemeterRewardDataSceneEffectsComponent(rewardDataSceneIdleEffects),
                    new ForcemeterRewardComponent(ref rewardElements)
                };
            }

            foreach (var component in components)
            {
                component.Initialize(this);
            }
        }


        protected override void EnableComponents()
        {
            foreach (var component in components)
            {
                component.Enable();
            }
        }


        protected override void DisableComponents()
        {
            foreach (var component in components)
            {
                component.Disable();
            }
        }


        public ForcemeterRewardElement GetRewardElement(int iterationIndex)
        {
            ForcemeterRewardElement result = default;

            if (iterationIndex >= 0 && iterationIndex < rewardElements.Length)
            {
                result = rewardElements[iterationIndex];
            }

            return result;
        }

        #endregion
    }
}
