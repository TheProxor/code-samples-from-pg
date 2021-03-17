using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Drawmasters.Effects;
using System.Linq;
using Drawmasters.ServiceUtil;


namespace Drawmasters.Levels
{
    public class EnemyBoss : EnemyBossGeneric<AttackedBossSerializableData>
    {
        #region Fields

        public event Action OnStartLeave;
        public event Action<float> OnStartCome;
               
        
        [SerializeField] private Collider2D mainPhysicsCollider = default;        
        
        [Header("Effects")]
        [SerializeField] private IdleEffect[] idleEffects = default;

        // TODO: replace Odin serialization. Low prio cuz of no boss 
        [SerializeField] private (string, ShooterColorType)[] shotBonesInfo = default;
        [SerializeField] private (Transform, string)[] hitFxInfo = default;
        [SerializeField] [Enum(typeof(EffectKeys))] private string destroyFxKey = default;
        
        private object moveTweenId;

        #endregion



        #region Properties               

        public override bool IsHardReturnToInitialState => false;

        public override bool ShouldResetComponentsOnReturn => false;

        public RocketLaunchData[] RocketLaunchData => loadedData.rocketLaunchData;

        public ShooterColorType[] TrajectoryDrawColorTypes { get; private set; }

        public ShooterColorType CurrentStageColorType { get; private set; }

        #endregion



        #region Abstract implementation

        protected override float ObjectsFreeFallDelay =>
            IngameData.Settings.bossLevelSettings.objectFreeFallDelay;

        protected override string SkinWithBoundingBoxesName
            => IngameData.Settings.bossLevelTargetSettings.skinWithBoundingBoxes;

        protected override List<string> AvailableSkins
            => IngameData.Settings.bossLevelTargetSettings.skins.ToList(); // TODO fix

        protected override void OnStateChanged() { }

        #endregion



        #region Overrided methods

        protected override List<LevelTargetComponent> CreateComponents()
        {
            List<LevelTargetComponent> result = new List<LevelTargetComponent>
            {
                new StandLevelTargetComponent(standColliders),
                new HittedLevelTargetComponent(),
                new LimbPartsImpulsLevelTargetComponent(limbsParts),
                new LimbsLiquidLevelTargetComponent(limbs),
                new LimbsVisualDamageLevelTargetComponent(limbs),
                new FixedJointLevelTargetComponent(),
                new ImpulsRagdollApplyLevelTargetComponent(limbs),
                new RagdollEffectsComponent(),
                new BossSounds(),
                new LevelTargetImmortalityComponent(),
                new StageHealthBarLevelTargetComponent(healthBar),
                //new StageLevelTargetComponent(),
                new BossAttackStageLevelTargetComponent(),
                new RagdollLevelTargetComponent(),
                new MotionEffectLevelTargetComponent(),
                new LimbsLaserLevelTargetComponent(),

                // new draw components
                new LevelTargetRocketLaunchComponent(),
                new LevelTargetRocketLaunchSfxComponent(),
                new LevelTargetRocketTrajectoryComponent(),
                new BossIdleFxComponent(idleEffects),
                new BossShotFxComponent(shotBonesInfo),
                new StageLevelTargetFxComponent(hitFxInfo, destroyFxKey),
                new LevelTargetConstHighlightComponent(),
                new BossAnimation(skeletonAnimation, 
                                  IngameData.Settings.levelTargetAnimationNamesSettings.bossAnimationNames)
            };

            return result;
        }


        public override void ApplyRagdoll()
        {
            //no logic
        }


