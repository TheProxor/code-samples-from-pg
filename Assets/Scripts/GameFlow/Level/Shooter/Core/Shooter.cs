using System;
using System.Collections.Generic;
using Drawmasters.Levels.Data;
using Drawmasters.ServiceUtil;
using Drawmasters.ServiceUtil.Interfaces;
using Drawmasters.Helpers;
using Drawmasters.Statistics.Data;
using Spine.Unity;
using Spine.Unity.Examples;
using UnityEngine;
using Sirenix.OdinInspector;


namespace Drawmasters.Levels
{
    public class Shooter : LevelTarget
    {
        #region Fields

        private static readonly HashSet<LevelState> nonControlableStates = new HashSet<LevelState> { LevelState.AllTargetsHitted,
                                                                                                     LevelState.EndPlaying,
                                                                                                     LevelState.OutOfAmmo };

        public static event Action OnStartAiming;

        public event Action<Vector3, Vector2> OnAiming; // start position , direction
        public event Action OnLookSideChanged;

        [SerializeField] private CollisionNotifier projectileLeftCollisionNotifier = default;

        [Enum(typeof(AudioKeys.Ingame))]
        [SerializeField] private string greetingSound = default;
        [SerializeField] private float greetingSoundDelay = default;

        [Required]
        [SerializeField] private ShooterTapZone shooterTapZone = default;

        [Required]
        [SerializeField] private IngameTouchMonitor touchMonitor = default;

        private List<ShooterComponent> shootersComponents = default;

        private ShooterSkinLink currentSkinLink;
        
        private Pet pet;
        
        private Camera gameCamera;

        private ShooterAimingDrawer aimingDrawer;

        private Weapon weapon;

        private Transform aimStartTransform;

        private ShooterLookSide currentLookSide;
        private bool allowShoot;

        private IShotModule shotModule;

        private bool isDisabledAiming;

        private ShooterInput input;

        private ILevelEnvironment levelEnvironment;

        private bool isSceneMode;

        #endregion



        #region Properties

        public int ProjectilesCount => weapon.ProjectilesCount;
        
        public override bool IsHardReturnToInitialState => false;

        public Transform ForcemeterFxRoot => currentSkinLink.ForcemeterFxRoot;

        public Transform ForcemeterHammerFxRoot => currentSkinLink.ForcemeterHammerFxRoot;

        public Transform ProjectileSpawnTransform => AimingAnimation.ProjectileSpawnTransform;

        public Transform ProjectileEffectSpawnTransform => AimingAnimation.ProjectileEffectSpawnTransform;

        public CustomBoneFollower ProjectileEffectBoneFollowewr => AimingAnimation.ProjectileEffectBoneFollower;

        public IngameTouchMonitor IngameTouchMonitor => touchMonitor;

        public override SkeletonRagdoll2D Ragdoll2D => currentSkinLink.Ragdoll2D;

        public override Renderer Renderer => currentSkinLink.Renderer;

        public ShooterLookSide LookingSide
        {
            get => currentLookSide;

            set
            {
                if (value != currentLookSide)
                {
                    currentLookSide = value;
                    transform.eulerAngles = transform.eulerAngles.SetY(currentLookSide == ShooterLookSide.Left ? 180.0f : 0.0f);

                    OnLookSideChanged?.Invoke();
                }
            }
        }

        public override SkeletonAnimation SkeletonAnimation => currentSkinLink.SkeletonAnimation;

        public ShooterAimingAnimation AimingAnimation => currentSkinLink.AimingAnimation;
        
        public SkinSkeletonType SkinSkeletonType => currentSkinLink.SkinSkeletonType;
        
        public SkeletonRenderSeparator SkeletonRenderSeparator => currentSkinLink.SkeletonRenderSeparator;

        public Vector2 CurrentDirection => aimingDrawer.CurrentDirection;

        public override LevelTargetType Type => LevelTargetType.Shooter;

        public override bool AllowPerfects => true;

        public override bool AllowVisualizeDamage => true;

        public CollisionNotifier ProjectileLeftCollisionNotifier => projectileLeftCollisionNotifier;

        public float DrawingPathDistance => aimingDrawer.PathDistance;

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

        public void Initialize(WeaponType weaponType, int projectilesCount)
        {
            isDisabledAiming = false;
            weapon = Content.Management.CreateWeapon(weaponType, AimingAnimation.WeaponRoot, ProjectileSpawnTransform, projectilesCount);
            weapon.SetupShooterColorType(ColorType);

            if (weaponType == WeaponType.HitmastersShotgun ||
                weaponType == WeaponType.HitmastersGravitygun ||
                weaponType == WeaponType.HitmastersSniper ||
                weaponType == WeaponType.HitmasteresPortalgun)
            {
                shotModule = new SimpleShotModule();
            }
            else
            {
                shotModule = new SingleShotModule(input);
            }

            shotModule.Initialize();

            pet = pet ?? new Pet();

            //TODO maxim.ak hotfix
            if (GameServices.Instance.LevelEnvironment.Context.Mode.IsHitmastersLiveOps())
            {
                gameObject.layer = LayerMask.NameToLayer(PhysicsLayers.Shooter);
            }
            else
            {
                gameObject.layer = LayerMask.NameToLayer(PhysicsLayers.Enemy);
            }
        }


