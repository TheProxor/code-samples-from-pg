using System.Collections.Generic;
using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil;
using Spine.Unity;
using Spine.Unity.Examples;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class LevelHostage : LevelTarget, ITeleportable
    {
        [SerializeField] protected SkeletonAnimation skeletonAnimation = default;
        [SerializeField] protected SkeletonRagdoll2D ragdoll2D = default;
        [SerializeField] protected Renderer currentRenderer = default;



        public override LevelTargetType Type => LevelTargetType.Hostage;

        public override bool AllowPerfects => false;

        public override bool AllowVisualizeDamage => true;

        public override SkeletonRagdoll2D Ragdoll2D => ragdoll2D;

        public override SkeletonAnimation SkeletonAnimation => skeletonAnimation;

        public override Renderer Renderer => currentRenderer;


        protected override List<LevelTargetComponent> CreateComponents()
        {
            List<LevelTargetComponent> result = new List<LevelTargetComponent>
            {
                new StandLevelTargetComponent(standColliders),
                new HittedLevelTargetComponent(),
                new RotationRagdollApplyLevelTargetComponent(),
                new RagdollLevelTargetComponent(),
                new LimbPartsImpulsLevelTargetComponent(limbsParts),
                new LimbsLiquidLevelTargetComponent(limbs),
                new FixedJointLevelTargetComponent(),
                new AccelerationRagdollApplier(),
                new ImpulsRagdollApplyLevelTargetComponent(limbs),
                new RagdollEffectsComponent(),
                new EnemySounds(),
                new LevelTargetImmortalityComponent(),
                new LimbsLaserLevelTargetComponent(),
                new HelpBubbleComponent(),
                new LevelTargetColorSkinComponent(),
                new PullLevelTargetEffectComponent()
            };

            LevelContext context = GameServices.Instance.LevelEnvironment.Context;

            if (!context.IsBonusLevel)
            {
                result.Add(new LimbsVisualDamageLevelTargetComponent(limbs));
            }

            result.Add(new HostageAnimation(skeletonAnimation,
                                              IngameData.Settings.levelTargetAnimationNamesSettings.hostageAnimationNames));

            return result;
        }
    }
}