        public override void StartStageChange(StageLevelObjectData data, int stage)
        {
            base.StartStageChange(data, stage);

            bool isFirstStage = stage == 0;

            moveTweenId = Guid.NewGuid();

            BossLevelTargetSettings bossLevelTargetSettings = IngameData.Settings.bossLevelTargetSettings;

            int pathIndex = isFirstStage ? 0 : stage - 1;
            LevelObjectMoveSettings moveSettings = pathIndex >= loadedData.stagesMovement.Length ? default : loadedData.stagesMovement[pathIndex];

            if (moveSettings != null && !moveSettings.path.IsNullOrEmpty())
            {
                DOTween.Complete(moveTweenId);
                StandRigidbody.bodyType = RigidbodyType2D.Kinematic;


                if (!isFirstStage)
                {
                    List<Vector3> reversedPath = new List<Vector3>(moveSettings.path);
                    reversedPath.Reverse();
                    Vector2[] path = reversedPath.ToArray().ToVector2Array();

                    transform.position = path.FirstObject();
                    Physics2D.SyncTransforms();

                    StandRigidbody
                        .DOPath(path, bossLevelTargetSettings.leaveDuration)
                        .SetId(moveTweenId)
                        .SetEase(bossLevelTargetSettings.leaveCurve)
                        .SetDelay(bossLevelTargetSettings.leaveDelay)
                        .OnPlay(() =>
                        {
                            mainPhysicsCollider.isTrigger = true;

                            OnStartLeave?.Invoke();
                        })
                        .OnComplete(() =>
                        {
                            float comeTotalDuration = bossLevelTargetSettings.comeDelay + bossLevelTargetSettings.comeDuration;
                            OnStartCome?.Invoke(comeTotalDuration);

                            transform.position = currentStageData.position;
                            transform.eulerAngles = currentStageData.rotation;

                            NextStageCome(() =>
                            {
                                SetupStageData();
                                mainPhysicsCollider.isTrigger = false;

                                InvokeOnAppearEvent();
                            });
                        });
                }
                else
                {
                    float comeTotalDuration = bossLevelTargetSettings.firstComeDelay + bossLevelTargetSettings.firstComeDuration;
                    OnStartCome?.Invoke(comeTotalDuration);

                    Vector2[] path = moveSettings.path.ToArray().ToVector2Array();
                    transform.position = path.FirstObject();
                    Physics2D.SyncTransforms();

                    StandRigidbody
                        .DOPath(path, bossLevelTargetSettings.firstComeDuration)
                        .SetId(moveTweenId)
                        .SetDelay(bossLevelTargetSettings.firstComeDelay)
                        .OnPlay(() => mainPhysicsCollider.isTrigger = true)
                        .SetEase(bossLevelTargetSettings.comeCurve)
                        .OnComplete(() =>
                        {
                            mainPhysicsCollider.isTrigger = false;

                            InvokeOnAppearEvent();
                        });
                }
            }
            else
            {
                InvokeOnAppearEvent();
            }



            bool isDataExists = LevelStageController.CurrentStageIndex < RocketLaunchData.Length;
            RocketLaunchData.Data[] rocketsData = isDataExists ? RocketLaunchData[LevelStageController.CurrentStageIndex].data : Array.Empty<RocketLaunchData.Data>();
            TrajectoryDrawColorTypes = rocketsData.Select(e => e.colorType).ToArray();

            // TODOL: hotfix first shooter. No need 3 or more
            CurrentStageColorType = GameServices.Instance.LevelControllerService.Target.GetShooters()
                                                                        .Select(e => e.ColorType)
                                                                        .Where(c => !TrajectoryDrawColorTypes.Contains(c))
                                                                        .First();


            void NextStageCome(Action callback)
            {
                LevelObjectMoveSettings comeSettings = loadedData.stagesMovement.SafeGet(pathIndex + 1);

                Vector2[] comePath = comeSettings.path.ToArray().ToVector2Array();
                transform.position = comePath.FirstObject();
                Physics2D.SyncTransforms();

                StandRigidbody
                    .DOPath(comePath, bossLevelTargetSettings.comeDuration)
                    .SetId(moveTweenId)
                    .SetEase(bossLevelTargetSettings.comeCurve)
                    .SetDelay(bossLevelTargetSettings.comeDelay)
                    .OnComplete(() => callback?.Invoke());
            }
        }

        #endregion
    }
}