        public void SetInputModule(ShooterInput _input) =>
            input = _input;


        public override void PreSetData()
        {
            RefreshSkinLink();

            levelEnvironment = GameServices.Instance.LevelEnvironment;

            base.PreSetData();

        }


        public override void SetData(LevelObjectData data)
        {
            base.SetData(data);

            LookingSide = Mathf.Approximately(data.rotation.y, 180.0f) ? ShooterLookSide.Left : ShooterLookSide.Right;
            currentSkinLink.transform.localPosition = Vector3.zero;
            currentSkinLink.transform.localEulerAngles = Vector3.zero;
        }


        public override void StartGame(GameMode mode, WeaponType weaponType, Transform levelTransform)
        {
            // hack animation also controlled from components, thats enable in base method
            AimingAnimation.SetupShooter(this);
            AimingAnimation.Deinitialize();
            AimingAnimation.Initialize();
            AimingAnimation.ResetAnimation();

            base.StartGame(mode, weaponType, levelTransform);

            LevelContext context = levelEnvironment.Context;
            isSceneMode = context.SceneMode.IsSceneMode();

            gameCamera = IngameCamera.Instance.Camera;

            aimingDrawer = Content.Management.CreateShooterAimDrawer(weapon.Type, this);
            aimingDrawer.Initialize(levelTransform, weapon.Type);
            aimingDrawer.ClearDraw(true);

            AimingAnimation.OnShouldTryFlip += AimingAnimation_OnShouldTryFlip;
            AimingAnimation.OnRootsChanged += AimingAnimation_OnRootsChanged;

            aimStartTransform = AimingAnimation.GetAimStartPosition(weapon.Type);

            weapon.SubscribeToEvents();

            WeaponReload.OnWeaponReloadBegin += WeaponReload_OnWeaponReloadBegin;
            WeaponReload.OnWeaponReloadEnd += WeaponReload_OnWeaponReloadEnd;

            AimingAnimation_OnRootsChanged();

            AllowShoot();

            pet.SetupColorType(ShooterColorType.Default);
            pet.Initialize(this, transform);

            Level.OnLevelStateChanged += Level_OnLevelStateChanged;

            GameServices.Instance.LevelControllerService.BonusLevelController.OnUnstopObjects += BonusLevelController_OnStageBegun;
            GameServices.Instance.PetsService.InvokeController.OnInvokePetForLevel += PetsInvokeController_OnInvokePetForLevel;
        }


        public override void FinishGame()
        {
            weapon.UnsubscribeFromEvents();
            WeaponReload.OnWeaponReloadBegin -= WeaponReload_OnWeaponReloadBegin;
            WeaponReload.OnWeaponReloadEnd -= WeaponReload_OnWeaponReloadEnd;
            weapon.Deinitialize();
            ForbidShoot();

            aimingDrawer.Deinitialize();
            aimingDrawer = null;

            AimingAnimation.OnShouldTryFlip -= AimingAnimation_OnShouldTryFlip;
            AimingAnimation.OnRootsChanged -= AimingAnimation_OnRootsChanged;
            AimingAnimation.Deinitialize();

            Level.OnLevelStateChanged -= Level_OnLevelStateChanged;

            shotModule.Deinitialize();
            
            pet.Deinitialize();

            GameServices.Instance.PetsService.InvokeController.OnInvokePetForLevel -= PetsInvokeController_OnInvokePetForLevel;
            GameServices.Instance.LevelControllerService.BonusLevelController.OnUnstopObjects -= BonusLevelController_OnStageBegun;

            base.FinishGame();
        }


        public override void ApplyRagdoll()
        {
            SkeletonAnimation.Skeleton.SetToSetupPose();
            SkeletonAnimation.AnimationState.SetAnimation(default, IngameData.Settings.shooterAnimationSettings.ragdollAnimationName, false);

            base.ApplyRagdoll();

            MarkHitted();
        }


        private void AllowShoot()
        {
            if (isSceneMode)
            {
                return;
            }

            input.OnResetDraw += Input_OnResetDraw;
            input.OnStartDraw += Input_OnDrawStart;
            input.OnDraw += Input_OnDraw;
            input.OnDrawFinish += Input_OnDrawFinish;

            shotModule.OnShotReady += Shot;

            allowShoot = true;
        }


