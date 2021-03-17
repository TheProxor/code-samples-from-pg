using System.Collections.Generic;
using Spine.Unity;
using Spine.Unity.Examples;
﻿using Drawmasters.LevelTargetObject;
using Drawmasters.ServiceUtil;
using UnityEngine;


namespace Drawmasters.Levels
{
    public class LevelEnemy : LevelTarget, ITeleportable
    {
        #region Fields

        [SerializeField] protected SkeletonAnimation skeletonAnimation = default;
        [SerializeField] protected SkeletonRagdoll2D ragdoll2D = default;
        [SerializeField] protected Renderer currentRenderer = default;

        public EnemyBodyType enemyBodyType = EnemyBodyType.None;

        #endregion



        #region Properties

        public override LevelTargetType Type => LevelTargetType.Enemy;

        public override bool AllowPerfects => true;

        public override bool AllowVisualizeDamage => true;

        public override SkeletonRagdoll2D Ragdoll2D => ragdoll2D;

        public override SkeletonAnimation SkeletonAnimation => skeletonAnimation;

        public override Renderer Renderer => currentRenderer;

        public override ShooterColorType ColorType
        {
            get
            {
                ShooterColorType result;
                if (!ShouldLoadColorData)
                {
                    result = default;
                }
                else
                {
                    result = base.ColorType;
                }
                return result;
            }
        }

        //using not "CurrentGameMode.IsHitmastersLiveOps()" because "SetData" was invokes before "StartGame"
        public override bool ShouldLoadColorData => !GameServices.Instance.LevelEnvironment.Context.Mode.IsHitmastersLiveOps();

        #endregion



        #region Methods

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
                new LevelTargetColorSkinComponent(),
                new LevelTargetHighlightComponent(),
                new PullObjectRagdollApplyLevelTargetComponent(),
                new PullLevelTargetEffectComponent(),
                new LoserBubbleComponent()
            };

            bool isNonColorableLevel = GameServices.Instance.LevelEnvironment.Context.Mode != GameMode.Draw;

            // TODO: fix wrong behaviour. We create component only once.
            if (isNonColorableLevel)
            {
                result.Add(new LimbsVisualDamageHitmastersLevelsLevelTarget(limbs));
            }
            else
            {
                result.Add(new LimbsVisualDamageLevelTargetComponent(limbs));
            }

            result.Add(new EnemyAnimation(skeletonAnimation,
                                          IngameData.Settings.levelTargetAnimationNamesSettings.enemyAnimationNames));
            return result;
        }

        protected override void RefreshVisualColor()
        {
            if (CurrentGameMode.IsHitmastersLiveOps())
            {
                return;
            }

            base.RefreshVisualColor();
        }
        #endregion
    }
}