        private void ForbidShoot()
        {
            input.OnResetDraw -= Input_OnResetDraw;
            input.OnStartDraw -= Input_OnDrawStart;
            input.OnDraw -= Input_OnDraw;
            input.OnDrawFinish -= Input_OnDrawFinish;

            shotModule.OnShotReady -= Shot;

            allowShoot = false;
        }


        private void Shot()
        {
            if (weapon.CanShoot)
            {
                if (aimingDrawer.IsEnoughtTrajectoryDrawed)
                {
                    weapon.Shot(aimingDrawer.CurrentProjectileTrajectory);
                }

                AimingAnimation.SetShotAnimation();
                AimingAnimation.Aim(aimingDrawer.StartDirection);
            }
        }


        public void RefreshSkin(ShooterSkinType type = ShooterSkinType.None)
        {
            RefreshSkinLink(type);
            RefreshVisualColor();
        }


        private void RefreshSkinLink(ShooterSkinType type = ShooterSkinType.None)
        {
            ShooterSkinType skinType = ShooterSkinType.None;
            if (type == ShooterSkinType.None)
            {
                skinType = GameServices.Instance.PlayerStatisticService.PlayerData.CurrentSkin;
            }
            else
            {
                skinType = type;
            }

            var skeletonType = IngameData.Settings.shooterSkinsSettings.FindSkinSkeletonType(skinType);

            ShooterSkinLink link = IngameData.Settings.shooterSkinsSettings.GetSkinLink(skinType);

            bool isLinksEquals = currentSkinLink != null && skeletonType == currentSkinLink.SkinSkeletonType;
            if (link == null || isLinksEquals)
            {
                return;
            }
            

            if (currentSkinLink != null)
            {
                AimingAnimation.Deinitialize();

                AimingAnimation.OnShouldTryFlip -= AimingAnimation_OnShouldTryFlip;
                AimingAnimation.OnRootsChanged -= AimingAnimation_OnRootsChanged;

                Destroy(currentSkinLink.gameObject);
                currentSkinLink = null;
            }

            currentSkinLink = Instantiate(link, transform);

            LevelContext context = GameServices.Instance.LevelEnvironment.Context;
            WeaponType weaponType = context.WeaponType;

            aimStartTransform = AimingAnimation.GetAimStartPosition(weaponType);
            AimingAnimation.SetupShooter(this);

            AimingAnimation.Initialize();
            AimingAnimation.OnShouldTryFlip += AimingAnimation_OnShouldTryFlip;
            AimingAnimation.OnRootsChanged += AimingAnimation_OnRootsChanged;
        }


        public override void FinishStageChange(int stage)
        {
            if (!allowShoot)
            {
                AllowShoot();
            }

            base.FinishStageChange(stage);
        }


        public void SetWeaponProjectiles(int value) =>
            weapon.ProjectilesCount = value;


        public void AddOnShootCallback(Action<Vector2> callback) =>
            weapon.OnShot += callback;


        public void RemoveOnShootCallback(Action<Vector2> callback) =>
            weapon.OnShot -= callback;

        protected override void FinishReturnToInitialState()
        {
            if (!isSceneMode)
            {
                if (aimingDrawer != null && aimingDrawer.IsEnoughtTrajectoryDrawed)
                {
                    AimingAnimation.SetAnimation();
                    AimingAnimation.Aim(aimingDrawer.StartDirection);
                }
            }

            base.FinishReturnToInitialState();

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



        #region Events handlers

        private void BonusLevelController_OnStageBegun(int stage)
        {
            BonusLevelSettings bonusLevelSettings = IngameData.Settings.bonusLevelSettings;

            aimingDrawer.SetClearDuration(bonusLevelSettings.lineClearDuration);
            aimingDrawer.ClearDraw(false);
        }


        private void PetsInvokeController_OnInvokePetForLevel(PetSkinType petSkinType)
        {
            float trajectoryClearDuration = IngameData.Settings.trajectoryDrawSettings.petsTrajectoryClearDuration;

            aimingDrawer.SetClearDuration(trajectoryClearDuration);
            aimingDrawer.ClearDraw(false);

            ForbidShoot();
            input.ForbidInput();
        }


        private void Level_OnLevelStateChanged(LevelState currentState)
        {
            isDisabledAiming = nonControlableStates.Contains(currentState);

            LevelContext context = levelEnvironment.Context;
            isDisabledAiming |= context.Mode.IsHitmastersLiveOps() && currentState == LevelState.Tutorial;

            if (isDisabledAiming)
            {
                ForbidShoot();
                aimingDrawer.ClearDraw(currentState != LevelState.AllTargetsHitted);
            }
            else if (!allowShoot)
            {
                AllowShoot();
            }
        }


        private void Input_OnResetDraw() =>
                aimingDrawer.ClearDraw(true);


        private void Input_OnDrawStart(Vector3 position)
        {
            AimingAnimation.SetAnimation();

            Vector3 touchWorldPosition = gameCamera.ScreenToWorldPoint(position);

            AimingAnimation.Aim(aimingDrawer.CurrentDirection);
            aimingDrawer.StartDrawing(touchWorldPosition);

            OnStartAiming?.Invoke();
        }


        private void Input_OnDraw(Vector3 position, bool wasRectLeft)
        {
            Vector3 touchWorldPosition = gameCamera.ScreenToWorldPoint(position);

            AimingAnimation.Aim(aimingDrawer.CurrentDirection);

            if (wasRectLeft)
            {
                aimingDrawer.DrawShotDirection(aimStartTransform.position, touchWorldPosition);
            }

            OnAiming?.Invoke(aimStartTransform.position, aimingDrawer.CurrentDirection);
        }


        private void Input_OnDrawFinish(bool success, Vector3 position)
        {
            if (success)
            {
                Vector3 touchWorldPosition = gameCamera.ScreenToWorldPoint(position);
                aimingDrawer.EndDrawShotDirection(aimStartTransform.position, touchWorldPosition);
            }
            else
            {
                aimingDrawer.ClearDraw(true);
            }
        }


        private void AimingAnimation_OnShouldTryFlip(ShooterLookSide targetlookingSide) =>
            LookingSide = targetlookingSide;


        private void AimingAnimation_OnRootsChanged()
        {
            weapon.SetupWeaponTransform(AimingAnimation.WeaponRoot);
        }


        private void WeaponReload_OnWeaponReloadBegin()
        {
            if (isDisabledAiming)
            {
                return;
            }

            aimingDrawer.SetReloadVisualEnabled(true);

            if (!allowShoot)
            {
                AllowShoot();
            }
        }


        private void WeaponReload_OnWeaponReloadEnd()
        {
            if (isDisabledAiming)
            {
                return;
            }

            aimingDrawer.SetReloadVisualEnabled(false);
        }

        #endregion



        #region Component Level Object

        protected override List<LevelTargetComponent> CreateComponents()
        {
            List<LevelTargetComponent> result = new List<LevelTargetComponent>();

            LevelContext context = GameServices.Instance.LevelEnvironment.Context;

            bool canApplyRagdoll = context.IsBossLevel || PlayerData.IsUaKillingShootersEnabled;

            result.Add(new LevelTargetImmortalityComponent());

            // ragdoll components
            result.Add(new RagdollEffectsComponent());
            result.Add(new HittedLevelTargetComponent());
            result.Add(new RagdollLevelTargetComponent());
            result.Add(new LimbPartsImpulsLevelTargetComponent(limbsParts));
            result.Add(new EnemySounds());

            if (PlayerData.IsUaKillingShootersEnabled)
            {
                result.Add(new StandLevelTargetComponent(standColliders));
                result.Add(new LevelTargetPhysicalObjectRagdollComponent(projectileLeftCollisionNotifier));
                result.Add(new LimbsVisualDamageLevelTargetComponent(limbs));
            }

            return result;
        }


        protected override void InitializeComponents()
        {
            base.InitializeComponents();

            LevelContext context = levelEnvironment.Context;

            if (shootersComponents == null)
            {
                shootersComponents = new List<ShooterComponent>()
                {
                    new ShotVfx(),
                    new ReloadVfxComponent(),
                    new ShooterAnimation(greetingSound, greetingSoundDelay),
                    new ShooterSkinComponent(),
                    new ShooterForcemeterComponent(),
                    new ShooterFxsComponent(),
                    new ShooterIdleFxsComponent(),
                    new ShooterImmortalityConstComponent(),
                    new ShooterTapZoneDisableComponent(shooterTapZone)
                };
            };

            shootersComponents.ForEach(c => c.Initialize(this));
        }


        protected override void EnableComponents()
        {
            base.EnableComponents();

            shootersComponents.ForEach(c => c.StartGame());
        }


        protected override void DisableComponents()
        {
            base.DisableComponents();

            shootersComponents.ForEach(c => c.Deinitialize());
        }


        #endregion


#if UNITY_EDITOR
        #region Editor methods

        private void OnDrawGizmosSelected()
        {
            Rect shooterRect = IngameData.Settings.shooter.collision.collisionRect;
            shooterRect.position = transform.position
                .SetY(transform.position.y + IngameData.Settings.shooter.collision.collisionRectOffset.y)
                .SetX(transform.position.x + IngameData.Settings.shooter.collision.collisionRectOffset.x);

            Gizmos.color = Color.green.SetA(0.3f);
            Gizmos.DrawCube(shooterRect.center, shooterRect.size);
        }

        #endregion
#endif
    }
}
